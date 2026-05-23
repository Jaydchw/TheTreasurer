using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using TheTreasurer.TheTreasurerCode.Relics;

namespace TheTreasurer.TheTreasurerCode.Powers;

public class ResinShieldPower : TheTreasurerPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public async Task AfterTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
    {
        if (!Owner.IsPlayer || Owner.Player == null || side != CombatSide.Player)
        {
            return;
        }

        if (!ResinRelicRegistry.WasResinRelicDestroyedThisTurn(Owner.Player))
        {
            return;
        }

        await PowerCmd.Apply<DexterityPower>(choiceContext, [Owner], Amount, Owner, null);
    }
}
