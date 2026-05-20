using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Characters;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Relics;
using TheTreasurer.TheTreasurerCode.Cards;

namespace TheTreasurer.TheTreasurerCode.Patches;

[HarmonyPatch]
public static class StrikeDefendCompatibilityPatch
{
    private static bool IsTreasurer(CharacterModel? character)
    {
        return character?.Id.Entry == "TheTreasurer";
    }

    [HarmonyPatch(typeof(LargeCapsule), "GetStrikeForCharacter")]
    [HarmonyPrefix]
    public static bool GetStrikeForCharacterPrefix(CharacterModel character, ref CardModel __result)
    {
        if (!IsTreasurer(character))
        {
            return true;
        }

        __result = ModelDb.Card<StrikeTreasurer>();
        return false;
    }

    [HarmonyPatch(typeof(LargeCapsule), "GetDefendForCharacter")]
    [HarmonyPrefix]
    public static bool GetDefendForCharacterPrefix(CharacterModel character, ref CardModel __result)
    {
        if (!IsTreasurer(character))
        {
            return true;
        }

        __result = ModelDb.Card<DefendTreasurer>();
        return false;
    }
}
