using System.Text.Json;
using L5R.Engine.Abilities;

namespace L5R.Engine.Dsl;

/// <summary>
/// Executes one GameActions.ts entry against context.Target (mutating state). Distinct
/// from the ~80-name RingtekiCatalog list, which only records that a name exists for
/// load-time validation - this is where a name actually does something.
/// </summary>
public interface IGameActionHandler
{
    void Execute(AbilityContext context, JsonElement? parameters);

    /// <summary>
    /// ringteki GameAction.canAffect: whether this action would currently do anything to
    /// context.Target. Matters when a card lists several gameActions as alternatives on
    /// the same target (e.g. against-the-waves' [bow, ready]) - only the currently-legal
    /// one(s) should actually run. Defaults to true for actions with no "already in this
    /// state" concept.
    /// </summary>
    bool CanAffect(AbilityContext context, JsonElement? parameters) => true;
}
