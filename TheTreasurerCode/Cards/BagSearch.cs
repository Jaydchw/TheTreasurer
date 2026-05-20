using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace TheTreasurer.TheTreasurerCode.Cards;

public class BagSearch : TheTreasurerCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new IntVar("GoldLoss", 10)
    ];

    public BagSearch() : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.None)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        var drawPile = PileType.Draw.GetPile(Owner).Cards.ToList();
        if (drawPile.Count > 0)
        {
            var prefs = new CardSelectorPrefs(new LocString("gameplay_ui", "SELECT_CARD"), 1);
            var selected = (await CardSelectCmd.FromSimpleGrid(choiceContext, drawPile, Owner, prefs)).FirstOrDefault();
            if (selected != null)
            {
                await CardPileCmd.Add(selected, PileType.Hand);
            }
        }

        await PlayerCmd.LoseGold(DynamicVars["GoldLoss"].BaseValue, Owner);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["GoldLoss"].UpgradeValueBy(-5);
    }
}
