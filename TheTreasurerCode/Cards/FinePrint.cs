using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Enchantments;
using System.Linq;

namespace TheTreasurer.TheTreasurerCode.Cards;

public class FinePrint : TheTreasurerCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new IntVar("GoldLoss", 20)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        HoverTipFactory.FromEnchantment<Spiral>(2);

    public FinePrint() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.None)
    {
    }

    protected override bool HasRequiredPlayTargets()
    {
        if (Owner == null)
        {
            return true;
        }

        return PileType.Hand.GetPile(Owner).Cards.Any(c => CardEnchantApi.CanApply<Spiral>(c));
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        await PlayerCmd.LoseGold(DynamicVars["GoldLoss"].BaseValue, Owner);

        var selected = await CardEnchantApi.SelectEnchantableHandCard<Spiral>(choiceContext, Owner);
        if (selected == null)
        {
            return;
        }

        _ = CardEnchantApi.TryApply<Spiral>(selected, 2);
    }

    protected override void OnUpgrade() { }
}
