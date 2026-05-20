using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Relics;
using TheTreasurer.TheTreasurerCode.Extensions;

namespace TheTreasurer.TheTreasurerCode.Patches;

[HarmonyPatch]
public static class ResinRelicVisualPatch
{
    private static readonly Color ResinTint = new("5cff7a");
    private static readonly System.Reflection.FieldInfo? NRelicModelField = AccessTools.Field(typeof(NRelic), "_model");

    private static void ApplyTint(TextureRect? texture)
    {
        if (texture == null) return;
        texture.SelfModulate = ResinTint;
        texture.Modulate = ResinTint;
    }

    private static bool IsResin(NRelic? nRelic)
    {
        if (nRelic == null) return false;
        try
        {
            var model = nRelic.Model;
            return model != null && model.IsResinRelic();
        }
        catch (System.InvalidOperationException)
        {
            return false;
        }
    }

    [HarmonyPatch(typeof(RelicModel), "get_Title")]
    [HarmonyPostfix]
    public static void TitlePostfix(RelicModel __instance, ref LocString __result)
    {
        if (!__instance.IsResinRelic()) return;
        var prefix = new LocString("relics", "THETREASURER-RESIN_RELIC_PREFIX");
        prefix.Add("Title", __result);
        __result = prefix;
    }

    [HarmonyPatch(typeof(RelicModel), nameof(RelicModel.UpdateTexture))]
    [HarmonyPostfix]
    public static void UpdateTexturePostfix(RelicModel __instance, TextureRect texture)
    {
        if (__instance.IsResinRelic()) ApplyTint(texture);
    }

    [HarmonyPatch(typeof(NRelicInventoryHolder), "_Ready")]
    [HarmonyPostfix]
    public static void HolderReadyPostfix(NRelicInventoryHolder __instance)
    {
        if (!IsResin(__instance.Relic)) return;
        ApplyTint(__instance.Relic?.Icon);
        ApplyTint(__instance.Relic?.Outline);
    }

    [HarmonyPatch(typeof(NRelicInventoryHolder), "RefreshStatus")]
    [HarmonyPostfix]
    public static void RefreshStatusPostfix(NRelicInventoryHolder __instance)
    {
        if (!IsResin(__instance.Relic)) return;
        ApplyTint(__instance.Relic?.Icon);
        ApplyTint(__instance.Relic?.Outline);
    }

    [HarmonyPatch(typeof(NRelicInventoryHolder), "OnModelChanged")]
    [HarmonyPostfix]
    public static void OnModelChangedPostfix(NRelicInventoryHolder __instance)
    {
        if (!IsResin(__instance.Relic)) return;
        ApplyTint(__instance.Relic?.Icon);
        ApplyTint(__instance.Relic?.Outline);
    }

    [HarmonyPatch(typeof(NRelicInventoryHolder), "_Process")]
    [HarmonyPostfix]
    public static void HolderProcessPostfix(NRelicInventoryHolder __instance)
    {
        if (!IsResin(__instance.Relic)) return;
        ApplyTint(__instance.Relic?.Icon);
        ApplyTint(__instance.Relic?.Outline);
    }

    [HarmonyPatch(typeof(NRelic), "_Ready")]
    [HarmonyPostfix]
    public static void NRelicReadyPostfix(NRelic __instance)
    {
        if (NRelicModelField?.GetValue(__instance) is not RelicModel model || !model.IsResinRelic()) return;
        ApplyTint(__instance.Icon);
        ApplyTint(__instance.Outline);
    }

    [HarmonyPatch(typeof(NRelic), "set_Model")]
    [HarmonyPostfix]
    public static void NRelicSetModelPostfix(NRelic __instance, RelicModel value)
    {
        if (!value.IsResinRelic()) return;
        ApplyTint(__instance.Icon);
        ApplyTint(__instance.Outline);
    }
}
