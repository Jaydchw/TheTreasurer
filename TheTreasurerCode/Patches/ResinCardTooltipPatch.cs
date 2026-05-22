using System.Collections.Generic;
using System.Linq;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using TheTreasurer.TheTreasurerCode.Extensions;

namespace TheTreasurer.TheTreasurerCode.Patches;

[HarmonyPatch]
public static class ResinCardTooltipPatch
{
    private static readonly Texture2D? ResinTipIcon = ResourceLoader.Load<Texture2D>("power.png".PowerImagePath());
    private static readonly LocString ResinTipTitle = new("static_hover_tips", "THETREASURER-RESIN.title");
    private static readonly LocString ResinTipDescription = new("static_hover_tips", "THETREASURER-RESIN.description");

    private static readonly HashSet<string> ResinCardIds =
    [
        "THETREASURER-TREASURE",
        "THETREASURER-CHECK_SATCHEL",
        "THETREASURER-CHINA_BULL",
        "THETREASURER-CRACKED_SHIELD",
        "THETREASURER-DIRECT_DEPOSIT",
        "THETREASURER-MELT_DOWN",
        "THETREASURER-EMERGENCY_FUND",
        "THETREASURER-COUNTERFEIT",
        "THETREASURER-BIG_SPENDER",
        "THETREASURER-BUY_OUT",
        "THETREASURER-KINTSUGI",
        "THETREASURER-PERFECT_COUNTERFEIT",
        "THETREASURER-RAID",
        "THETREASURER-SOLIDIFY",
        "THETREASURER-FROM_THE_ASHES"
    ];

    [HarmonyPatch(typeof(CardModel), "get_HoverTips")]
    [HarmonyPostfix]
    public static void HoverTipsPostfix(CardModel __instance, ref IEnumerable<IHoverTip> __result)
    {
        if (!ResinCardIds.Contains(__instance.Id.ToString()))
        {
            return;
        }

        var resinTip = new HoverTip(ResinTipTitle, ResinTipDescription, ResinTipIcon);
        __result = (__result ?? []).Concat(new IHoverTip[] { resinTip });
    }
}
