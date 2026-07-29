using L5R.Engine.Abilities;
using L5R.Engine.Dsl;
using L5R.Engine.Dsl.GameActions;

namespace L5R.Engine.Cards.Scripts;

/// <summary>
/// keeper-initiate: after claiming a ring matching your role's element, put this
/// character into play for free from your provinces or discard pile, then place 1 fate on
/// it. Same context.TargetRing/Player.Role convention as
/// SeekerInitiateSearchTopFiveOnMatchingRingClaim. "Put into play" bypasses the printed
/// cost entirely (unlike PlayCardGameActionHandler), so it's a direct ZoneMover call
/// rather than routed through playCard - ringteki's own putIntoPlay action never charges
/// fate either. The "then" follow-up (placeFate) is inlined directly rather than modeled
/// as a separate chained-ability concept, since there's nothing else that would consume
/// the intermediate state between the two steps.
/// </summary>
public sealed class KeeperInitiatePutIntoPlayOnMatchingRingClaim : ICardScript
{
    public void Execute(AbilityContext context)
    {
        var keeperInitiate = context.Source;

        var ring = context.TargetRing
            ?? throw new InvalidOperationException($"'{keeperInitiate.Id}' requires context.TargetRing (the ring just claimed) to be set.");

        if (ring.ClaimedBy != context.Player)
            throw new InvalidOperationException($"'{keeperInitiate.Id}' can only trigger when its controller claims the ring.");

        var role = context.Player.Role
            ?? throw new InvalidOperationException($"'{keeperInitiate.Id}' requires its controller to have a role.");

        if (!role.Traits.Contains(ring.Element))
            throw new InvalidOperationException($"'{keeperInitiate.Id}' can only trigger when the claimed ring's element matches its controller's role.");

        if (!context.Player.Provinces.Contains(keeperInitiate) && !context.Player.Discard.Contains(keeperInitiate))
            throw new InvalidOperationException($"'{keeperInitiate.Id}' must be in its controller's provinces or discard pile.");

        // ZoneMover only clears Hand/PlayArea/Discard/Deck (see its own doc comment) - this
        // is the first ported card that moves a card out of Provinces, so that zone needs
        // an explicit removal here.
        context.Player.Provinces.Remove(keeperInitiate);
        ZoneMover.MoveTo(keeperInitiate, context.Player.PlayArea, "play area");

        context.Target = keeperInitiate;
        new PlaceFateGameActionHandler().Execute(context, null);
    }
}
