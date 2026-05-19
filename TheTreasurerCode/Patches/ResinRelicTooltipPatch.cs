using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using TheTreasurer.TheTreasurerCode.Extensions;

namespace TheTreasurer.TheTreasurerCode.Patches;

[HarmonyPatch]
public static class ResinRelicTooltipPatch
{
    [HarmonyPatch(typeof(RelicModel), "get_HoverTips")]
    [HarmonyPostfix]
    public static void HoverTipsPostfix(RelicModel __instance, ref IEnumerable<IHoverTip> __result)
    {
        if (!__instance.IsResinRelic())
        {
            return;
        }

        var title = new LocString("static_hover_tips", "THETREASURER-RESIN.title");
        var description = new LocString("static_hover_tips", "THETREASURER-RESIN.description");
        var resinTip = new HoverTip(title, description);

        __result = __result.Concat(new IHoverTip[] { resinTip });
    }
}
