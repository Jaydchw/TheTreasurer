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

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        await PlayerCmd.LoseGold(DynamicVars["GoldLoss"].BaseValue, Owner);

        var hand = PileType.Hand.GetPile(Owner).Cards
            .Where(c => c.Enchantment == null && CanApplyTypedEnchant(c))
            .ToList();
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
        switch (card.Type)
        {
            case CardType.Power:
                _ = CardEnchantApi.TryApply<Swift>(card, 2);
                break;
            case CardType.Attack:
                _ = CardEnchantApi.TryApply<Sharp>(card, 2);
                break;
            case CardType.Skill:
                _ = CardEnchantApi.TryApply<Nimble>(card, 2);
                break;
        }
    }

    private static bool CanApplyTypedEnchant(CardModel card)
    {
        return card.Type switch
        {
            CardType.Power => CardEnchantApi.CanApply<Swift>(card),
            CardType.Attack => CardEnchantApi.CanApply<Sharp>(card),
            CardType.Skill => CardEnchantApi.CanApply<Nimble>(card),
            _ => false
        };
    }
}
