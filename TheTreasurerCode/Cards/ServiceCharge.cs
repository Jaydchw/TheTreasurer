using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace TheTreasurer.TheTreasurerCode.Cards;

public class ServiceCharge : TheTreasurerCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new IntVar("GoldLoss", 10)
    ];

    public ServiceCharge() : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.None)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        if (Owner.Gold > 0)
        {
            await PlayerCmd.LoseGold(System.Math.Min(DynamicVars["GoldLoss"].BaseValue, Owner.Gold), Owner);
        }

        var hand = PileType.Hand.GetPile(Owner).Cards
            .Where(c => c.Enchantment == null)
            .ToList();

        for (var i = 0; i < 2 && hand.Count > 0; i++)
        {
            var target = Owner.RunState.Rng.TreasureRoomRelics.NextItem(hand);
            if (target == null)
            {
                break;
            }

            _ = CardEnchantApi.TryForceApplyRandomKnownEnchant(target, 1, Owner);
            hand.Remove(target);
        }
    }
}

