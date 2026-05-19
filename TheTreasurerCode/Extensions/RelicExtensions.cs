using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;
using TheTreasurer.TheTreasurerCode.Relics;

namespace TheTreasurer.TheTreasurerCode.Extensions;

public static class RelicExtensions
{
    public static bool IsResinRelic(this RelicModel relic)
    {
        return ResinRelicRegistry.IsResin(relic);
    }
}
