using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Enchantments;
using System.Collections.Generic;

namespace TheTreasurer.TheTreasurerCode.Cards;

public class ManaBuild : TheTreasurerCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new IntVar("EnergyGain", 1)
    ];

    public ManaBuild() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.None)
    {
    }

    protected override bool HasRequiredPlayTargets()
    {
        return Owner == null || PileType.Hand.GetPile(Owner).Cards.Any(c => c.Enchantment != null);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        var candidates = PileType.Hand.GetPile(Owner).Cards.Where(c => c.Enchantment != null).ToList();
        if (candidates.Count == 0)
        {
            return;
        }

        var prefs = new CardSelectorPrefs(CardSelectorPrefs.RemoveSelectionPrompt, 1);
        var selected = (await CardSelectCmd.FromHandForDiscard(choiceContext, Owner, prefs, null, source: this)).FirstOrDefault();
        if (selected == null)
        {
            return;
        }

        if (!candidates.Contains(selected))
        {
            return;
        }

        CardCmd.ClearEnchantment(selected);
        await PlayerCmd.GainEnergy(DynamicVars["EnergyGain"].BaseValue, Owner);
        await CardPileCmd.Draw(choiceContext, 2, Owner);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["EnergyGain"].UpgradeValueBy(1);
    }
}
