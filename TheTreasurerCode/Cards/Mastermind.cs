using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using TheTreasurer.TheTreasurerCode.Powers;

namespace TheTreasurer.TheTreasurerCode.Cards;

public class Mastermind : TheTreasurerCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new IntVar("StrGain", 2)
    ];

    public Mastermind() : base(2, CardType.Power, CardRarity.Uncommon, TargetType.None)
    {
    }

    protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        return PowerCmd.Apply<MastermindPower>(choiceContext, Owner.Creature, DynamicVars["StrGain"].BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["StrGain"].UpgradeValueBy(1);
    }
}

