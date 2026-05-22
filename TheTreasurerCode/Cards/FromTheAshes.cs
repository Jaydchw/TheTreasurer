using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using System.Collections.Generic;
using System.Threading.Tasks;
using TheTreasurer.TheTreasurerCode.Powers;
using TheTreasurer.TheTreasurerCode.Relics;

namespace TheTreasurer.TheTreasurerCode.Cards;

public class FromTheAshes : TheTreasurerCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new IntVar("Hits", 2)
    ];

    public FromTheAshes() : base(2, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        if (play.Target == null)
        {
            return;
        }

        var hits = DynamicVars["Hits"].BaseValue;
        for (var i = 0; i < hits; i++)
        {
            await DamageCmd.Attack(8)
                .FromCard(this)
                .Targeting(play.Target)
                .WithHitFx("vfx/vfx_attack_slash")
                .Execute(choiceContext);

            if (!play.Target.IsAlive)
            {
                var created = await ResinRelicRegistry.CreateResinRelic(
                    Owner,
                    new ResinRelicRegistry.ResinCreateRequest(Rarity: RelicRarity.Rare));
                if (created != null)
                {
                    await PowerCmd.Apply<ResinRelicCleanupPower>(choiceContext, Owner.Creature, 1, Owner.Creature, this, silent: true);
                }

                break;
            }
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Hits"].UpgradeValueBy(1);
    }
}
