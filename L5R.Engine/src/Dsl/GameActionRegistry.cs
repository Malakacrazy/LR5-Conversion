using L5R.Engine.Dsl.GameActions;

namespace L5R.Engine.Dsl;

/// <summary>
/// Maps a GameActions.ts name to its executable handler. Deliberately small - grows one
/// entry at a time as a real ported card needs the next game action.
/// </summary>
public sealed class GameActionRegistry
{
    private readonly Dictionary<string, IGameActionHandler> _handlers = new()
    {
        ["placeFate"] = new PlaceFateGameActionHandler(),
        ["returnToHand"] = new ReturnToHandGameActionHandler(),
        ["draw"] = new DrawGameActionHandler(),
        ["gainFate"] = new GainFateGameActionHandler(),
        ["cardLastingEffect"] = new CardLastingEffectGameActionHandler(),
        ["discardFromPlay"] = new DiscardFromPlayGameActionHandler(),
        // ringteki GameActions.ts: sacrifice is `new DiscardFromPlayAction(propertyFactory, true)` -
        // the exact same action class as discardFromPlay, only the display name differs.
        ["sacrifice"] = new DiscardFromPlayGameActionHandler(),
        ["honor"] = new HonorGameActionHandler(),
        ["dishonor"] = new DishonorGameActionHandler(),
        ["bow"] = new BowGameActionHandler(),
        ["ready"] = new ReadyGameActionHandler(),
        ["moveToConflict"] = new MoveToConflictGameActionHandler(),
        ["sendHome"] = new SendHomeGameActionHandler(),
        ["removeFate"] = new RemoveFateGameActionHandler(),
        ["switchConflictType"] = new SwitchConflictTypeGameActionHandler(),
        ["putIntoConflict"] = new PutIntoConflictGameActionHandler(),
        ["modifyBid"] = new ModifyBidGameActionHandler(),
        ["selectRing"] = new SelectRingGameActionHandler(),
        ["cancel"] = new CancelGameActionHandler(),
        ["chosenDiscard"] = new ChosenDiscardGameActionHandler()
    };

    public IGameActionHandler Resolve(string name) =>
        _handlers.TryGetValue(name, out var handler)
            ? handler
            : throw new NotSupportedException($"No executable handler registered for gameAction '{name}' yet.");
}
