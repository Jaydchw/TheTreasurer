using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Enchantments;
using System.Linq;
using System.Threading.Tasks;

namespace TheTreasurer.TheTreasurerCode.Cards;

public class Bankruptcy : TheTreasurerCard
{
    public Bankruptcy() : base(0, CardType.Skill, CardRarity.Rare, TargetType.None)
    {
    }

    protected override bool HasRequiredPlayTargets()
    {
        return Owner == null || Owner.Gold >= 50;
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        if (Owner.Gold > 0)
        {
            await PlayerCmd.LoseGold(Owner.Gold, Owner);
        }

        foreach (var card in PileType.Hand.GetPile(Owner).Cards.ToList())
        {
            if (card.Enchantment != null)
            {
                CardCmd.ClearEnchantment(card);
            }

            _ = CardEnchantApi.TryApply<Spiral>(card, 1);
        }
    }
}
