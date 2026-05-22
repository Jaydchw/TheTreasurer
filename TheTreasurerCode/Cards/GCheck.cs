using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace TheTreasurer.TheTreasurerCode.Cards;

public class GCheck : TheTreasurerCard
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromKeyword(CardKeyword.Exhaust)
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(15, ValueProp.Move),
        ..CustomCardModel.MakeCalculatedDamage(
            baseVal: 15,
            bonus: CalculateConditionalDamageBonus,
            mult: 1,
            props: ValueProp.Move)
    ];

    public GCheck() : base(3, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        if (play.Target == null)
        {
            return;
        }

        var damage = DynamicVars.Damage.BaseValue + CalculateConditionalDamageBonus(this, play.Target);

        await DamageCmd.Attack(damage)
            .FromCard(this)
            .Targeting(play.Target)
            .WithHitFx("vfx/vfx_attack_blunt")
            .Execute(choiceContext);

        if (play.Target.Monster?.IntendsToAttack == true)
        {
            await PlayerCmd.GainGold(10, Owner);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(5);
    }

    protected override PileType GetResultPileTypeForCardPlay()
    {
        return PileType.Exhaust;
    }

    private static decimal CalculateConditionalDamageBonus(CardModel card, Creature? target)
    {
        if (card is not GCheck gCheck || target?.Monster?.IntendsToAttack != true)
        {
            return 0;
        }

        return gCheck.DynamicVars.Damage.BaseValue;
    }
}
