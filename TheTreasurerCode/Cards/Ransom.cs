using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace TheTreasurer.TheTreasurerCode.Cards;

public class Ransom : TheTreasurerCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(3, ValueProp.Move),
        new IntVar("GoldStep", 30),
        ..CustomCardModel.MakeCalculatedDamage(
            baseVal: 3,
            bonus: CalculateGoldScaledBonus,
            mult: 1,
            props: ValueProp.Move)
    ];

    public Ransom() : base(
        cost: 1,
        type: CardType.Attack,
        rarity: CardRarity.Basic,
        target: TargetType.AnyEnemy)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        if (play.Target == null)
        {
            return;
        }

        var damage = ComputeDamage();
        if (damage <= 0)
        {
            return;
        }

        await DamageCmd.Attack(damage)
            .FromCard(this)
            .Targeting(play.Target)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["GoldStep"].UpgradeValueBy(-10);
    }

    private int ComputeDamage()
    {
        var divisor = GetGoldStep(this);
        if (divisor <= 0)
        {
            return 0;
        }

        return (int)DynamicVars.Damage.BaseValue + (int)(Owner.Gold / divisor);
    }

    private static int GetGoldStep(Ransom ransom)
    {
        return ransom.DynamicVars["GoldStep"].IntValue;
    }

    private static decimal CalculateGoldScaledBonus(CardModel card, Creature? _target)
    {
        if (card is not Ransom ransom || ransom.Owner == null)
        {
            return 0;
        }

        var divisor = GetGoldStep(ransom);
        if (divisor <= 0)
        {
            return 0;
        }

        return ransom.Owner.Gold / divisor;
    }
}
