using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace TheTreasurer.TheTreasurerCode.Powers;

public class CompoundInterestPower : TheTreasurerPower
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new IntVar("GainNow", 0)
    ];

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override Task AfterSideTurnStart(CombatSide side, ICombatState combatState)
    {
        if (side == CombatSide.Player && Owner.IsPlayer && Owner.Player != null)
        {
            DynamicVars["GainNow"].BaseValue = CalculateGain(Owner.Player, Amount);
        }

        return Task.CompletedTask;
    }

    public override async Task AfterTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
    {
        if (side != CombatSide.Player || !Owner.IsPlayer || Owner.Player == null || Amount <= 0)
        {
            return;
        }

        var gain = CalculateGain(Owner.Player, Amount);
        DynamicVars["GainNow"].BaseValue = gain;
        if (gain > 0)
        {
            await PlayerCmd.GainGold(gain, Owner.Player);
        }
    }

    public override Task AfterGoldGained(Player player)
    {
        if (Owner.IsPlayer && Owner.Player == player)
        {
            DynamicVars["GainNow"].BaseValue = CalculateGain(player, Amount);
        }

        return Task.CompletedTask;
    }

    private static int CalculateGain(Player player, int percent)
    {
        return Math.Min(100, (int)Math.Floor(player.Gold * (percent / 100m)));
    }
}
