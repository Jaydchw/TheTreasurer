using BaseLib.Abstracts;
using BaseLib.Utils;
using TheTreasurer.TheTreasurerCode.Character;

namespace TheTreasurer.TheTreasurerCode.Potions;

[Pool(typeof(TheTreasurerPotionPool))]
public abstract class TheTreasurerPotion : CustomPotionModel;