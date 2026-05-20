using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Relics;
using System.Collections.Generic;
using TheTreasurer.TheTreasurerCode.Extensions;

namespace TheTreasurer.TheTreasurerCode.Patches;

[HarmonyPatch]
public static class ResinRelicVisualPatch
{
    private static readonly Color ResinTint = new("4CFF62");
    private static readonly System.Reflection.FieldInfo? NRelicModelField = AccessTools.Field(typeof(NRelic), "_model");

    private static void ApplyTint(CanvasItem? item)
    {
        if (item == null) return;
        item.SelfModulate = ResinTint;
        item.Modulate = ResinTint;
    }

    private static void ApplyTint(NRelic? relic)
    {
        if (relic == null) return;
        ApplyTint((CanvasItem)relic);
        ApplyTint(relic.Icon);
        ApplyTint(relic.Outline);
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
        ApplyTint(__instance.Relic);
    }

    [HarmonyPatch(typeof(NRelicInventoryHolder), "RefreshStatus")]
    [HarmonyPostfix]
    public static void RefreshStatusPostfix(NRelicInventoryHolder __instance)
    {
        if (!IsResin(__instance.Relic)) return;
        ApplyTint(__instance.Relic);
    }

    [HarmonyPatch(typeof(NRelicInventoryHolder), "OnModelChanged")]
    [HarmonyPostfix]
    public static void OnModelChangedPostfix(NRelicInventoryHolder __instance)
    {
        if (!IsResin(__instance.Relic)) return;
        ApplyTint(__instance.Relic);
    }

    [HarmonyPatch(typeof(NRelicInventoryHolder), "_Process")]
    [HarmonyPostfix]
    public static void HolderProcessPostfix(NRelicInventoryHolder __instance)
    {
        if (!IsResin(__instance.Relic)) return;
        ApplyTint(__instance.Relic);
    }

    [HarmonyPatch(typeof(NRelic), "_Ready")]
    [HarmonyPostfix]
    public static void NRelicReadyPostfix(NRelic __instance)
    {
        if (NRelicModelField?.GetValue(__instance) is not RelicModel model || !model.IsResinRelic()) return;
        ApplyTint(__instance);
    }

    [HarmonyPatch(typeof(NRelic), "set_Model")]
    [HarmonyPostfix]
    public static void NRelicSetModelPostfix(NRelic __instance, RelicModel value)
    {
        if (!value.IsResinRelic()) return;
        ApplyTint(__instance);
    }

    [HarmonyPatch(typeof(NRelic), "_Process")]
    [HarmonyPostfix]
    public static void NRelicProcessPostfix(NRelic __instance)
    {
        if (!IsResin(__instance)) return;
        ApplyTint(__instance);
    }

    public static void ForceRefreshAllRelics()
    {
        if (Engine.GetMainLoop() is not SceneTree tree || tree.Root == null)
        {
            return;
        }

        foreach (var node in FindNodes<NRelicInventoryHolder>(tree.Root))
        {
            var relicNode = node.Relic;
            if (relicNode == null)
            {
                continue;
            }

            if (IsResin(relicNode))
            {
                ApplyTint(relicNode);
            }
        }
    }

    private static IEnumerable<T> FindNodes<T>(Node root) where T : Node
    {
        var stack = new Stack<Node>();
        stack.Push(root);
        while (stack.Count > 0)
        {
            var node = stack.Pop();
            if (node is T matched)
            {
                yield return matched;
            }

            foreach (Node child in node.GetChildren())
            {
                stack.Push(child);
            }
        }
    }
}
