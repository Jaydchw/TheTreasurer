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
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves.Runs;
using TheTreasurer.TheTreasurerCode.Patches;

namespace TheTreasurer.TheTreasurerCode.Relics;

public static class ResinRelicRegistry
{
    private static readonly string[] CharacterRelicMarkers =
    [
        "IRONCLAD",
        "SILENT",
        "DEFECT",
        "REGENT",
        "NECROBINDER",
        "WATCHER",
        "RED",
        "GREEN",
        "BLUE",
        "PURPLE"
    ];

    public enum RelicCopyScope
    {
        All,
        ResinOnly,
        NonResinOnly
    }

    public enum ResinTemplateSource
    {
        Vanilla,
        VanillaIncludingCharacterSpecific
    }

    public enum ResinDestroySelector
    {
        Random,
        ByName,
        ByIndex,
        ByInstance
    }

    public sealed record RelicCopyRequest(
        RelicCopyScope Scope = RelicCopyScope.All,
        string? IdOrEntry = null,
        RelicRarity? Rarity = null,
        int? Index = null
    );

    public sealed record ResinQueryRequest(
        string? IdOrEntry = null,
        RelicRarity? Rarity = null
    );

    public sealed record ResinCreateRequest(
        ResinTemplateSource Source = ResinTemplateSource.Vanilla,
        string? IdOrEntry = null,
        RelicRarity? Rarity = null,
        int? Index = null,
        RelicModel? Template = null
    );

