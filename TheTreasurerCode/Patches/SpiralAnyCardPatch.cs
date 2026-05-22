using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Enchantments;
using TheTreasurer.TheTreasurerCode.Cards;

namespace TheTreasurer.TheTreasurerCode.Patches;

[HarmonyPatch(typeof(Spiral), nameof(Spiral.CanEnchant))]
public static class SpiralAnyCardPatch
{
    [HarmonyPrefix]
    public static bool Prefix(CardModel c, ref bool __result)
    {
        if (!CardEnchantApi.IsSpiralAnyCardAllowed(c))
        {
            return true;
        }

        __result = c != null;
        return false;
    }
}
