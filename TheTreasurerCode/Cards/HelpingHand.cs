using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Enchantments;
using MegaCrit.Sts2.Core.ValueProps;
using System.Linq;

namespace TheTreasurer.TheTreasurerCode.Cards;

public class HelpingHand : TheTreasurerCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new BlockVar(6, ValueProp.Move)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        HoverTipFactory.FromEnchantment<Nimble>(2);

    public HelpingHand() : base(1, CardType.Skill, CardRarity.Common, TargetType.None)
    {
    }

    protected override bool HasRequiredPlayTargets()
    {
        if (Owner == null)
        {
            return true;
        }

        return PileType.Hand.GetPile(Owner).Cards.Any(c => CardEnchantApi.CanApplyNimble(c));
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, play);

        var selected = await CardEnchantApi.SelectEnchantableHandCard<Nimble>(choiceContext, Owner);
        if (selected == null)
        {
            return;
        }

        _ = CardEnchantApi.TryApplyNimble(selected, 2);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(2);
    }
}
