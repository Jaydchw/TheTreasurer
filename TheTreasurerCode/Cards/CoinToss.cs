using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace TheTreasurer.TheTreasurerCode.Cards;

public class CoinToss : TheTreasurerCard
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromKeyword(CardKeyword.Exhaust)
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(3, ValueProp.Move),
        new IntVar("Hits", 2),
        new IntVar("GoldGain", 2)
    ];

    public CoinToss() : base(1, CardType.Attack, CardRarity.Common, TargetType.None)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        if (CombatState == null)
        {
            return;
        }

        var enemies = CombatState.Creatures.Where(c => c.IsAlive && c.Side != Owner.Creature.Side).ToList();
        if (enemies.Count == 0)
        {
            return;
        }

        for (var i = 0; i < DynamicVars["Hits"].BaseValue; i++)
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

        await PlayerCmd.GainGold(DynamicVars["GoldGain"].BaseValue, Owner);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Hits"].UpgradeValueBy(1);
        DynamicVars["GoldGain"].UpgradeValueBy(1);
    }

    protected override PileType GetResultPileTypeForCardPlay()
    {
        return PileType.Exhaust;
    }
}
