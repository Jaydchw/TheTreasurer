using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Enchantments;

namespace TheTreasurer.TheTreasurerCode.Cards;

public class Artifice : TheTreasurerCard
{
    private readonly record struct EnchantRule(
        System.Func<CardModel, bool> CanApply,
        System.Action<CardModel> Apply);

    private static readonly Dictionary<CardType, EnchantRule> RulesByType = new()
    {
        [CardType.Power] = new(
            CanApply: card => CardEnchantApi.CanApply<Swift>(card),
            Apply: card => _ = CardEnchantApi.TryApply<Swift>(card, 2)),
        [CardType.Attack] = new(
            CanApply: card => CardEnchantApi.CanApply<Sharp>(card),
            Apply: card => _ = CardEnchantApi.TryApply<Sharp>(card, 2)),
        [CardType.Skill] = new(
            CanApply: card => CardEnchantApi.CanApplyNimble(card, allowNonBlocking: true),
            Apply: card => _ = CardEnchantApi.TryApplyNimble(card, 2, allowNonBlocking: true))
    };

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new IntVar("GoldLoss", 80)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        ..HoverTipFactory.FromEnchantment<Swift>(2),
        ..HoverTipFactory.FromEnchantment<Sharp>(2),
        ..HoverTipFactory.FromEnchantment<Nimble>(2)
    ];

    public Artifice() : base(2, CardType.Skill, CardRarity.Uncommon, TargetType.None)
    {
    }

    protected override bool HasRequiredPlayTargets()
    {
        if (Owner == null)
        {
            return true;
        }

        return GetEnchantableHandCards().Count > 0;
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        await PlayerCmd.LoseGold(DynamicVars["GoldLoss"].BaseValue, Owner);

        var hand = GetEnchantableHandCards();
        if (hand.Count == 0)
        {
            return;
        }

        var selected = (await CardSelectCmd.FromSimpleGrid(
            choiceContext,
            hand,
            Owner,
            new MegaCrit.Sts2.Core.CardSelection.CardSelectorPrefs(new MegaCrit.Sts2.Core.Localization.LocString("gameplay_ui", "SELECT_CARD"), 1)))
            .FirstOrDefault();
        if (selected == null)
        {
            return;
        }

        ApplyTypedEnchant(selected);

        var deckCard = Owner.Deck.Cards.FirstOrDefault(c => c.Id == selected.Id && c.CurrentUpgradeLevel == selected.CurrentUpgradeLevel);
        if (deckCard != null)
        {
            ApplyTypedEnchant(deckCard);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["GoldLoss"].UpgradeValueBy(-30);
    }

    private static void ApplyTypedEnchant(CardModel card)
    {
        if (RulesByType.TryGetValue(card.Type, out var rule))
        {
            rule.Apply(card);
        }
    }

    private static bool CanApplyTypedEnchant(CardModel card)
    {
        return RulesByType.TryGetValue(card.Type, out var rule) && rule.CanApply(card);
    }

    private List<CardModel> GetEnchantableHandCards()
    {
        return PileType.Hand.GetPile(Owner).Cards
            .Where(c => c.Enchantment == null && CanApplyTypedEnchant(c))
            .ToList();
    }
}
