using System.Text.Json;
using L5R.Engine.Abilities;
using L5R.Engine.Dsl;

namespace L5R.Engine.Dsl.GameActions;

/// <summary>
/// ringteki GameActions.ts placeFate: add fate from nowhere onto a card. Also fires
/// ikoma-prodigy's own reaction if the target is one - see IkomaProdigyFirer's own doc
/// comment.
/// </summary>
public sealed class PlaceFateGameActionHandler : IGameActionHandler
{
    public void Execute(AbilityContext context, JsonElement? parameters)
    {
        if (context.Target is null)
            throw new InvalidOperationException("placeFate requires context.Target to be set.");

        var amount = parameters?.TryGetProperty("amount", out var amountElement) == true
            ? amountElement.GetInt32()
            : 1;

        context.Target.Fate += amount;

        IkomaProdigyFirer.FireIfLegal(context.Game, context.Target);
    }
}
