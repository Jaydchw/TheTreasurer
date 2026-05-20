using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using TheTreasurer.TheTreasurerCode.Powers;

namespace TheTreasurer.TheTreasurerCode.Cards;

public class CompoundInterest : TheTreasurerCard
{
    public CompoundInterest() : base(2, CardType.Power, CardRarity.Rare, TargetType.None)
    {
    }

    protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        return PowerCmd.Apply<CompoundInterestPower>(choiceContext, Owner.Creature, IsUpgraded ? 15 : 10, Owner.Creature, this);
    }
}
