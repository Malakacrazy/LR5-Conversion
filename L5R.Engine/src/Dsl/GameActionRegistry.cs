using L5R.Engine.Dsl.GameActions;

namespace L5R.Engine.Dsl;

/// <summary>
/// Maps a GameActions.ts name to its executable handler. Deliberately small - grows one
/// entry at a time as a real ported card needs the next game action.
/// </summary>
public sealed class GameActionRegistry
{
    private readonly Dictionary<string, IGameActionHandler> _handlers;

    public GameActionRegistry()
    {
        _handlers = new Dictionary<string, IGameActionHandler>
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
            ["chosenDiscard"] = new ChosenDiscardGameActionHandler(),
            ["gainHonor"] = new GainHonorGameActionHandler(),
            ["loseHonor"] = new LoseHonorGameActionHandler(),
            ["takeFate"] = new TakeFateGameActionHandler(),
            ["takeHonor"] = new TakeHonorGameActionHandler(),
            ["returnRing"] = new ReturnRingGameActionHandler(),
            // ringteki's own TakeRingAction class is oddly named 'takeFate' internally (a
            // copy-paste artifact), but card-schema.json's name for it - and the JSON key every
            // ported card actually uses (know-the-world) - is "takeRing".
            ["takeRing"] = new TakeRingGameActionHandler(),
            ["playerLastingEffect"] = new PlayerLastingEffectGameActionHandler(),
            ["playCard"] = new PlayCardGameActionHandler(),
            ["flipDynasty"] = new FlipDynastyGameActionHandler(),
            ["lookAt"] = new LookAtGameActionHandler(),
            ["deckSearch"] = new DeckSearchGameActionHandler(),
            ["discardStatusToken"] = new DiscardStatusTokenGameActionHandler(),
            ["discardCard"] = new DiscardCardGameActionHandler(),
            ["returnToDeck"] = new ReturnToDeckGameActionHandler(),
            ["attach"] = new AttachGameActionHandler()
        };

        // cardMenu is the first handler that needs to invoke *another* gameAction by name
        // (kitsuki-investigator's nested "discardCard"), so it needs a reference to this
        // already-populated registry - added after the collection initializer rather than
        // inside it, since `this` isn't fully constructed yet during that initializer.
        _handlers["cardMenu"] = new CardMenuGameActionHandler(this);
    }

    public IGameActionHandler Resolve(string name) =>
        _handlers.TryGetValue(name, out var handler)
            ? handler
            : throw new NotSupportedException($"No executable handler registered for gameAction '{name}' yet.");
}
