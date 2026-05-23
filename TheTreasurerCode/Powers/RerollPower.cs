using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models.Enchantments;
using TheTreasurer.TheTreasurerCode.Cards;

namespace TheTreasurer.TheTreasurerCode.Powers;

public class RerollPower : TheTreasurerPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    public Task AfterSideTurnStart(CombatSide side, ICombatState combatState)
    {
        if (!Owner.IsPlayer || Owner.Player == null || side != CombatSide.Player)
        {
            return Task.CompletedTask;
        }

        var target = PileType.Hand.GetPile(Owner.Player).Cards
            .FirstOrDefault(c => c.Enchantment == null && CardEnchantApi.CanApply<Slither>(c));
        if (target != null)
        {
            _ = CardEnchantApi.TryApply<Slither>(target, 1);
        }

        return Task.CompletedTask;
    }
}
