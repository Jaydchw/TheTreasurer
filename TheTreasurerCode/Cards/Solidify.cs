using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using System.Collections.Generic;
using System.Threading.Tasks;
using TheTreasurer.TheTreasurerCode.Relics;

namespace TheTreasurer.TheTreasurerCode.Cards;

public class Solidify : TheTreasurerCard
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromKeyword(CardKeyword.Exhaust)
    ];

    public Solidify() : base(3, CardType.Skill, CardRarity.Rare, TargetType.None)
    {
    }

    protected override bool HasRequiredPlayTargets()
    {
        return Owner == null || ResinRelicRegistry.GetResinRelics(Owner).Count > 0;
    }

    protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        var resin = ResinRelicRegistry.GetResinRelics(Owner);
        if (resin.Count == 0)
        {
            return Task.CompletedTask;
        }

        var target = Owner.RunState.Rng.TreasureRoomRelics.NextItem(resin);
        if (target != null)
        {
            ResinRelicRegistry.Unregister(Owner, target);
        }

        return Task.CompletedTask;
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }

    protected override PileType GetResultPileTypeForCardPlay()
    {
        return PileType.Exhaust;
    }
}
