using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace TheTreasurer.TheTreasurerCode.Powers;

public class ArcaneFlowPower : TheTreasurerPower
{
    protected bool UsedThisTurn;

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    public Task AfterSideTurnStart(CombatSide side, ICombatState combatState)
    {
        if (side == CombatSide.Player && Owner.IsPlayer)
        {
            UsedThisTurn = false;
        }

        return Task.CompletedTask;
    }

    public override bool TryModifyEnergyCostInCombat(CardModel card, decimal originalCost, out decimal modifiedCost)
    {
        modifiedCost = originalCost;
        if (UsedThisTurn || !Owner.IsPlayer || Owner.Player == null)
        {
            return false;
        }

        if (card.Owner != Owner.Player || card.Enchantment == null)
        {
            return false;
        }

        modifiedCost = 0;
        return true;
    }

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay play)
    {
        if (UsedThisTurn || play.Card.Owner != Owner.Player || play.Card.Enchantment == null)
        {
            return;
        }

        UsedThisTurn = true;
        await Task.CompletedTask;
    }
}

