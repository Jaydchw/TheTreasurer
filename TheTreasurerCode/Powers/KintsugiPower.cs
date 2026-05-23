using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using System.Linq;
using System.Threading.Tasks;
using TheTreasurer.TheTreasurerCode.Cards;
using TheTreasurer.TheTreasurerCode.Relics;

namespace TheTreasurer.TheTreasurerCode.Powers;

public class KintsugiPower : TheTreasurerPower
{
    private int _lastHandledRound = -999;

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    public async Task AfterTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
    {
        if (side != CombatSide.Player || !Owner.IsPlayer || Owner.Player == null)
        {
            return;
        }

        var round = Owner.CombatState?.RoundNumber ?? -1;
        if (_lastHandledRound == round || !ResinRelicRegistry.WasResinRelicDestroyedThisTurn(Owner.Player))
        {
            return;
        }

        _lastHandledRound = round;
        await PlayerCmd.GainGold(5, Owner.Player);

        var hand = PileType.Hand.GetPile(Owner.Player).Cards.Where(c => c.Enchantment == null).ToList();
        var applications = System.Math.Min(Amount, hand.Count);
        for (var i = 0; i < applications; i++)
        {
            var target = Owner.Player.RunState.Rng.TreasureRoomRelics.NextItem(hand);
            if (target == null)
            {
                break;
            }

            hand.Remove(target);
            _ = CardEnchantApi.TryApplyRandomKnownEnchant(target, 1, Owner.Player);
        }
    }
}
