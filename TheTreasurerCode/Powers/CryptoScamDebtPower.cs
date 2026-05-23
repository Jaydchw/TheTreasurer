using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace TheTreasurer.TheTreasurerCode.Powers;

public class CryptoScamDebtPower : TheTreasurerPower
{
    private int _turnsRemaining = 2;

    public override PowerType Type => PowerType.Debuff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public async Task AfterTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
    {
        if (!Owner.IsPlayer || Owner.Player == null || side != CombatSide.Player)
        {
            return;
        }

        _turnsRemaining--;
        if (_turnsRemaining > 0)
        {
            return;
        }

        await PlayerCmd.LoseGold(Amount, Owner.Player);
        await PowerCmd.Remove(this);
    }
}
