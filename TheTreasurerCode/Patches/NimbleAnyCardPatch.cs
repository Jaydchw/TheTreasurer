using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Enchantments;
using TheTreasurer.TheTreasurerCode.Cards;

namespace TheTreasurer.TheTreasurerCode.Patches;

[HarmonyPatch(typeof(Nimble), nameof(Nimble.CanEnchant))]
public static class NimbleAnyCardPatch
{
    [HarmonyPrefix]
    public static bool Prefix(CardModel c, ref bool __result)
    {
        if (!CardEnchantApi.IsNimbleAnyCardAllowed(c))
        {
            return true;
        }

        __result = c != null;
        return false;
    }
}
