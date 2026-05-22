using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Powers;
using System.Threading.Tasks;

namespace TheTreasurer.TheTreasurerCode.Powers;

public class CharmFormPower : TheTreasurerPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    public override Task AfterSideTurnStart(CombatSide side, ICombatState combatState) => Task.CompletedTask;
}