    public sealed record ResinDestroyRequest(
        ResinDestroySelector Selector = ResinDestroySelector.Random,
        string? IdOrEntry = null,
        int? Index = null,
        RelicModel? Relic = null
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

    public static IReadOnlyList<RelicModel> GetVanillaRelicPool(bool includeCharacterSpecific = false)
    {
        return ModelDb.AllRelics
            .Where(r =>
                r.GetType().Namespace?.StartsWith("MegaCrit.Sts2.Core.Models.Relics") == true &&
                (includeCharacterSpecific || !IsCharacterSpecificRelic(r)) &&
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

    private static bool IsCharacterSpecificRelic(RelicModel relic)
    {
        var idText = relic.Id.ToString().ToUpperInvariant();
        if (CharacterRelicMarkers.Any(idText.Contains))
        {
            return true;
        }

        var characterMarker = TryGetCharacterMarker(relic);
        if (!string.IsNullOrWhiteSpace(characterMarker))
        {
            var normalized = characterMarker.Trim().ToUpperInvariant();
            if (normalized != "SHARED" && normalized != "COLORLESS" && normalized != "NONE")
            {
                return true;
            }
        }

        var poolMarker = TryGetPoolMarker(relic);
        if (!string.IsNullOrWhiteSpace(poolMarker))
        {
            var normalized = poolMarker.Trim().ToUpperInvariant();
            if (CharacterRelicMarkers.Any(normalized.Contains))
            {
                return true;
            }
        }

        return false;
    }

    private static string? TryGetCharacterMarker(RelicModel relic)
    {
        var relicType = relic.GetType();
        var propertyNames = new[] { "Character", "CharacterId", "OwnerCharacter", "CharacterType", "CharacterName" };
        foreach (var propertyName in propertyNames)
        {
            var property = AccessTools.Property(relicType, propertyName);
            if (property?.GetGetMethod() == null)
            {
                continue;
            }

            var value = property.GetValue(relic);
            if (value != null)
            {
                return value.ToString();
            }
        }

        var fieldNames = new[] { "Character", "CharacterId", "OwnerCharacter", "CharacterType", "CharacterName" };
        foreach (var fieldName in fieldNames)
        {
            var field = AccessTools.Field(relicType, fieldName);
            if (field == null)
            {
                continue;
            }

            var value = field.GetValue(relic);
            if (value != null)
            {
                return value.ToString();
            }
        }

        return null;
    }

    private static string? TryGetPoolMarker(RelicModel relic)
    {
        var relicType = relic.GetType();
        foreach (var attribute in relicType.GetCustomAttributes(inherit: true))
        {
            var attrType = attribute.GetType();
            if (!attrType.Name.Contains("Pool"))
            {
                continue;
            }

            var poolTypeProperty = AccessTools.Property(attrType, "PoolType");
            if (poolTypeProperty?.GetGetMethod() != null)
            {
                if (poolTypeProperty.GetValue(attribute) is System.Type poolType)
                {
                    return poolType.Name;
                }
            }

            var ctorArg = AccessTools.Field(attrType, "<PoolType>k__BackingField")?.GetValue(attribute) as System.Type;
            if (ctorArg != null)
            {
                return ctorArg.Name;
            }
        }

        return null;
    }

    public static void Register(Player player, RelicModel relic)
    {
        if (!ResinRelicsByPlayer.TryGetValue(player.NetId, out var set))
        {
            set = new HashSet<RelicModel>();
            ResinRelicsByPlayer[player.NetId] = set;
        }

        set.Add(relic);
        MainFile.Logger.Info($"[Resin] Register {relic.Id} for player {player.NetId}");
        RelicIconChangedMethod?.Invoke(relic, null);
        ResinRelicVisualPatch.ForceRefreshAllRelics();
    }

    public static bool IsResin(RelicModel relic)
    {
        return ResinRelicsByPlayer.Values.Any(set => set.Contains(relic));
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
        }

        ResinRelicVisualPatch.ForceRefreshAllRelics();
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

    public static IReadOnlyList<RelicModel> QueryResinRelics(Player player, ResinQueryRequest request)
    {
        var relics = GetResinRelics(player).AsEnumerable();

        if (!string.IsNullOrWhiteSpace(request.IdOrEntry))
        {
            var key = request.IdOrEntry.Trim().ToLowerInvariant();
            relics = relics.Where(r =>
                r.Id.ToString().ToLowerInvariant().Contains(key) ||
                r.Id.Entry.ToLowerInvariant().Contains(key));
        }

        if (request.Rarity.HasValue)
        {
            relics = relics.Where(r => r.Rarity == request.Rarity.Value);
        }

        return relics.ToList();
    }

    public static async Task<RelicModel?> CreateResinRelic(Player player, ResinCreateRequest request)
    {
        if (request.Template != null)
        {
            return await CreateResinFromTemplate(player, request.Template);
        }

        var pool = request.Source == ResinTemplateSource.VanillaIncludingCharacterSpecific
            ? GetVanillaRelicPool(includeCharacterSpecific: true)
            : GetVanillaRelicPool(includeCharacterSpecific: false);

        if (request.Rarity.HasValue)
        {
            pool = pool.Where(r => r.Rarity == request.Rarity.Value).ToList();
        }

        if (!string.IsNullOrWhiteSpace(request.IdOrEntry))
        {
            var key = request.IdOrEntry.Trim().ToLowerInvariant();
            var template = pool.FirstOrDefault(r =>
                r.Id.ToString().ToLowerInvariant().Contains(key) ||
                r.Id.Entry.ToLowerInvariant().Contains(key));
            if (template == null)
            {
                return null;
            }

            return await CreateResinFromTemplate(player, template);
        }

        if (request.Index.HasValue)
        {
            var idx = request.Index.Value;
            if (idx < 0 || idx >= pool.Count)
            {
                return null;
            }

            return await CreateResinFromTemplate(player, pool[idx]);
        }

        if (pool.Count == 0)
        {
            return null;
        }

        var randomTemplate = player.RunState.Rng.TreasureRoomRelics.NextItem(pool);
        if (randomTemplate == null)
        {
            return null;
        }

        return await CreateResinFromTemplate(player, randomTemplate);
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

    public static async Task<bool> DestroyResinRelic(Player player, ResinDestroyRequest request)
    {
        return request.Selector switch
        {
            ResinDestroySelector.Random => await DestroyRandom(player),
            ResinDestroySelector.ByName when !string.IsNullOrWhiteSpace(request.IdOrEntry) =>
                await DestroyByName(player, request.IdOrEntry),
            ResinDestroySelector.ByIndex when request.Index.HasValue =>
                await DestroyByIndex(player, request.Index.Value),
            ResinDestroySelector.ByInstance when request.Relic != null =>
                await DestroyResinRelic(player, request.Relic),
            _ => false
        };
    }

    private static async Task<bool> DestroyRandom(Player player)
    {
        var resinRelics = GetResinRelics(player);
        var target = player.RunState.Rng.TreasureRoomRelics.NextItem(resinRelics);
        if (target == null)
        {
            return false;
        }

        return await DestroyResinRelic(player, target);
    }

    private static async Task<bool> DestroyByName(Player player, string idOrEntry)
    {
        var target = QueryResinRelics(player, new ResinQueryRequest(IdOrEntry: idOrEntry)).FirstOrDefault();
        if (target == null)
        {
            return false;
        }

        return await DestroyResinRelic(player, target);
    }

    private static async Task<bool> DestroyByIndex(Player player, int index)
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
        await TryRunStartOfCombatHooks(player, relic);
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

        if (delta.MaxPotionDelta != 0)
        {
            var targetMaxPotion = player.MaxPotionCount - delta.MaxPotionDelta;
            ForceSetMaxPotionCount(player, targetMaxPotion);
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

    private static void ForceSetMaxPotionCount(Player player, int targetMaxPotion)
    {
        if (targetMaxPotion < 0)
        {
            targetMaxPotion = 0;
        }

        if (player.MaxPotionCount < targetMaxPotion)
        {
            player.AddToMaxPotionCount(targetMaxPotion - player.MaxPotionCount);
            return;
        }

        if (player.MaxPotionCount > targetMaxPotion)
        {
            player.SubtractFromMaxPotionCount(player.MaxPotionCount - targetMaxPotion);
        }
    }

    private static async Task TryRunStartOfCombatHooks(Player player, RelicModel relic)
    {
        var combatState = player.Creature?.CombatState;
        if (combatState == null)
        {
            return;
        }

        var relicType = relic.GetType();
        var noArgHooks = new[]
        {
            "AtCombatStart", "OnCombatStart", "BeforeCombatStart", "AfterCombatStart",
            "OnPickup", "OnObtain", "AfterObtained", "AfterRelicObtained"
        };
        foreach (var methodName in noArgHooks)
        {
            var method = AccessTools.Method(relicType, methodName, []);
            if (method == null)
            {
                continue;
            }

            var result = method.Invoke(relic, null);
            if (result is Task task)
            {
                await task;
            }
        }

        var turnStart = AccessTools.Method(relicType, "AfterSideTurnStart", [typeof(CombatSide), typeof(ICombatState)]);
        if (turnStart != null)
        {
            var result = turnStart.Invoke(relic, [player.Creature.Side, combatState]);
            if (result is Task task)
            {
                await task;
            }
        }

        var onRelicsChangedHooks = new[] { "OnRelicsChanged", "NotifyRelicsChanged", "RefreshRelics", "RecalculateRelicEffects" };
        foreach (var hookName in onRelicsChangedHooks)
        {
            var method = AccessTools.Method(player.GetType(), hookName, []);
            method?.Invoke(player, null);

            var combatMethod = AccessTools.Method(player.PlayerCombatState.GetType(), hookName, []);
            combatMethod?.Invoke(player.PlayerCombatState, null);
        }

        await RefreshCombatCardsForRelicCreate(player);
    }

    private static async Task RefreshCombatCardsForRelicCreate(Player player)
    {
        foreach (var card in player.PlayerCombatState.AllCards)
        {
            var noArgHooks = new[] { "UpdateDescription", "RefreshDescription", "OnValuesUpdated", "NotifyRelicsChanged", "RefreshValues", "RecalculateDamage", "OnRelicChanged" };
            foreach (var hookName in noArgHooks)
            {
                var method = AccessTools.Method(card.GetType(), hookName, []);
                if (method == null)
                {
                    continue;
                }

                method.Invoke(card, null);
            }
        }

        await Task.CompletedTask;
    }
}

