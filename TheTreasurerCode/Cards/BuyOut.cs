using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using System.Reflection;
using System.Threading.Tasks;
using TheTreasurer.TheTreasurerCode.Powers;
using TheTreasurer.TheTreasurerCode.Relics;

namespace TheTreasurer.TheTreasurerCode.Cards;

public class BuyOut : TheTreasurerCard
{
    public BuyOut() : base(-1, CardType.Skill, CardRarity.Rare, TargetType.None)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        var xSpent = ResolveXSpent(play);
        var count = xSpent + (IsUpgraded ? 1 : 0);
        if (count <= 0)
        {
            return;
        }

        var goldLoss = count * 15;
        if (Owner.Gold > 0)
        {
            await PlayerCmd.LoseGold(System.Math.Min(goldLoss, Owner.Gold), Owner);
        }

        var createdAny = false;
        for (var i = 0; i < count; i++)
        {
            var created = await ResinRelicRegistry.CreateResinRelic(Owner, new ResinRelicRegistry.ResinCreateRequest());
            if (created != null)
            {
                createdAny = true;
            }
        }

        await CardPileCmd.Draw(choiceContext, count, Owner);

        if (createdAny)
        {
            await PowerCmd.Apply<ResinRelicCleanupPower>(choiceContext, Owner.Creature, 1, Owner.Creature, this, silent: true);
        }
    }

    private static int ResolveXSpent(CardPlay play)
    {
        var playType = play.GetType();
        var prop = playType.GetProperty("EnergySpent", BindingFlags.Public | BindingFlags.Instance)
                   ?? playType.GetProperty("XValue", BindingFlags.Public | BindingFlags.Instance)
                   ?? playType.GetProperty("EnergyUsed", BindingFlags.Public | BindingFlags.Instance);

        if (prop?.GetValue(play) is int value && value >= 0)
        {
            return value;
        }

        return 0;
    }
}
