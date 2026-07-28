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
}
