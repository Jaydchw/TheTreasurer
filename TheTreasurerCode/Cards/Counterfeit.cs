using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using TheTreasurer.TheTreasurerCode.Powers;
using TheTreasurer.TheTreasurerCode.Relics;

namespace TheTreasurer.TheTreasurerCode.Cards;

public class Counterfeit : TheTreasurerCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new IntVar("Copies", 1)
    ];

    public Counterfeit() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.None)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        var count = DynamicVars["Copies"].BaseValue;
        for (var i = 0; i < count; i++)
        {
            var created = await ResinRelicRegistry.CopyRelic(
                Owner,
                new ResinRelicRegistry.RelicCopyRequest(
                    Scope: ResinRelicRegistry.RelicCopyScope.NonResinOnly));
            if (created != null)
            {
                await PowerCmd.Apply<ResinRelicCleanupPower>(choiceContext, Owner.Creature, 1, Owner.Creature, this, silent: true);
            }
            else
            {
                break;
            }
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Copies"].UpgradeValueBy(1);
    }
}

