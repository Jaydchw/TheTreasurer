using BaseLib.Abstracts;
using TheTreasurer.TheTreasurerCode.Extensions;
using Godot;

namespace TheTreasurer.TheTreasurerCode.Character;

public class TheTreasurerRelicPool : CustomRelicPoolModel
{
    public override Color LabOutlineColor => TheTreasurer.Color;

    public override string BigEnergyIconPath => "charui/big_energy.png".ImagePath();
    public override string TextEnergyIconPath => "charui/text_energy.png".ImagePath();
}