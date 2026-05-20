using BaseLib.Abstracts;
using BaseLib.Extensions;
using BaseLib.Utils;
using TheTreasurer.TheTreasurerCode.Character;
using TheTreasurer.TheTreasurerCode.Extensions;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using System.Threading.Tasks;
using TheTreasurer.TheTreasurerCode.Relics;

namespace TheTreasurer.TheTreasurerCode.Cards;

[Pool(typeof(TheTreasurerCardPool))]
public abstract class TheTreasurerCard(int cost, CardType type, CardRarity rarity, TargetType target) :
    CustomCardModel(cost, type, rarity, target)
{
    //Image size:
    //Normal art: 1000x760 (Using 500x380 should also work, it will simply be scaled.)
    //Full art: 606x852
    public override string CustomPortraitPath => $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".BigCardImagePath();
    
    //Smaller variants of card images for efficiency:
    //Smaller variant of fullart: 250x350
    //Smaller variant of normalart: 250x190
    
    //Uses card_portraits/card_name.png as image path. These should be smaller images.
    public override string PortraitPath => $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".CardImagePath();
    public override string BetaPortraitPath => $"beta/{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".CardImagePath();

    protected virtual void ApplySpawnEnchantment()
    {
    }

    protected virtual bool RequiresResinRelicToPlay => false;
    protected virtual bool HasRequiredPlayTargets() => true;

    protected void EnsureSelfEnchantment<T>(int amount) where T : EnchantmentModel
    {
        if (Enchantment != null && Enchantment.GetType() == typeof(T) && Enchantment.Amount == amount)
        {
            return;
        }

        if (Enchantment != null)
        {
            ClearEnchantmentInternal();
        }

        EnchantInternal(ModelDb.Enchantment<T>().ToMutable(), amount);
    }

    public override Task AfterCardEnteredCombat(CardModel card)
    {
        if (ReferenceEquals(card, this))
        {
            ApplySpawnEnchantment();
        }

        return Task.CompletedTask;
    }

    public override Task AfterCardChangedPiles(CardModel card, PileType pileType, AbstractModel? source)
    {
        if (ReferenceEquals(card, this))
        {
            ApplySpawnEnchantment();
        }

        return Task.CompletedTask;
    }

    public override Task BeforeCardPlayed(CardPlay play)
    {
        if (ReferenceEquals(play.Card, this))
        {
            ApplySpawnEnchantment();
        }

        return Task.CompletedTask;
    }

    protected override void AfterCloned()
    {
        base.AfterCloned();
        if (!IsCanonical)
        {
            ApplySpawnEnchantment();
        }
    }

    public override bool ShouldPlay(CardModel card, AutoPlayType autoPlayType)
    {
        if (ReferenceEquals(card, this) && RequiresResinRelicToPlay && Owner != null)
        {
            if (ResinRelicRegistry.GetResinRelics(Owner).Count == 0)
            {
                return false;
            }
        }

        if (ReferenceEquals(card, this) && !HasRequiredPlayTargets())
        {
            return false;
        }

        return base.ShouldPlay(card, autoPlayType);
    }
}
