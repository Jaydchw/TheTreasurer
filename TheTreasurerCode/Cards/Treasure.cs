using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using TheTreasurer.TheTreasurerCode.Powers;
using TheTreasurer.TheTreasurerCode.Relics;

namespace TheTreasurer.TheTreasurerCode.Cards;

public class Treasure : TheTreasurerCard
{
    public Treasure() : base(
        cost: 2,
        type: CardType.Skill,
        rarity: CardRarity.Basic,
        target: TargetType.None)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        _ = await ResinRelicRegistry.CreateRandomResinRelic(Owner, commonOnly: false);
        await PowerCmd.Apply<ResinRelicCleanupPower>(choiceContext, Owner.Creature, 1, Owner.Creature, this, silent: true);
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
