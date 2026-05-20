using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models.Enchantments;

namespace TheTreasurer.TheTreasurerCode.Cards;

public class BringBack : TheTreasurerCard
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        HoverTipFactory.FromEnchantment<Spiral>(1);

    public BringBack() : base(
        cost: 2,
        type: CardType.Skill,
        rarity: CardRarity.Common,
        target: TargetType.None)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        var discard = PileType.Discard.GetPile(Owner).Cards.ToList();
        if (discard.Count == 0)
        {
            return;
        }

        var prefs = new CardSelectorPrefs(new LocString("gameplay_ui", "SELECT_CARD"), 1);
        var selected = (await CardSelectCmd.FromSimpleGrid(choiceContext, discard, Owner, prefs)).FirstOrDefault();
        if (selected == null)
        {
            return;
        }

        await CardPileCmd.Add(selected, PileType.Hand);
        if (selected.Enchantment != null && selected.Enchantment.GetType() != typeof(Spiral))
        {
            CardCmd.ClearEnchantment(selected);
        }

        if (selected.Enchantment == null || selected.Enchantment.GetType() == typeof(Spiral))
        {
            CardCmd.Enchant<Spiral>(selected, 1);
        }
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
