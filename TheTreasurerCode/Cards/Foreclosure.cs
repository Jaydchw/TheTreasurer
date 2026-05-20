using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace TheTreasurer.TheTreasurerCode.Cards;

public class Foreclosure : TheTreasurerCard
{
    public Foreclosure() : base(3, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        var damage = Owner.Gold / 2;
        if (damage > 0)
        {
            if (IsUpgraded && CombatState != null)
            {
                var enemies = CombatState.Creatures.Where(c => c.IsAlive && c.Side != Owner.Creature.Side).ToList();
                foreach (var enemy in enemies)
                {
                    await DamageCmd.Attack(damage)
                        .FromCard(this)
                        .Targeting(enemy)
                        .WithHitFx("vfx/vfx_attack_blunt")
                        .Execute(choiceContext);
                }
            }
            else if (play.Target != null)
            {
                await DamageCmd.Attack(damage)
                    .FromCard(this)
                    .Targeting(play.Target)
                    .WithHitFx("vfx/vfx_attack_blunt")
                    .Execute(choiceContext);
            }
        }

        if (Owner.Gold > 0)
        {
            await PlayerCmd.LoseGold(Owner.Gold, Owner);
        }
    }
}
