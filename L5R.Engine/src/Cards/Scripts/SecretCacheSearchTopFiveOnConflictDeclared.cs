using System.Text.Json;
using L5R.Engine.Abilities;
using L5R.Engine.Dsl.GameActions;

namespace L5R.Engine.Cards.Scripts;

/// <summary>
/// secret-cache: after a conflict is declared against this province, look at the top 5
/// cards of your deck and take one to hand. Needs Conflict.DeclaredProvince field
/// inspection (see its own doc comment) - not modeled by the closed predicate vocabulary.
/// Reuses DeckSearchGameActionHandler with an explicit amount:5 (no ported card needed a
/// non-default amount before this one, so there's no existing precedent for constructing
/// one inline from a script - this is the smallest way to do it without forcing the whole
/// deck into scope, which would let the player search deeper than the card allows).
/// Reshuffling afterward is a no-op, same as DeckSearchGameActionHandler's own doc comment
/// (no RNG/shuffle primitive, no observable behavior depends on deck order beyond "top N").
/// </summary>
public sealed class SecretCacheSearchTopFiveOnConflictDeclared : ICardScript
{
    private static readonly JsonElement AmountFive = JsonDocument.Parse("{\"amount\":5}").RootElement;

    public void Execute(AbilityContext context)
    {
        var secretCache = context.Source;

        var conflict = context.Game.CurrentConflict
            ?? throw new InvalidOperationException($"'{secretCache.Id}' requires an active conflict.");

        if (conflict.DeclaredProvince != secretCache)
            throw new InvalidOperationException($"'{secretCache.Id}' can only trigger when the conflict is declared against it.");

        new DeckSearchGameActionHandler().Execute(context, AmountFive);
    }
}
