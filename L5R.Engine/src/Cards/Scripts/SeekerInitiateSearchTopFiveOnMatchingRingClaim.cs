using System.Text.Json;
using L5R.Engine.Abilities;
using L5R.Engine.Dsl.GameActions;

namespace L5R.Engine.Cards.Scripts;

/// <summary>
/// seeker-initiate: after claiming a ring matching your role's element, look at the top 5
/// cards of your deck and take one to hand. context.TargetRing carries the ring that was
/// just claimed (ringteki's own event.ring/event.conflict, whose elements are always
/// derived from the claimed ring itself - see Conflict's own doc comment on why this
/// engine doesn't separately model that derivation), and Player.Role.Traits is checked
/// directly rather than routing through a role-trait-cross-reference concept the closed
/// predicate vocabulary doesn't have.
/// </summary>
public sealed class SeekerInitiateSearchTopFiveOnMatchingRingClaim : ICardScript
{
    private static readonly JsonElement AmountFive = JsonDocument.Parse("{\"amount\":5}").RootElement;

    public void Execute(AbilityContext context)
    {
        var seekerInitiate = context.Source;

        var ring = context.TargetRing
            ?? throw new InvalidOperationException($"'{seekerInitiate.Id}' requires context.TargetRing (the ring just claimed) to be set.");

        if (ring.ClaimedBy != context.Player)
            throw new InvalidOperationException($"'{seekerInitiate.Id}' can only trigger when its controller claims the ring.");

        var role = context.Player.Role
            ?? throw new InvalidOperationException($"'{seekerInitiate.Id}' requires its controller to have a role.");

        if (!role.Traits.Contains(ring.Element))
            throw new InvalidOperationException($"'{seekerInitiate.Id}' can only trigger when the claimed ring's element matches its controller's role.");

        new DeckSearchGameActionHandler().Execute(context, AmountFive);
    }
}
