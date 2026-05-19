using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using TheTreasurer.TheTreasurerCode.Extensions;
using TheTreasurer.TheTreasurerCode.Relics;

namespace TheTreasurer.TheTreasurerCode.Patches;

[HarmonyPatch]
public static class ResinRelicVisualPatch
{
    [HarmonyPatch(typeof(RelicModel), "get_Title")]
    [HarmonyPostfix]
    public static void TitlePostfix(RelicModel __instance, ref LocString __result)
    {
        if (!__instance.IsResinRelic())
        {
            return;
        }

        var prefix = new LocString("relics", "THETREASURER-RESIN_RELIC_PREFIX");
        prefix.Add("Title", __result);
        __result = prefix;
    }

    [HarmonyPatch(typeof(RelicModel), nameof(RelicModel.UpdateTexture))]
    [HarmonyPostfix]
    public static void UpdateTexturePostfix(RelicModel __instance, TextureRect texture)
    {
        if (!__instance.IsResinRelic())
        {
            return;
        }

        texture.SelfModulate = new Color("5cff7a");
    }
}
