using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace TheTreasurer.TheTreasurerCode.Relics;

public static class ResinRelicRegistry
{
    public sealed record ResinStateSnapshot(
        int Gold,
        int MaxHp,
        int CurrentHp,
        int MaxEnergy,
        int BaseOrbSlotCount,
        int MaxPotionCount,
        IReadOnlyList<SerializableCard> Deck
    );

    private static readonly Dictionary<ulong, HashSet<RelicModel>> ResinRelicsByPlayer = new();

    public static IReadOnlyList<RelicModel> GetVanillaRelicPool()
    {
        return ModelDb.AllRelics
            .Where(r =>
                r.GetType().Namespace?.StartsWith("MegaCrit.Sts2.Core.Models.Relics") == true &&
                r.Pool is SharedRelicPool &&
                (r.Rarity == RelicRarity.Common ||
                 r.Rarity == RelicRarity.Uncommon ||
                 r.Rarity == RelicRarity.Rare ||
                 r.Rarity == RelicRarity.Shop))
            .ToList();
    }

    public static IReadOnlyList<RelicModel> GetVanillaCommonRelicPool()
    {
        return GetVanillaRelicPool()
            .Where(r => r.Rarity == RelicRarity.Common)
            .ToList();
    }

    public static void Register(Player player, RelicModel relic)
    {
        if (!ResinRelicsByPlayer.TryGetValue(player.NetId, out var set))
        {
            set = new HashSet<RelicModel>();
            ResinRelicsByPlayer[player.NetId] = set;
        }

        set.Add(relic);
    }

    public static bool IsResin(RelicModel relic)
    {
        return ResinRelicsByPlayer.Values.Any(set => set.Contains(relic));
    }

    public static ResinStateSnapshot CaptureState(Player player)
    {
        return new ResinStateSnapshot(
            Gold: player.Gold,
            MaxHp: player.Creature.MaxHp,
            CurrentHp: player.Creature.CurrentHp,
            MaxEnergy: player.MaxEnergy,
            BaseOrbSlotCount: player.BaseOrbSlotCount,
            MaxPotionCount: player.MaxPotionCount,
            Deck: player.Deck.Cards.Select(c => c.ToSerializable()).ToList()
        );
    }

    public static async Task RestoreState(Player player, ResinStateSnapshot snapshot)
    {
        player.Creature.SetMaxHpInternal(snapshot.MaxHp);
        player.Creature.SetCurrentHpInternal(snapshot.CurrentHp);
        player.MaxEnergy = snapshot.MaxEnergy;
        player.BaseOrbSlotCount = snapshot.BaseOrbSlotCount;

        if (player.MaxPotionCount < snapshot.MaxPotionCount)
        {
            player.AddToMaxPotionCount(snapshot.MaxPotionCount - player.MaxPotionCount);
        }
        else if (player.MaxPotionCount > snapshot.MaxPotionCount)
        {
            player.SubtractFromMaxPotionCount(player.MaxPotionCount - snapshot.MaxPotionCount);
        }

        await PlayerCmd.SetGold(snapshot.Gold, player);

        player.Deck.Clear(silent: true);
        foreach (var serializableCard in snapshot.Deck)
        {
            CardModel card = player.RunState.LoadCard(serializableCard, player);
            player.Deck.AddInternal(card, silent: true);
        }
    }

    public static IReadOnlyList<RelicModel> GetResinRelics(Player player)
    {
        if (!ResinRelicsByPlayer.TryGetValue(player.NetId, out var set))
        {
            return [];
        }

        return set.Where(r => r.Owner == player).ToList();
    }

    public static async Task<RelicModel?> CreateRandomResinRelic(Player player, bool commonOnly)
    {
        var pool = commonOnly ? GetVanillaCommonRelicPool() : GetVanillaRelicPool();
        if (pool.Count == 0)
        {
            return null;
        }

        var template = player.RunState.Rng.TreasureRoomRelics.NextItem(pool);
        if (template == null)
        {
            return null;
        }

        var snapshot = CaptureState(player);
        var relic = template.ToMutable();
        await RelicCmd.Obtain(relic, player);
        Register(player, relic);
        await RestoreState(player, snapshot);
        return relic;
    }

    public static async Task Cleanup(Player player)
    {
        if (!ResinRelicsByPlayer.TryGetValue(player.NetId, out var set) || set.Count == 0)
        {
            return;
        }

        foreach (var relic in set.ToList())
        {
            if (relic.Owner == player)
            {
                await RelicCmd.Remove(relic);
            }
        }

        ResinRelicsByPlayer.Remove(player.NetId);
    }
}
