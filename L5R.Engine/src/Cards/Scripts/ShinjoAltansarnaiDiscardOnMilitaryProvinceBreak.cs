using L5R.Engine.Abilities;
using L5R.Engine.Dsl.GameActions;

namespace L5R.Engine.Cards.Scripts;

/// <summary>
/// shinjo-altansarnai: after breaking a province during a military conflict while
/// attacking, the opponent discards one of their own characters ("event.conflict.
/// conflictType === 'military' && context.source.isAttacking()"). Province-breaking isn't
/// otherwise modeled in this engine, so the caller asserts it happened the same way every
/// other afterConflict/onBreakProvince reaction in this backlog does - by setting up
/// Conflict.ConflictType and Attackers directly before invoking the script. The chosen
/// target (context.Target, caller-supplied) must be controlled by the opponent; the effect
/// reuses DiscardFromPlayGameActionHandler directly.
/// </summary>
public sealed class ShinjoAltansarnaiDiscardOnMilitaryProvinceBreak : ICardScript
{
    public void Execute(AbilityContext context)
    {
        var altansarnai = context.Source;
        var conflict = context.Game.CurrentConflict
            ?? throw new InvalidOperationException($"'{altansarnai.Id}' requires an active conflict.");

        if (conflict.ConflictType != "military")
            throw new InvalidOperationException($"'{altansarnai.Id}' can only trigger when a military province is broken.");

        if (!conflict.Attackers.Contains(altansarnai))
            throw new InvalidOperationException($"'{altansarnai.Id}' can only trigger while attacking.");

        var target = context.Target
            ?? throw new InvalidOperationException($"'{altansarnai.Id}' requires context.Target to be set.");

        if (target.Controller != context.Game.Opponent(context.Player))
            throw new InvalidOperationException($"'{target.Id}' must be controlled by the opponent.");

        new DiscardFromPlayGameActionHandler().Execute(context, null);
    }
}
