using BaseLib.Abstracts;
using TheTreasurer.TheTreasurerCode.Extensions;
using Godot;

namespace TheTreasurer.TheTreasurerCode.Character;

public class TheTreasurerCardPool : CustomCardPoolModel
{
    public override string Title => TheTreasurer.CharacterId;
    public override string BigEnergyIconPath => "charui/big_energy.png".ImagePath();
    public override string TextEnergyIconPath => "charui/text_energy.png".ImagePath();

    public override float H => 0.145f;
    public override float S => 1.00f;
    public override float V => 1.00f;

    public override Color DeckEntryCardColor => TheTreasurer.Color;
    public override bool IsColorless => false;
}
