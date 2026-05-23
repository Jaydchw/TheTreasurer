using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Enchantments;
using TheTreasurer.TheTreasurerCode.Powers;

namespace TheTreasurer.TheTreasurerCode.Cards;

public class ReRoll : TheTreasurerCard
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        ..HoverTipFactory.FromEnchantment<Slither>(1)
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new IntVar("CostThreshold", 3)
    ];

    public ReRoll() : base(1, CardType.Power, CardRarity.Uncommon, TargetType.None)
    {
    }

    protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        return PowerCmd.Apply<RerollPower>(choiceContext, Owner.Creature, DynamicVars["CostThreshold"].BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["CostThreshold"].UpgradeValueBy(-1);
    }
}
