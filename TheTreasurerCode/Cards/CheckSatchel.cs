using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using TheTreasurer.TheTreasurerCode.Powers;
using TheTreasurer.TheTreasurerCode.Relics;

namespace TheTreasurer.TheTreasurerCode.Cards;

public class CheckSatchel : TheTreasurerCard
{
    public CheckSatchel() : base(
        cost: 1,
        type: CardType.Skill,
        rarity: CardRarity.Common,
        target: TargetType.None)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        var created = await ResinRelicRegistry.CreateRandomResinRelic(Owner, commonOnly: true);
        if (created != null)
        {
            await PowerCmd.Apply<ResinRelicCleanupPower>(choiceContext, Owner.Creature, 1, Owner.Creature, this, silent: true);
        }
    }

    protected override PileType GetResultPileTypeForCardPlay()
    {
        if (!IsUpgraded)
        {
            return PileType.Exhaust;
        }

        return base.GetResultPileTypeForCardPlay();
    }
}
