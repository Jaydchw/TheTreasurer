using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using TheTreasurer.TheTreasurerCode.Relics;

namespace TheTreasurer.TheTreasurerCode.Cards;

public class WindowShop : TheTreasurerCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new IntVar("Choices", 2)
    ];

    public WindowShop() : base(2, CardType.Skill, CardRarity.Uncommon, TargetType.None)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        var pool = ResinRelicRegistry.GetVanillaRelicPool().ToList();
        if (pool.Count == 0)
        {
            return;
        }

        var picks = new List<MegaCrit.Sts2.Core.Models.RelicModel>();
        for (var i = 0; i < DynamicVars["Choices"].BaseValue && pool.Count > 0; i++)
        {
            var pick = Owner.RunState.Rng.TreasureRoomRelics.NextItem(pool);
            if (pick == null)
            {
                break;
            }

            picks.Add(pick);
            pool.Remove(pick);
        }

        if (picks.Count == 0)
        {
            return;
        }

        // No dedicated relic-choice UI command; choose one from the offered set randomly.
        var chosen = Owner.RunState.Rng.TreasureRoomRelics.NextItem(picks);
        if (chosen != null)
        {
            _ = await ResinRelicRegistry.CreateResinFromTemplate(Owner, chosen);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Choices"].UpgradeValueBy(1);
    }
}
