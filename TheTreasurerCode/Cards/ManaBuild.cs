using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Enchantments;
using System.Collections.Generic;

namespace TheTreasurer.TheTreasurerCode.Cards;

public class ManaBuild : TheTreasurerCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new IntVar("DrawAmt", 2)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromKeyword(CardKeyword.Exhaust)
    ];

    public ManaBuild() : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.None)
    {
    }

    protected override bool HasRequiredPlayTargets()
    {
        return Owner == null || PileType.Hand.GetPile(Owner).Cards.Any(c => c.Enchantment != null);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        var candidates = PileType.Hand.GetPile(Owner).Cards.Where(c => c.Enchantment != null).ToList();
        if (candidates.Count == 0)
        {
            return;
        }

        var prefs = new CardSelectorPrefs(new LocString("gameplay_ui", "SELECT_CARD"), 1);
        var selected = (await CardSelectCmd.FromSimpleGrid(choiceContext, candidates, Owner, prefs)).FirstOrDefault();
        if (selected == null)
        {
            return;
        }

        CardCmd.ClearEnchantment(selected);
        await PlayerCmd.GainEnergy(2, Owner);
        await CardPileCmd.Draw(choiceContext, DynamicVars["DrawAmt"].BaseValue, Owner);
    }

    protected override PileType GetResultPileTypeForCardPlay()
    {
        return PileType.Exhaust;
    }

    protected override void OnUpgrade()
    {
        DynamicVars["DrawAmt"].UpgradeValueBy(2);
    }
}
