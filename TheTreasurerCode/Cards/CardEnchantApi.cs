using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Enchantments;

namespace TheTreasurer.TheTreasurerCode.Cards;

public static class CardEnchantApi
{
    public static bool CanApply<T>(CardModel card, bool requireUnenchanted = true) where T : EnchantmentModel
    {
        if (requireUnenchanted && card.Enchantment != null)
        {
            return false;
        }

        return ModelDb.Enchantment<T>().CanEnchant(card);
    }

    public static List<CardModel> GetEnchantableHandCards<T>(Player owner, bool requireUnenchanted = true) where T : EnchantmentModel
    {
        return PileType.Hand.GetPile(owner).Cards
            .Where(c => CanApply<T>(c, requireUnenchanted))
            .ToList();
    }

    public static async Task<CardModel?> SelectEnchantableHandCard<T>(
        PlayerChoiceContext choiceContext,
        Player owner,
        bool requireUnenchanted = true) where T : EnchantmentModel
    {
        var candidates = GetEnchantableHandCards<T>(owner, requireUnenchanted);
        if (candidates.Count == 0)
        {
            return null;
        }

        var prefs = new CardSelectorPrefs(new LocString("gameplay_ui", "SELECT_CARD"), 1);
        return (await CardSelectCmd.FromSimpleGrid(choiceContext, candidates, owner, prefs)).FirstOrDefault();
    }

    public static bool TryApply<T>(CardModel card, int amount) where T : EnchantmentModel
    {
        if (!CanApply<T>(card, requireUnenchanted: true))
        {
            return false;
        }

        CardCmd.Enchant<T>(card, amount);
        return true;
    }
}

