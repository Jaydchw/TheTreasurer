using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Powers;

namespace TheTreasurer.TheTreasurerCode.Powers;

public class DirectDepositPower : TheTreasurerPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterSideTurnStart(CombatSide side, ICombatState combatState)
    {
        if (side != CombatSide.Player || !Owner.IsPlayer || Owner.Player == null || Amount <= 0)
        {
            return;
        }

        await PlayerCmd.GainGold(Amount, Owner.Player);
        await PowerCmd.Remove(this);
    }
}
