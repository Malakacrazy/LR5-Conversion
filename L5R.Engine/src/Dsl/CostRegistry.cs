using L5R.Engine.Dsl.Costs;

namespace L5R.Engine.Dsl;

/// <summary>
/// Maps a costs.js name to its executable handler. Deliberately small - grows one entry
/// at a time as a real ported card needs the next cost, mirroring how RingtekiCatalog's
/// name lists were transcribed wholesale but execution is built incrementally.
/// </summary>
public sealed class CostRegistry
{
    private readonly Dictionary<string, ICostHandler> _handlers = new()
    {
        ["sacrificeSelf"] = new SacrificeSelfCostHandler(),
        ["bowSelf"] = new BowSelfCostHandler(),
        ["sacrifice"] = new SacrificeCostHandler(),
        ["discardCard"] = new DiscardCardCostHandler(),
        ["payHonor"] = new PayHonorCostHandler(),
        ["removeFateFromSelf"] = new RemoveFateFromSelfCostHandler()
    };

    public ICostHandler Resolve(string name) =>
        _handlers.TryGetValue(name, out var handler)
            ? handler
            : throw new NotSupportedException($"No executable handler registered for cost '{name}' yet.");
}
