using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace TheTreasurer.TheTreasurerCode.Cards;

public class DebtCollect : TheTreasurerCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(8, ValueProp.Move)
    ];

    public DebtCollect() : base(0, CardType.Attack, CardRarity.Common, TargetType.None)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        if (CombatState != null)
        {
            var enemies = CombatState.Creatures.Where(c => c.IsAlive && c.Side != Owner.Creature.Side).ToList();
            if (enemies.Count > 0)
            {
                foreach (var enemy in enemies)
                {
                    await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
                        .FromCard(this)
                        .Targeting(enemy)
                        .WithHitFx("vfx/vfx_attack_blunt")
                        .Execute(choiceContext);
                }
            }
        }

        await PlayerCmd.LoseGold(5, Owner);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(4);
    }
}
