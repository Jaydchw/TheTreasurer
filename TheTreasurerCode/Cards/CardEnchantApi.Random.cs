using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Enchantments;
using System.Collections.Generic;
using System.Linq;

namespace TheTreasurer.TheTreasurerCode.Cards;

public static partial class CardEnchantApi
{
    public static bool TryApplyRandomKnownEnchant(CardModel card, int amount, Player owner)
    {
        var attempts = new List<System.Func<bool>>
        {
            () => TryApply<Sharp>(card, amount),
            () => TryApply<Swift>(card, amount),
            () => TryApplyNimble(card, amount, allowNonBlocking: true),
            () => TryApply<Spiral>(card, amount),
            () => TryApply<Sown>(card, amount)
        };

        owner.RunState.Rng.TreasureRoomRelics.Shuffle(attempts);
        foreach (var attempt in attempts)
        {
            if (attempt())
            {
                return true;
            }
        }

        return false;
    }
}
