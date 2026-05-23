using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using TheTreasurer.TheTreasurerCode.Powers;

namespace TheTreasurer.TheTreasurerCode.Cards;

public class Replica : TheTreasurerCard
{
    public Replica() : base(2, CardType.Power, CardRarity.Uncommon, TargetType.None)
    {
    }

    protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        return PowerCmd.Apply<ReplicaPower>(choiceContext, Owner.Creature, 1, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}

