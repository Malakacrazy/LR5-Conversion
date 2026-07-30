using L5R.Engine.Abilities;
using L5R.Engine.State;

namespace L5R.Engine.Cards.Scripts;

/// <summary>
/// togashi-yokuni: choose a printed ability on another character and gain a copy of it
/// until the end of the phase. Needs target.mode 'ability' (choose one of another card's
/// printed abilities) and copying an entire ability object, both explicitly outside the
/// closed predicate/gameAction vocabulary - matches this card's own scriptOverride reason.
/// context.ChosenAbility carries the specific ActionDefinition being copied (trust-the-
/// caller, since this engine has no live per-card action registry to pick from); the grant
/// itself is recorded in GameState.GainedAbilities, consumed later exactly like any other
/// ActionDefinition (AbilityExecutor.Execute with context.Source set to this character).
/// "max: perRound(1)" needs no work, matching every other "max"/"limit" field's established
/// no-op precedent.
/// </summary>
public sealed class TogashiYokuniCopyAnotherCharactersAbility : ICardScript
{
    public void Execute(AbilityContext context)
    {
        var yokuni = context.Source;

        var target = context.Target
            ?? throw new InvalidOperationException($"'{yokuni.Id}' requires context.Target (the character to copy from) to be set.");

        if (target == yokuni)
            throw new InvalidOperationException($"'{yokuni.Id}' cannot copy its own ability.");

        if (target.Type != CardType.Character)
            throw new InvalidOperationException($"'{target.Id}' must be a character.");

        var ability = context.ChosenAbility
            ?? throw new InvalidOperationException($"'{yokuni.Id}' requires context.ChosenAbility (the printed ability being copied) to be set.");

        context.Game.GainedAbilities.Add((yokuni, ability));
    }
}
