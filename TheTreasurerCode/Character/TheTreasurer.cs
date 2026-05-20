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
    public static readonly Color Color = new("ffdd00");

    public override Color NameColor => Color;
    public override CharacterGender Gender => CharacterGender.Neutral;
    public override int StartingHp => 70;

    public override IEnumerable<CardModel> StartingDeck => [
        ModelDb.Card<StrikeTreasurer>(),
        ModelDb.Card<StrikeTreasurer>(),
        ModelDb.Card<StrikeTreasurer>(),
        ModelDb.Card<StrikeTreasurer>(),
        ModelDb.Card<DefendTreasurer>(),
        ModelDb.Card<DefendTreasurer>(),
        ModelDb.Card<DefendTreasurer>(),
        ModelDb.Card<DefendTreasurer>(),
        ModelDb.Card<Cream>(),
        ModelDb.Card<Treasure>()
    ];

    public override IReadOnlyList<RelicModel> StartingRelics => [ ModelDb.Relic<StarterResinPress>() ];
    public override CardPoolModel CardPool => ModelDb.CardPool<TheTreasurerCardPool>();
    public override RelicPoolModel RelicPool => ModelDb.RelicPool<TheTreasurerRelicPool>();
    public override PotionPoolModel PotionPool => ModelDb.PotionPool<TheTreasurerPotionPool>();

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
