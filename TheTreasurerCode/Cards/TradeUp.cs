using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using TheTreasurer.TheTreasurerCode.Relics;

namespace TheTreasurer.TheTreasurerCode.Cards;

public class TradeUp : TheTreasurerCard
{
    public TradeUp() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.None)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        var nonResin = Owner.Relics.Where(r => !ResinRelicRegistry.IsResin(r)).ToList();
        if (nonResin.Count == 0)
        {
            return;
        }

        var source = Owner.RunState.Rng.TreasureRoomRelics.NextItem(nonResin);
        if (source == null)
        {
            return;
        }

        var rarity = source.Rarity;
        await RelicCmd.Remove(source);
        for (var i = 0; i < 3; i++)
        {
            _ = await ResinRelicRegistry.CreateResinRelic(Owner, new ResinRelicRegistry.ResinCreateRequest(Rarity: rarity));
        }
    }
}
