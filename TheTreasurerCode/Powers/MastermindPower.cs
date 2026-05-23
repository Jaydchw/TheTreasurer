using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using TheTreasurer.TheTreasurerCode.Relics;

namespace TheTreasurer.TheTreasurerCode.Powers;

public class MastermindPower : TheTreasurerPower
{
    private int _lastSeenResinCount;

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override Task AfterApplied(MegaCrit.Sts2.Core.Entities.Creatures.Creature applier, MegaCrit.Sts2.Core.Models.CardModel cardSource)
    {
        if (Owner.IsPlayer && Owner.Player != null)
        {
            _lastSeenResinCount = ResinRelicRegistry.GetResinRelics(Owner.Player).Count;
        }

        return Task.CompletedTask;
    }

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay play)
    {
        if (!Owner.IsPlayer || Owner.Player == null)
        {
            return;
        }

        var current = ResinRelicRegistry.GetResinRelics(Owner.Player).Count;
        var gained = current - _lastSeenResinCount;
        _lastSeenResinCount = current;
        if (gained <= 0 || Amount <= 0)
        {
            return;
        }

        await PowerCmd.Apply<StrengthPower>(choiceContext, [Owner], gained * Amount, Owner, null);
    }
}
