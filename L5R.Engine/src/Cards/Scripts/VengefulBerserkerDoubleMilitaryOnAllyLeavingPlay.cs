using L5R.Engine.Abilities;
using L5R.Engine.State;

namespace L5R.Engine.Cards.Scripts;

/// <summary>
/// vengeful-berserker: after another character you control leaves play during a
/// conflict, double this character's military skill until the end of the conflict.
/// ringteki's event.cardStateWhenLeftPlay is a snapshot of the departed character taken
/// at the moment it left play - this engine has no event bus to capture that snapshot, so
/// the caller sets context.Target directly to the character that just left play (trust-
/// the-caller, same convention as every other event-shaped script this session). Reuses
/// the existing LastingEffect.Multiplier mechanism directly (way-of-the-lion's own
/// modifyBaseMilitarySkillMultiplier plumbing), applied to context.Source rather than a
/// player-chosen target.
/// </summary>
public sealed class VengefulBerserkerDoubleMilitaryOnAllyLeavingPlay : ICardScript
{
    public void Execute(AbilityContext context)
    {
        var berserker = context.Source;

        if (context.Game.CurrentConflict is null)
            throw new InvalidOperationException($"'{berserker.Id}' can only trigger during a conflict.");

        var departed = context.Target
            ?? throw new InvalidOperationException($"'{berserker.Id}' requires context.Target (the character that left play) to be set.");

        if (departed.Type != CardType.Character)
            throw new InvalidOperationException($"'{departed.Id}' must be a character.");

        if (departed.Controller != context.Player)
            throw new InvalidOperationException($"'{departed.Id}' must be controlled by '{berserker.Id}''s controller.");

        context.Game.LastingEffects.Add(new LastingEffect { Target = berserker, Stat = "military", Value = 0, Multiplier = 2, Duration = "untilEndOfConflict" });
    }
}
