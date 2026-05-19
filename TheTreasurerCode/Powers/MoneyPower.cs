using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Powers;

namespace TheTreasurer.TheTreasurerCode.Powers;

public class MoneyPower : TheTreasurerPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    public override Task AfterSideTurnStart(CombatSide side, ICombatState combatState)
    {
        if (side == CombatSide.Player && Owner.IsPlayer && Owner.Player != null && Owner.Player.Gold > 100)
        {
            return PlayerCmd.GainEnergy(1, Owner.Player);
        }

        return Task.CompletedTask;
    }
}
