using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Nodes.CommonUi;

namespace TheTreasurer.TheTreasurerCode.Cards;

public class Spellsmith : TheTreasurerCard
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromKeyword(CardKeyword.Exhaust)
    ];

    public Spellsmith() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.None)
    {
    }

    protected override bool HasRequiredPlayTargets()
    {
        return Owner == null || PileType.Hand.GetPile(Owner).Cards.Any(c => c.Enchantment != null);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        var enchanted = PileType.Hand.GetPile(Owner).Cards.Where(c => c.Enchantment != null).ToList();
        if (enchanted.Count == 0)
        {
            return;
        }

        var prefs = new CardSelectorPrefs(CardSelectorPrefs.EnchantSelectionPrompt, 1);
        var source = (await CardSelectCmd.FromHandForDiscard(choiceContext, Owner, prefs, null, this)).FirstOrDefault();
        if (source?.Enchantment == null || !enchanted.Contains(source))
        {
            return;
        }

        var enchantment = source.Enchantment;
        foreach (var card in PileType.Hand.GetPile(Owner).Cards.Where(c => !ReferenceEquals(c, source)))
        {
            if (card.Enchantment != null)
            {
                CardCmd.ClearEnchantment(card);
            }

            var clone = (enchantment.IsCanonical ? enchantment : enchantment.CanonicalInstance ?? enchantment).ToMutable();
            CardCmd.Enchant(clone, card, enchantment.Amount);
        }
    }

    protected override void OnUpgrade()
    {
        // Upgraded effect is represented in localization as "this combat".
    }

    protected override PileType GetResultPileTypeForCardPlay() => PileType.Exhaust;
}
