using L5R.Engine.Abilities;
using L5R.Engine.Dsl.GameActions;

namespace L5R.Engine.Cards.Scripts;

/// <summary>
/// strength-in-numbers: while its controller is the attacking player, send a defending
/// character home if its (effective) glory is at most the number of attacking characters.
/// "Is the acting player the attacking player" has no equivalent predicate (isDuringConflict's
/// type param scopes the conflict's type/element, not which side the acting player is on) -
/// checked directly against Conflict.AttackingPlayer here instead.
/// </summary>
public sealed class StrengthInNumbersSendHomeLowGloryDefender : ICardScript
{
    public void Execute(AbilityContext context)
    {
        var source = context.Source;
        var conflict = context.Game.CurrentConflict
            ?? throw new InvalidOperationException($"'{source.Id}' requires an active conflict.");

        if (conflict.AttackingPlayer != context.Player)
            throw new InvalidOperationException($"'{source.Id}' can only be used while its controller is the attacking player.");

        var target = context.Target
            ?? throw new InvalidOperationException($"'{source.Id}' requires context.Target to be set.");

        if (!conflict.Defenders.Contains(target))
            throw new InvalidOperationException($"'{target.Id}' is not defending.");

        if (context.Game.EffectiveGlory(target) > conflict.Attackers.Count)
            throw new InvalidOperationException($"'{target.Id}''s glory exceeds the number of attacking characters.");

        new SendHomeGameActionHandler().Execute(context, null);
    }
}
