using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using TheTreasurer.TheTreasurerCode.Powers;

namespace TheTreasurer.TheTreasurerCode.Cards;

public class ResinShield : TheTreasurerCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new IntVar("DexGain", 1)
    ];

    public ResinShield() : base(1, CardType.Power, CardRarity.Uncommon, TargetType.None)
    {
    }

    protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        return PowerCmd.Apply<ResinShieldPower>(choiceContext, Owner.Creature, DynamicVars["DexGain"].BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["DexGain"].UpgradeValueBy(1);
    }
}

