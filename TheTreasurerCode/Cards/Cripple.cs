using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using TheTreasurer.TheTreasurerCode.Powers;

namespace TheTreasurer.TheTreasurerCode.Cards;

public class Cripple : TheTreasurerCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new IntVar("WeakAmt", 1),
        new IntVar("VulnAmt", 1),
        new IntVar("StrDownAmt", 1)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<WeakPower>(),
        HoverTipFactory.FromPower<VulnerablePower>(),
        HoverTipFactory.FromPower<StrengthPower>(),
        HoverTipFactory.FromPower<TempStrengthDownPower>()
    ];

    public Cripple() : base(0, CardType.Skill, CardRarity.Common, TargetType.AnyEnemy)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        if (play.Target == null)
        {
            return;
        }

        await PowerCmd.Apply<WeakPower>(choiceContext, play.Target, DynamicVars["WeakAmt"].BaseValue, Owner.Creature, this);
        await PowerCmd.Apply<VulnerablePower>(choiceContext, play.Target, DynamicVars["VulnAmt"].BaseValue, Owner.Creature, this);
        await PowerCmd.Apply<TempStrengthDownPower>(choiceContext, play.Target, DynamicVars["StrDownAmt"].BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["WeakAmt"].UpgradeValueBy(1);
        DynamicVars["VulnAmt"].UpgradeValueBy(1);
        DynamicVars["StrDownAmt"].UpgradeValueBy(1);
    }
}
