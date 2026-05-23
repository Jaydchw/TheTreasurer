using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace TheTreasurer.TheTreasurerCode.Powers;

public class PocketsOutPower : TheTreasurerPower
{
    private int _hpAtTurnStart;

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    public Task AfterSideTurnStart(CombatSide side, ICombatState combatState)
    {
        if (Owner.IsPlayer && side == CombatSide.Player)
        {
            _hpAtTurnStart = Owner.CurrentHp;
        }

        return Task.CompletedTask;
    }

    public async Task AfterTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
    {
        if (!Owner.IsPlayer || Owner.Player == null || side != CombatSide.Player)
        {
            return;
        }

        var lost = _hpAtTurnStart - Owner.CurrentHp;
        if (lost <= 0)
        {
            return;
        }

        var divisor = Amount <= 2 ? 2 : 3;
        var stolen = lost / divisor;
        if (stolen > 0)
        {
            await PlayerCmd.LoseGold(stolen, Owner.Player);
        }
    }
}

