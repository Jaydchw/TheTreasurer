using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace TheTreasurer.TheTreasurerCode.Powers;

public class RunicFlowPower : TheTreasurerPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay play)
    {
        if (!Owner.IsPlayer || Owner.Player == null || play.Card.Owner != Owner.Player || play.Card.Enchantment == null)
        {
            return;
        }

        await CreatureCmd.GainBlock(Owner, new BlockVar(Amount, ValueProp.Move), play);
    }
}
