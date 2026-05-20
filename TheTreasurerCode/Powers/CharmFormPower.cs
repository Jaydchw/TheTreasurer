using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using System.Linq;
using System.Threading.Tasks;
using TheTreasurer.TheTreasurerCode.Cards;

namespace TheTreasurer.TheTreasurerCode.Powers;

public class CharmFormPower : TheTreasurerPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    public override Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay play)
    {
        if (!Owner.IsPlayer || Owner.Player == null || play.Card.Owner != Owner.Player)
        {
            return Task.CompletedTask;
        }

        var hand = PileType.Hand.GetPile(Owner.Player).Cards.Where(c => c.Enchantment == null).ToList();
        if (hand.Count == 0)
        {
            return Task.CompletedTask;
        }

        var target = Owner.Player.RunState.Rng.TreasureRoomRelics.NextItem(hand);
        if (target != null)
        {
            _ = CardEnchantApi.TryApplyRandomKnownEnchant(target, 1, Owner.Player);
        }

        return Task.CompletedTask;
    }
}
