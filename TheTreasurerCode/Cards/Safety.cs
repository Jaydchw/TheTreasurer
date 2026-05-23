using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using TheTreasurer.TheTreasurerCode.Cards;

namespace TheTreasurer.TheTreasurerCode.Cards;

public class Safety : TheTreasurerCard
{
    public Safety() : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.None)
    {
    }

    protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        foreach (var card in PileType.Hand.GetPile(Owner).Cards)
        {
            _ = CardEnchantApi.TryApplyNimble(card, 2, allowNonBlocking: true);
        }

        return Task.CompletedTask;
    }
}

