using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Rooms;
using TheTreasurer.TheTreasurerCode.Relics;

namespace TheTreasurer.TheTreasurerCode.Powers;

public class ResinRelicCleanupPower : TheTreasurerPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    public override async Task AfterCombatEnd(CombatRoom room)
    {
        if (Owner.IsPlayer && Owner.Player != null)
        {
            await ResinRelicRegistry.Cleanup(Owner.Player);
        }

        await PowerCmd.Remove(this);
    }
}
