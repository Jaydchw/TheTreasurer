using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using TheTreasurer.TheTreasurerCode.Powers;

namespace TheTreasurer.TheTreasurerCode.Cards;

public class ArcaneFlow : TheTreasurerCard
{
    public ArcaneFlow() : base(2, CardType.Power, CardRarity.Uncommon, TargetType.None)
    {
    }

    protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        return IsUpgraded
            ? PowerCmd.Apply<ArcaneFlowPlusPower>(choiceContext, Owner.Creature, 1, Owner.Creature, this)
            : PowerCmd.Apply<ArcaneFlowPower>(choiceContext, Owner.Creature, 1, Owner.Creature, this);
    }
}
