using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using HarmonyLib;
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
    public enum RelicCopyScope
    {
        All,
        ResinOnly,
        NonResinOnly
    }

    public sealed record RelicCopyRequest(
        RelicCopyScope Scope = RelicCopyScope.All,
        string? IdOrEntry = null,
        RelicRarity? Rarity = null,
        int? Index = null
    );

    private static readonly System.Reflection.MethodInfo? RelicIconChangedMethod =
        AccessTools.Method(typeof(RelicModel), "RelicIconChanged");
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
    private static readonly Dictionary<ulong, Dictionary<ModelId, int>> ResinRelicIdCountsByPlayer = new();
    private static readonly Dictionary<ulong, Dictionary<RelicModel, ResinEffectDelta>> ResinDeltasByPlayer = new();
    private static readonly Dictionary<ulong, int> LastResinDestroyedRoundByPlayer = new();

    private sealed record ResinEffectDelta(
        int GoldDelta,
        int MaxHpDelta,
        int CurrentHpDelta,
        int MaxEnergyDelta,
        int BaseOrbSlotDelta,
        int MaxPotionDelta,
        IReadOnlyList<SerializableCard> AddedCards,
        IReadOnlyList<SerializableCard> RemovedCards
    );

    public static IReadOnlyList<RelicModel> GetVanillaRelicPool()
    {
        return ModelDb.AllRelics
            .Where(r =>
                r.GetType().Namespace?.StartsWith("MegaCrit.Sts2.Core.Models.Relics") == true &&
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
        if (!ResinRelicIdCountsByPlayer.TryGetValue(player.NetId, out var idCounts))
        {
            idCounts = new Dictionary<ModelId, int>();
            ResinRelicIdCountsByPlayer[player.NetId] = idCounts;
        }

        idCounts.TryGetValue(relic.Id, out var count);
        idCounts[relic.Id] = count + 1;

        MainFile.Logger.Info($"[Resin] Register {relic.Id} for player {player.NetId}");
        RelicIconChangedMethod?.Invoke(relic, null);
    }

    public static bool IsResin(RelicModel relic)
    {
        if (ResinRelicsByPlayer.Values.Any(set => set.Contains(relic)))
        {
            return true;
        }

        return ResinRelicIdCountsByPlayer.Values.Any(idCounts =>
            idCounts.TryGetValue(relic.Id, out var count) && count > 0);
    }

    private static int GetCurrentRound(Player player)
    {
        return player.Creature?.CombatState?.RoundNumber ?? -1;
    }

    public static bool WasResinRelicDestroyedThisTurn(Player player)
    {
        return LastResinDestroyedRoundByPlayer.TryGetValue(player.NetId, out var round) &&
               round == GetCurrentRound(player);
    }

    public static void Unregister(Player player, RelicModel relic)
    {
        if (!ResinRelicsByPlayer.TryGetValue(player.NetId, out var set))
        {
            return;
        }

        set.Remove(relic);
        if (ResinRelicIdCountsByPlayer.TryGetValue(player.NetId, out var idCounts))
        {
            if (idCounts.TryGetValue(relic.Id, out var count))
            {
                if (count <= 1)
                {
                    idCounts.Remove(relic.Id);
                }
                else
                {
                    idCounts[relic.Id] = count - 1;
                }
            }

            if (idCounts.Count == 0)
            {
                ResinRelicIdCountsByPlayer.Remove(player.NetId);
            }
        }

        if (ResinDeltasByPlayer.TryGetValue(player.NetId, out var deltas))
        {
            deltas.Remove(relic);
            if (deltas.Count == 0)
            {
                ResinDeltasByPlayer.Remove(player.NetId);
            }
        }

        if (set.Count == 0)
        {
            ResinRelicsByPlayer.Remove(player.NetId);
            ResinRelicIdCountsByPlayer.Remove(player.NetId);
        }
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

        var live = set.Where(r => r.Owner == player && player.Relics.Contains(r)).ToList();
        ResinRelicsByPlayer[player.NetId] = live.ToHashSet();
        return live;
    }

    public static IReadOnlyList<RelicModel> QueryResinRelics(Player player, string? idOrEntry = null, RelicRarity? rarity = null)
    {
        var relics = GetResinRelics(player).AsEnumerable();

        if (!string.IsNullOrWhiteSpace(idOrEntry))
        {
            var key = idOrEntry.Trim().ToLowerInvariant();
            relics = relics.Where(r =>
                r.Id.ToString().ToLowerInvariant().Contains(key) ||
                r.Id.Entry.ToLowerInvariant().Contains(key));
        }

        if (rarity.HasValue)
        {
            relics = relics.Where(r => r.Rarity == rarity.Value);
        }

        return relics.ToList();
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

        return await CreateResinFromTemplate(player, template);
    }

    public static async Task<RelicModel?> CreateResinRelicByRarity(Player player, RelicRarity rarity)
    {
        var pool = GetVanillaRelicPool().Where(r => r.Rarity == rarity).ToList();
        if (pool.Count == 0)
        {
            return null;
        }

        var template = player.RunState.Rng.TreasureRoomRelics.NextItem(pool);
        if (template == null)
        {
            return null;
        }

        return await CreateResinFromTemplate(player, template);
    }

    public static async Task<RelicModel?> CreateRandomShopResinRelic(Player player)
    {
        return await CreateResinRelicByRarity(player, RelicRarity.Shop);
    }

    public static async Task<RelicModel?> CreateResinRelicByName(Player player, string idOrEntry)
    {
        var key = idOrEntry.Trim().ToLowerInvariant();
        var template = GetVanillaRelicPool().FirstOrDefault(r =>
            r.Id.ToString().ToLowerInvariant().Contains(key) ||
            r.Id.Entry.ToLowerInvariant().Contains(key));
        if (template == null)
        {
            return null;
        }

        return await CreateResinFromTemplate(player, template);
    }

    public static async Task<RelicModel?> CreateResinRelicByIndex(Player player, int index)
    {
        var pool = GetVanillaRelicPool();
        if (index < 0 || index >= pool.Count)
        {
            return null;
        }

        return await CreateResinFromTemplate(player, pool[index]);
    }

    public static RelicModel ResolveTemplateForRelic(RelicModel source)
    {
        return GetVanillaRelicPool().FirstOrDefault(v => v.Id == source.Id)
               ?? source.CanonicalInstance as RelicModel
               ?? source;
    }

    public static async Task<RelicModel?> CopyRelic(Player player, RelicCopyRequest request)
    {
        var candidates = player.Relics.AsEnumerable();
        candidates = request.Scope switch
        {
            RelicCopyScope.ResinOnly => candidates.Where(IsResin),
            RelicCopyScope.NonResinOnly => candidates.Where(r => !IsResin(r)),
            _ => candidates
        };

        if (!string.IsNullOrWhiteSpace(request.IdOrEntry))
        {
            var key = request.IdOrEntry.Trim().ToLowerInvariant();
            candidates = candidates.Where(r =>
                r.Id.ToString().ToLowerInvariant().Contains(key) ||
                r.Id.Entry.ToLowerInvariant().Contains(key));
        }

        if (request.Rarity.HasValue)
        {
            candidates = candidates.Where(r => r.Rarity == request.Rarity.Value);
        }

        var list = candidates.ToList();
        if (list.Count == 0)
        {
            return null;
        }

        RelicModel? source;
        if (request.Index.HasValue)
        {
            var idx = request.Index.Value;
            if (idx < 0 || idx >= list.Count)
            {
                return null;
            }

            source = list[idx];
        }
        else
        {
            source = player.RunState.Rng.TreasureRoomRelics.NextItem(list);
        }

        if (source == null)
        {
            return null;
        }

        return await CreateResinFromTemplate(player, ResolveTemplateForRelic(source));
    }

    public static async Task<RelicModel?> CreateRandomResinFromOwnedNonResin(Player player)
    {
        return await CopyRelic(player, new RelicCopyRequest(Scope: RelicCopyScope.NonResinOnly));
    }

    public static async Task<bool> DestroyResinRelic(Player player, RelicModel relic)
    {
        var liveRelic = player.Relics.FirstOrDefault(r => ReferenceEquals(r, relic));
        if (liveRelic == null || !IsResin(liveRelic))
        {
            return false;
        }

        liveRelic.Flash();
        await RelicCmd.Remove(liveRelic);
        if (TryGetDelta(player, liveRelic, out var delta) && delta != null)
        {
            await RevertDelta(player, delta);
        }
        Unregister(player, liveRelic);
        LastResinDestroyedRoundByPlayer[player.NetId] = GetCurrentRound(player);
        return true;
    }

    public static async Task<bool> DestroyRandomResinRelic(Player player)
    {
        var resinRelics = GetResinRelics(player);
        var target = player.RunState.Rng.TreasureRoomRelics.NextItem(resinRelics);
        if (target == null)
        {
            return false;
        }

        return await DestroyResinRelic(player, target);
    }

    public static async Task<bool> DestroyResinRelicByName(Player player, string idOrEntry)
    {
        var target = QueryResinRelics(player, idOrEntry: idOrEntry).FirstOrDefault();
        if (target == null)
        {
            return false;
        }

        return await DestroyResinRelic(player, target);
    }

    public static async Task<bool> DestroyResinRelicByIndex(Player player, int index)
    {
        var relics = GetResinRelics(player);
        if (index < 0 || index >= relics.Count)
        {
            return false;
        }

        return await DestroyResinRelic(player, relics[index]);
    }

    public static async Task Cleanup(Player player)
    {
        if (!ResinRelicsByPlayer.TryGetValue(player.NetId, out var set) || set.Count == 0)
        {
            return;
        }

        var ownedResinRelics = GetResinRelics(player);
        if (ownedResinRelics.Count == 0)
        {
            ResinRelicsByPlayer.Remove(player.NetId);
            LastResinDestroyedRoundByPlayer.Remove(player.NetId);
            return;
        }

        RelicModel? savedByKeepsake = null;
        if (player.Relics.Any(r => r is Keepsake))
        {
            savedByKeepsake = player.RunState.Rng.TreasureRoomRelics.NextItem(ownedResinRelics);
        }

        foreach (var relic in ownedResinRelics)
        {
            if (savedByKeepsake != null && relic == savedByKeepsake)
            {
                continue;
            }

            _ = await DestroyResinRelic(player, relic);
        }

        var remaining = GetResinRelics(player);
        if (remaining.Count == 0)
        {
            ResinRelicsByPlayer.Remove(player.NetId);
            LastResinDestroyedRoundByPlayer.Remove(player.NetId);
            return;
        }

        ResinRelicsByPlayer[player.NetId] = remaining.ToHashSet();
    }

    public static async Task<RelicModel?> CreateResinFromTemplate(Player player, RelicModel template)
    {
        var before = CaptureState(player);
        var relic = template.ToMutable();
        await RelicCmd.Obtain(relic, player);
        Register(player, relic);
        var after = CaptureState(player);
        StoreDelta(player, relic, ComputeDelta(before, after));
        await SyncCombatCardsFromDeck(player);
        return relic;
    }

    private static async Task SyncCombatCardsFromDeck(Player player)
    {
        var combatState = player.Creature?.CombatState;
        if (combatState == null)
        {
            return;
        }

        var combatCards = player.PlayerCombatState.AllCards.ToList();
        if (combatCards.Count == 0)
        {
            return;
        }

        var deckGroups = player.Deck.Cards.GroupBy(c => c.Id).ToDictionary(g => g.Key, g => g.OrderBy(c => c.CurrentUpgradeLevel).ToList());
        var combatGroups = combatCards.GroupBy(c => c.Id).ToDictionary(g => g.Key, g => g.OrderBy(c => c.CurrentUpgradeLevel).ToList());

        foreach (var (cardId, deckList) in deckGroups)
        {
            if (!combatGroups.TryGetValue(cardId, out var combatList))
            {
                continue;
            }

            var pairCount = System.Math.Min(deckList.Count, combatList.Count);
            for (var i = 0; i < pairCount; i++)
            {
                var source = deckList[i];
                var target = combatList[i];

                while (target.CurrentUpgradeLevel < source.CurrentUpgradeLevel)
                {
                    target.UpgradeInternal();
                }

                var sourceEnchantment = source.Enchantment;
                var targetEnchantment = target.Enchantment;
                if (sourceEnchantment == null && targetEnchantment != null)
                {
                    CardCmd.ClearEnchantment(target);
                    continue;
                }

                if (sourceEnchantment == null)
                {
                    continue;
                }

                if (targetEnchantment == null ||
                    targetEnchantment.GetType() != sourceEnchantment.GetType() ||
                    targetEnchantment.Amount != sourceEnchantment.Amount)
                {
                    CardCmd.ClearEnchantment(target);
                    var canonicalSource = sourceEnchantment.IsCanonical
                        ? sourceEnchantment
                        : sourceEnchantment.CanonicalInstance ?? sourceEnchantment;
                    var clonedEnchantment = canonicalSource.ToMutable();
                    CardCmd.Enchant(clonedEnchantment, target, sourceEnchantment.Amount);
                }
            }
        }

        MainFile.Logger.Info($"[Resin] Synced combat cards from deck for player {player.NetId}");
        await Task.CompletedTask;
    }

    private static void StoreDelta(Player player, RelicModel relic, ResinEffectDelta delta)
    {
        if (!ResinDeltasByPlayer.TryGetValue(player.NetId, out var perRelic))
        {
            perRelic = new Dictionary<RelicModel, ResinEffectDelta>();
            ResinDeltasByPlayer[player.NetId] = perRelic;
        }

        perRelic[relic] = delta;
    }

    private static bool TryGetDelta(Player player, RelicModel relic, out ResinEffectDelta? delta)
    {
        delta = null;
        return ResinDeltasByPlayer.TryGetValue(player.NetId, out var perRelic) &&
               perRelic.TryGetValue(relic, out delta);
    }

    private static Dictionary<string, Queue<SerializableCard>> BuildCardMultiset(IEnumerable<SerializableCard> cards)
    {
        var map = new Dictionary<string, Queue<SerializableCard>>();
        foreach (var card in cards)
        {
            var key = JsonSerializer.Serialize(card);
            if (!map.TryGetValue(key, out var queue))
            {
                queue = new Queue<SerializableCard>();
                map[key] = queue;
            }

            queue.Enqueue(card);
        }

        return map;
    }

    private static ResinEffectDelta ComputeDelta(ResinStateSnapshot before, ResinStateSnapshot after)
    {
        var beforeMap = BuildCardMultiset(before.Deck);
        var afterMap = BuildCardMultiset(after.Deck);

        var added = new List<SerializableCard>();
        var removed = new List<SerializableCard>();

        foreach (var (key, cardsAfter) in afterMap)
        {
            beforeMap.TryGetValue(key, out var cardsBefore);
            var beforeCount = cardsBefore?.Count ?? 0;
            var addCount = cardsAfter.Count - beforeCount;
            for (var i = 0; i < addCount; i++)
            {
                added.Add(cardsAfter.ElementAt(i));
            }
        }

        foreach (var (key, cardsBefore) in beforeMap)
        {
            afterMap.TryGetValue(key, out var cardsAfter);
            var afterCount = cardsAfter?.Count ?? 0;
            var removeCount = cardsBefore.Count - afterCount;
            for (var i = 0; i < removeCount; i++)
            {
                removed.Add(cardsBefore.ElementAt(i));
            }
        }

        return new ResinEffectDelta(
            GoldDelta: after.Gold - before.Gold,
            MaxHpDelta: after.MaxHp - before.MaxHp,
            CurrentHpDelta: after.CurrentHp - before.CurrentHp,
            MaxEnergyDelta: after.MaxEnergy - before.MaxEnergy,
            BaseOrbSlotDelta: after.BaseOrbSlotCount - before.BaseOrbSlotCount,
            MaxPotionDelta: after.MaxPotionCount - before.MaxPotionCount,
            AddedCards: added,
            RemovedCards: removed
        );
    }

    private static async Task RevertDelta(Player player, ResinEffectDelta delta)
    {
        if (delta.MaxHpDelta != 0)
        {
            player.Creature.SetMaxHpInternal(player.Creature.MaxHp - delta.MaxHpDelta);
        }

        if (delta.CurrentHpDelta != 0)
        {
            var targetHp = player.Creature.CurrentHp - delta.CurrentHpDelta;
            if (targetHp > player.Creature.MaxHp)
            {
                targetHp = player.Creature.MaxHp;
            }

            if (targetHp < 1)
            {
                targetHp = 1;
            }

            player.Creature.SetCurrentHpInternal(targetHp);
        }

        if (delta.MaxEnergyDelta != 0)
        {
            player.MaxEnergy -= delta.MaxEnergyDelta;
        }

        if (delta.BaseOrbSlotDelta != 0)
        {
            player.BaseOrbSlotCount -= delta.BaseOrbSlotDelta;
        }

        if (delta.MaxPotionDelta > 0)
        {
            player.SubtractFromMaxPotionCount(delta.MaxPotionDelta);
        }
        else if (delta.MaxPotionDelta < 0)
        {
            player.AddToMaxPotionCount(-delta.MaxPotionDelta);
        }

        if (delta.GoldDelta != 0)
        {
            await PlayerCmd.SetGold(player.Gold - delta.GoldDelta, player);
        }

        foreach (var added in delta.AddedCards)
        {
            var addedCard = player.Deck.Cards.FirstOrDefault(c => JsonSerializer.Serialize(c.ToSerializable()) == JsonSerializer.Serialize(added));
            if (addedCard != null)
            {
                player.Deck.RemoveInternal(addedCard, silent: true);
            }
        }

        foreach (var removed in delta.RemovedCards)
        {
            var card = player.RunState.LoadCard(removed, player);
            player.Deck.AddInternal(card, silent: true);
        }
    }
}

