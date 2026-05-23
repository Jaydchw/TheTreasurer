using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using TheTreasurer.TheTreasurerCode.Relics;

namespace TheTreasurer.TheTreasurerCode.Cards;

public class Repurpose : TheTreasurerCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new IntVar("DrawCount", 2)
    ];

    public Repurpose() : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.None)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        _ = await ResinRelicRegistry.DestroyResinRelic(Owner, new ResinRelicRegistry.ResinDestroyRequest());
        await PlayerCmd.GainEnergy(1, Owner);
        await CardPileCmd.Draw(choiceContext, DynamicVars["DrawCount"].BaseValue, Owner);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["DrawCount"].UpgradeValueBy(1);
    }
}

