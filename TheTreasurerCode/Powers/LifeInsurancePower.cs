using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using System.Threading.Tasks;

namespace TheTreasurer.TheTreasurerCode.Powers;

public class LifeInsurancePower : TheTreasurerPower
{
    private bool _used;

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    public async Task AfterTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
    {
        if (_used || side != CombatSide.Player || !Owner.IsPlayer || Owner.Player == null)
        {
            return;
        }

        if (Owner.CurrentHp > 0)
        {
            return;
        }

        _used = true;
        Owner.SetCurrentHpInternal(1);
        if (Owner.Player.Gold > 0)
        {
            await PlayerCmd.LoseGold(System.Math.Min(Amount, Owner.Player.Gold), Owner.Player);
        }

    }
}
