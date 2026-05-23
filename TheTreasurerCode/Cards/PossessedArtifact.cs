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

public class PossessedArtifact : TheTreasurerCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new IntVar("PerEnchant", 3),
        new IntVar("PerResinBlock", 4)
    ];

    public PossessedArtifact() : base(2, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        if (play.Target == null)
        {
            return;
        }

        var enchanted = Owner.Deck.Cards.Count(c => c.Enchantment != null);
        var damage = enchanted * DynamicVars["PerEnchant"].BaseValue;
        if (damage > 0)
        {
            await DamageCmd.Attack(damage)
                .FromCard(this)
                .Targeting(play.Target)
                .WithHitFx("vfx/vfx_attack_blunt")
                .Execute(choiceContext);
        }

        var resinCount = ResinRelicRegistry.GetResinRelics(Owner).Count;
        for (var i = 0; i < resinCount; i++)
        {
            await CreatureCmd.GainBlock(Owner.Creature, new BlockVar(DynamicVars["PerResinBlock"].BaseValue, ValueProp.Move), play);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["PerEnchant"].UpgradeValueBy(1);
        DynamicVars["PerResinBlock"].UpgradeValueBy(1);
    }
}
