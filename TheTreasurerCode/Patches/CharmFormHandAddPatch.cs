using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using System.Linq;
using TheTreasurer.TheTreasurerCode.Cards;
using TheTreasurer.TheTreasurerCode.Powers;

namespace TheTreasurer.TheTreasurerCode.Patches;

[HarmonyPatch(typeof(CardPileCmd))]
public static class CharmFormHandAddPatch
{
    [HarmonyPatch("CreateCardNodeAndUpdateVisuals")]
    [HarmonyPostfix]
    public static void Postfix(CardModel card, PileType targetPileType)
    {
        var owner = card?.Owner;
        if (owner?.Creature == null)
        {
            return;
        }

        if (targetPileType != PileType.Hand)
        {
            return;
        }

        var hasCharmForm = owner.Creature.Powers.Any(p => p is CharmFormPower);
        if (!hasCharmForm)
        {
            return;
        }

        _ = CardEnchantApi.TryForceApplyRandomKnownEnchant(card, 1, owner);
    }
}
