using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace TheTreasurer.TheTreasurerCode.Powers;

public class ArcaneFlowPlusPower : ArcaneFlowPower
{
    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay play)
    {
        if (UsedThisTurn || !Owner.IsPlayer || Owner.Player == null || play.Card.Owner != Owner.Player || play.Card.Enchantment == null)
        {
            return;
        }

        await base.AfterCardPlayed(choiceContext, play);
        await CardPileCmd.Draw(choiceContext, 1, Owner.Player);
    }
}
