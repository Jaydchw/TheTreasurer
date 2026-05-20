using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Nodes.CommonUi;

namespace TheTreasurer.TheTreasurerCode.Cards;

public class CashOut : TheTreasurerCard
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromKeyword(CardKeyword.Exhaust)
    ];

    public CashOut() : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.None)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        await PlayerCmd.LoseGold(15, Owner);

        var hand = PileType.Hand.GetPile(Owner).Cards.ToList();
        if (hand.Count > 0)
        {
            CardCmd.Upgrade(hand, CardPreviewStyle.None);
        }
    }

    protected override PileType GetResultPileTypeForCardPlay()
    {
        if (!IsUpgraded)
        {
            return PileType.Exhaust;
        }

        return base.GetResultPileTypeForCardPlay();
    }
}
