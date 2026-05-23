using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace TheTreasurer.TheTreasurerCode.Cards;

public class Overdraft : TheTreasurerCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(30, ValueProp.Move),
        new IntVar("GoldLoss", 50),
        new IntVar("Refund", 30)
    ];

    public Overdraft() : base(2, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        if (play.Target == null)
        {
            return;
        }

        var loss = DynamicVars["GoldLoss"].BaseValue;
        if (Owner.Gold > 0)
        {
            await PlayerCmd.LoseGold(System.Math.Min(loss, Owner.Gold), Owner);
        }

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this)
            .Targeting(play.Target)
            .WithHitFx("vfx/vfx_attack_blunt")
            .Execute(choiceContext);

        var killed = play.Target.IsDead;
        if (killed)
        {
            await PlayerCmd.GainGold(DynamicVars["Refund"].BaseValue, Owner);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(10);
    }
}
