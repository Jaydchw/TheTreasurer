using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using System.Threading.Tasks;

namespace TheTreasurer.TheTreasurerCode.Powers;

public class GamblingAddictionPower : TheTreasurerPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    public async Task AfterTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
    {
        if (side != CombatSide.Player || !Owner.IsPlayer || Owner.Player == null)
        {
            return;
        }

        var loss = Owner.Player.RunState.Rng.TreasureRoomRelics.NextItem(new[] { 5, 6, 7, 8, 9, 10 });
        if (Owner.Player.Gold > 0)
        {
            await PlayerCmd.LoseGold(System.Math.Min(loss, Owner.Player.Gold), Owner.Player);
        }

        if (Owner.Player.Gold <= 0)
        {
            Owner.SetCurrentHpInternal(0);
            return;
        }

        await PlayerCmd.GainEnergy(1, Owner.Player);
        await CardPileCmd.Draw(choiceContext, Amount, Owner.Player);
    }
}
