using System.Collections.Generic;
using System.Linq;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Relics;
using TheTreasurer.TheTreasurerCode.Extensions;

namespace TheTreasurer.TheTreasurerCode.Patches;

[HarmonyPatch]
public static class ResinRelicTooltipPatch
{
    private static readonly Texture2D? ResinTipIcon = ResourceLoader.Load<Texture2D>("power.png".PowerImagePath());
    private static readonly LocString ResinTipTitle = new("static_hover_tips", "THETREASURER-RESIN.title");
    private static readonly LocString ResinTipDescription = new("static_hover_tips", "THETREASURER-RESIN.description");

    [HarmonyTargetMethods]
    public static IEnumerable<System.Reflection.MethodBase> TargetMethods()
    {
        var modelHoverTipsGetter = AccessTools.PropertyGetter(typeof(RelicModel), "HoverTips");
        if (modelHoverTipsGetter != null)
        {
            yield return modelHoverTipsGetter;
        }

        var modelHoverTipsMethod = AccessTools.Method(typeof(RelicModel), "GetHoverTips");
        if (modelHoverTipsMethod != null)
        {
            yield return modelHoverTipsMethod;
        }

        var nodeHoverTipsGetter = AccessTools.PropertyGetter(typeof(NRelic), "HoverTips");
        if (nodeHoverTipsGetter != null)
        {
            yield return nodeHoverTipsGetter;
        }

        var nodeHoverTipsMethod = AccessTools.Method(typeof(NRelic), "GetHoverTips");
        if (nodeHoverTipsMethod != null)
        {
            yield return nodeHoverTipsMethod;
        }
    }

    [HarmonyPostfix]
    public static void HoverTipsPostfix(object __instance, ref IEnumerable<IHoverTip> __result)
    {
        var relicModel = __instance switch
        {
            RelicModel model => model,
            NRelic node => node.Model,
            _ => null
        };

        if (relicModel == null || !relicModel.IsResinRelic())
        {
            return;
        }

        var existing = __result?.ToList() ?? [];
        var resinTip = new HoverTip(ResinTipTitle, ResinTipDescription, ResinTipIcon);
        __result = existing.Concat(new IHoverTip[] { resinTip });
    }
}
