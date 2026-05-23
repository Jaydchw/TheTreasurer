using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace TheTreasurer.TheTreasurerCode.Cards;

public class Improvise : TheTreasurerCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new IntVar("EnchantCount", 3)
    ];

    public Improvise() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.None)
    {
    }

    protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        var hand = PileType.Hand.GetPile(Owner).Cards
            .Where(c => c.Enchantment == null)
            .ToList();

        var count = DynamicVars["EnchantCount"].BaseValue;
        for (var i = 0; i < count && hand.Count > 0; i++)
        {
            var target = Owner.RunState.Rng.TreasureRoomRelics.NextItem(hand);
            if (target == null)
            {
                break;
            }

            _ = CardEnchantApi.TryForceApplyRandomKnownEnchant(target, 1, Owner);
            hand.Remove(target);
        }

        return Task.CompletedTask;
    }

    protected override void OnUpgrade()
    {
        DynamicVars["EnchantCount"].UpgradeValueBy(1);
    }
}

