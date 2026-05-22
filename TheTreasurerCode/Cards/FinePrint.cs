using System.Collections.Generic;
using MegaCrit.Sts2.Core.CardSelection;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Enchantments;
using System.Linq;

namespace TheTreasurer.TheTreasurerCode.Cards;

public class FinePrint : TheTreasurerCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new IntVar("GoldLoss", 20)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips
    {
        get
        {
            var tips = new List<IHoverTip>();
            tips.AddRange(HoverTipFactory.FromEnchantment<Spiral>(2));
            if (!IsUpgraded)
            {
                tips.AddRange(HoverTipFactory.FromKeyword(CardKeyword.Exhaust));
            }

            return tips;
        }
    }

    public FinePrint() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.None)
    {
    }

    protected override bool HasRequiredPlayTargets()
    {
        if (Owner == null)
        {
            return true;
        }

        return PileType.Hand.GetPile(Owner).Cards.Any(c => CardEnchantApi.CanApplySpiral(c, allowAnyCard: true));
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        await PlayerCmd.LoseGold(DynamicVars["GoldLoss"].BaseValue, Owner);

        var candidates = PileType.Hand.GetPile(Owner).Cards
            .Where(c => CardEnchantApi.CanApplySpiral(c, allowAnyCard: true))
            .ToList();
        if (candidates.Count == 0)
        {
            return;
        }

        var prefs = new CardSelectorPrefs(CardSelectorPrefs.EnchantSelectionPrompt, 1);
        var selected = (await CardSelectCmd.FromHandForDiscard(choiceContext, Owner, prefs, null, source: this)).FirstOrDefault();
        if (selected == null)
        {
            return;
        }

        if (!candidates.Contains(selected))
        {
            return;
        }

        _ = CardEnchantApi.TryApplySpiral(selected, 2, allowAnyCard: true);
    }

    protected override PileType GetResultPileTypeForCardPlay()
    {
        return IsUpgraded ? PileType.Discard : PileType.Exhaust;
    }

    protected override void OnUpgrade() { }
}
