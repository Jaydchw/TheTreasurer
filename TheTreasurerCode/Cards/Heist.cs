using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using TheTreasurer.TheTreasurerCode.Relics;

namespace TheTreasurer.TheTreasurerCode.Cards;

public class Heist : TheTreasurerCard
{
    public Heist() : base(2, CardType.Skill, CardRarity.Uncommon, TargetType.None)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        var resin = Owner.RunState.Rng.TreasureRoomRelics.NextItem(ResinRelicRegistry.GetResinRelics(Owner));
        if (resin == null)
        {
            return;
        }

        var bonus = resin.Rarity switch
        {
            RelicRarity.Common => 1,
            RelicRarity.Uncommon => 2,
            RelicRarity.Rare => 3,
            RelicRarity.Shop => 2,
            _ => 1
        };

        await ResinRelicRegistry.DestroyResinRelic(Owner, resin);
        await PowerCmd.Apply<StrengthPower>(choiceContext, Owner.Creature, bonus, Owner.Creature, this);
        await PowerCmd.Apply<DexterityPower>(choiceContext, Owner.Creature, bonus, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }

    protected override PileType GetResultPileTypeForCardPlay() => PileType.Exhaust;
}
