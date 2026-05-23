using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using TheTreasurer.TheTreasurerCode.Relics;

namespace TheTreasurer.TheTreasurerCode.Cards;

public class Exchange : TheTreasurerCard
{
    public Exchange() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.None)
    {
    }

    protected override bool HasRequiredPlayTargets()
    {
        return Owner == null || PileType.Hand.GetPile(Owner).Cards.Any(c => c.Enchantment != null);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        var enchanted = PileType.Hand.GetPile(Owner).Cards.Where(c => c.Enchantment != null).ToList();
        if (enchanted.Count == 0)
        {
            return;
        }

        var prefs = new CardSelectorPrefs(CardSelectorPrefs.RemoveSelectionPrompt, 1);
        var selected = (await CardSelectCmd.FromHandForDiscard(choiceContext, Owner, prefs, null, this)).FirstOrDefault();
        if (selected == null || !enchanted.Contains(selected))
        {
            return;
        }

        CardCmd.ClearEnchantment(selected);
        _ = await ResinRelicRegistry.CreateResinRelic(Owner, new ResinRelicRegistry.ResinCreateRequest());
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}

