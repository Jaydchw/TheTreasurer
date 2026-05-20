using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using TheTreasurer.TheTreasurerCode.Cards;
using TheTreasurer.TheTreasurerCode.Relics;

namespace TheTreasurer.TheTreasurerCode.Patches;

[HarmonyPatch(typeof(CardModel), "get_ShouldGlowGold")]
public static class ConditionalCardGlowPatch
{
    [HarmonyPostfix]
    public static void Postfix(CardModel __instance, ref bool __result)
    {
        if (__result)
        {
            return;
        }

        if (__instance is MeltDown meltDown && meltDown.Owner != null && ResinRelicRegistry.WasResinRelicDestroyedThisTurn(meltDown.Owner))
        {
            __result = true;
            return;
        }

        if (__instance is CrackedShield crackedShield && crackedShield.Owner != null && ResinRelicRegistry.WasResinRelicDestroyedThisTurn(crackedShield.Owner))
        {
            __result = true;
        }
    }
}
