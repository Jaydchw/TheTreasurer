using BaseLib.Abstracts;
using BaseLib.Utils.NodeFactories;
using TheTreasurer.TheTreasurerCode.Cards;
using TheTreasurer.TheTreasurerCode.Extensions;
using TheTreasurer.TheTreasurerCode.Relics;
using Godot;
using MegaCrit.Sts2.Core.Entities.Characters;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Relics;

namespace TheTreasurer.TheTreasurerCode.Character;
public class TheTreasurer : PlaceholderCharacterModel
{
    public const string CharacterId = "TheTreasurer";
    
    public static readonly Color Color = new("ffffff");

    public override Color NameColor => Color;
    public override CharacterGender Gender => CharacterGender.Neutral;
    public override int StartingHp => 70;
    
    public override IEnumerable<CardModel> StartingDeck => [
        ModelDb.Card<Cream>(),
        ModelDb.Card<Treasure>(),
        ModelDb.Card<StrikeTreasurer>(),
        ModelDb.Card<StrikeTreasurer>(),
        ModelDb.Card<StrikeTreasurer>(),
        ModelDb.Card<StrikeTreasurer>(),
        ModelDb.Card<DefendTreasurer>(),
        ModelDb.Card<DefendTreasurer>(),
        ModelDb.Card<DefendTreasurer>(),
        ModelDb.Card<DefendTreasurer>()
    ];

    public override IReadOnlyList<RelicModel> StartingRelics =>
    [
        ModelDb.Relic<StarterResinPress>()
    ];
    
    public override CardPoolModel CardPool => ModelDb.CardPool<TheTreasurerCardPool>();
    public override RelicPoolModel RelicPool => ModelDb.RelicPool<TheTreasurerRelicPool>();
    public override PotionPoolModel PotionPool => ModelDb.PotionPool<TheTreasurerPotionPool>();
    
    /*  PlaceholderCharacterModel will utilize placeholder basegame assets for most of your character assets until you
        override all the other methods that define those assets. 
        These are just some of the simplest assets, given some placeholders to differentiate your character with. 
        You don't have to, but you're suggested to rename these images. */
    public override Control CustomIcon
    {
        get
        {
            var icon = NodeFactory<Control>.CreateFromResource(CustomIconTexturePath);
            icon.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
            return icon;
        }
    }
    public override string CustomIconTexturePath => "character_icon_char_name.png".CharacterUiPath();
    public override string CustomCharacterSelectIconPath => "char_select_char_name.png".CharacterUiPath();
    public override string CustomCharacterSelectLockedIconPath => "char_select_char_name_locked.png".CharacterUiPath();
    public override string CustomMapMarkerPath => "map_marker_char_name.png".CharacterUiPath();
}
