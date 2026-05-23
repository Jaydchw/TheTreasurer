using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using TheTreasurer.TheTreasurerCode.Relics;

namespace TheTreasurer.TheTreasurerCode.Powers;

public class ReplicaPower : TheTreasurerPower
{
    private bool _duplicatedThisTurn;
    private int _lastSeenResinCount;

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    public Task AfterSideTurnStart(CombatSide side, ICombatState combatState)
    {
        if (!Owner.IsPlayer || Owner.Player == null)
        {
            return Task.CompletedTask;
        }

        if (side == CombatSide.Player)
        {
            _duplicatedThisTurn = false;
        }

        _lastSeenResinCount = ResinRelicRegistry.GetResinRelics(Owner.Player).Count;
        return Task.CompletedTask;
    }

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay play)
    {
        if (!Owner.IsPlayer || Owner.Player == null || _duplicatedThisTurn)
        {
            return;
        }

        var current = ResinRelicRegistry.GetResinRelics(Owner.Player);
        if (current.Count <= _lastSeenResinCount)
        {
            return;
        }

        var source = Owner.Player.RunState.Rng.TreasureRoomRelics.NextItem(current);
        if (source == null)
        {
            return;
        }

        _duplicatedThisTurn = true;
        _lastSeenResinCount = current.Count;
        _ = await ResinRelicRegistry.CreateResinFromTemplate(Owner.Player, ResinRelicRegistry.ResolveTemplateForRelic(source));
    }
}

