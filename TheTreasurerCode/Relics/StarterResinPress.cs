using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Relics;
using TheTreasurer.TheTreasurerCode.Relics;

namespace TheTreasurer.TheTreasurerCode.Relics;

public class StarterResinPress : TheTreasurerRelic
{
    public override RelicRarity Rarity => RelicRarity.Starter;

    public override async Task BeforeCombatStart()
    {
        _ = await ResinRelicRegistry.CreateResinRelic(
            Owner,
            new ResinRelicRegistry.ResinCreateRequest(Rarity: RelicRarity.Common));
    }

    public override Task AfterCombatEnd(MegaCrit.Sts2.Core.Rooms.CombatRoom room)
    {
        return ResinRelicRegistry.Cleanup(Owner);
    }
}
