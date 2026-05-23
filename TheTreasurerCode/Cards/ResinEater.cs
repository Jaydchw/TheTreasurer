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

public class ResinEater : TheTreasurerCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(8, ValueProp.Move),
        new IntVar("PermaGain", 5)
    ];

    public ResinEater() : base(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        if (play.Target == null)
        {
            return;
        }

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this)
            .Targeting(play.Target)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);

        var destroyed = await ResinRelicRegistry.DestroyResinRelic(Owner, new ResinRelicRegistry.ResinDestroyRequest());
        if (!destroyed)
        {
            return;
        }

        var gain = DynamicVars["PermaGain"].BaseValue;
        foreach (var card in Owner.Deck.Cards.Where(c => c.Id == Id))
        {
            card.DynamicVars.Damage.BaseValue += gain;
        }
        foreach (var card in Owner.PlayerCombatState.AllCards.Where(c => c.Id == Id))
        {
            card.DynamicVars.Damage.BaseValue += gain;
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["PermaGain"].UpgradeValueBy(1);
    }
}

