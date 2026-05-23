using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using TheTreasurer.TheTreasurerCode.Relics;

namespace TheTreasurer.TheTreasurerCode.Cards;

public class PowerFantasy : TheTreasurerCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new BlockVar(5, ValueProp.Move),
        new DamageVar(5, ValueProp.Move)
    ];

    public PowerFantasy() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.None)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        var resin = ResinRelicRegistry.GetResinRelics(Owner);
        var count = resin.Count;
        if (count <= 0)
        {
            return;
        }

        foreach (var relic in resin)
        {
            _ = await ResinRelicRegistry.DestroyResinRelic(Owner, relic);
        }

        for (var i = 0; i < count; i++)
        {
            await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, play);
        }

        if (CombatState == null)
        {
            return;
        }

        var enemies = CombatState.Creatures.Where(c => c.IsAlive && c.Side != Owner.Creature.Side).ToList();
        foreach (var enemy in enemies)
        {
            await DamageCmd.Attack(count * DynamicVars.Damage.BaseValue)
                .FromCard(this)
                .Targeting(enemy)
                .WithHitFx("vfx/vfx_attack_fire")
                .Execute(choiceContext);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(2);
        DynamicVars.Damage.UpgradeValueBy(2);
    }
}
