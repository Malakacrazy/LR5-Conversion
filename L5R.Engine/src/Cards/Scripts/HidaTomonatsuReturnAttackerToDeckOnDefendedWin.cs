using L5R.Engine.Abilities;
using L5R.Engine.Dsl;
using L5R.Engine.Dsl.GameActions;

namespace L5R.Engine.Cards.Scripts;

/// <summary>
/// hida-tomonatsu: after winning a conflict while defending, sacrifice this character to
/// return a non-unique attacking character to the top of its owner's deck (afterConflict
/// reaction, "event.conflict.winner === context.player && context.source.isDefending()").
/// context.Target is the caller-chosen attacker (trust-the-caller, same convention as every
/// other target in this engine) - cardType/controller/isUnique are re-checked here anyway
/// since a script has no separate ResolveLegalTargets-backed guarantee the way a JSON
/// target's cardCondition does. Cost (sacrificeSelf) and effect (returnToDeck) reuse
/// ZoneMover/ReturnToDeckGameActionHandler directly.
/// </summary>
public sealed class HidaTomonatsuReturnAttackerToDeckOnDefendedWin : ICardScript
{
    public void Execute(AbilityContext context)
    {
        var tomonatsu = context.Source;

        if (context.Game.CurrentConflict?.Winner != context.Player)
            throw new InvalidOperationException($"'{tomonatsu.Id}' can only trigger when its controller wins the conflict.");

        if (context.Game.CurrentConflict?.Defenders.Contains(tomonatsu) != true)
            throw new InvalidOperationException($"'{tomonatsu.Id}' can only trigger while defending.");

        var target = context.Target
            ?? throw new InvalidOperationException($"'{tomonatsu.Id}' requires context.Target to be set.");

        if (target.Unique)
            throw new InvalidOperationException($"'{target.Id}' is unique and cannot be returned by this effect.");

        if (context.Game.CurrentConflict?.Attackers.Contains(target) != true)
            throw new InvalidOperationException($"'{target.Id}' is not attacking.");

        ZoneMover.MoveTo(tomonatsu, tomonatsu.Controller.Discard, "discard");

        new ReturnToDeckGameActionHandler().Execute(context, null);
    }
}
