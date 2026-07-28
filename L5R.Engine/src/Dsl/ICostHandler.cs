using System.Text.Json;
using L5R.Engine.Abilities;

namespace L5R.Engine.Dsl;

/// <summary>
/// Executes one costs.js entry: can it currently be paid, and paying it (mutating state).
/// Distinct from Abilities.ICost (the task-8 legal-actions spike), which only checks
/// affordability and never mutates anything.
/// </summary>
public interface ICostHandler
{
    bool CanPay(AbilityContext context, JsonElement? parameters);

    void Pay(AbilityContext context, JsonElement? parameters);
}
