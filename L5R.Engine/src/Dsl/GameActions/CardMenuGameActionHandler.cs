using System.Text.Json;
using L5R.Engine.Abilities;

namespace L5R.Engine.Dsl.GameActions;

/// <summary>
/// ringteki CardMenuAction (kitsuki-investigator's "look at the opponent's hand and discard
/// a card from it"): a single-selection menu built from params.cards - picking one card runs
/// params.gameAction against that one card only, not a multi-select. No selection-menu UI
/// exists, so the caller supplies which card was chosen via context.ChosenCardMenuCard
/// directly, validated to be among the candidate pool. The first handler that needs to invoke
/// *another* gameAction by name, so it takes a GameActionRegistry dependency (see
/// GameActionRegistry.cs's two-step construction).
/// </summary>
public sealed class CardMenuGameActionHandler : IGameActionHandler
{
    private readonly GameActionRegistry _gameActions;

    public CardMenuGameActionHandler(GameActionRegistry gameActions) => _gameActions = gameActions;

    public void Execute(AbilityContext context, JsonElement? parameters)
    {
        if (parameters is null)
            throw new InvalidOperationException("cardMenu requires params (cards and gameAction).");

        var props = parameters.Value;
        var candidates = TargetResolver.ResolveAllCardsMatching(props.GetProperty("cards"), context);

        var chosen = context.ChosenCardMenuCard
            ?? throw new InvalidOperationException("cardMenu requires a chosen card.");

        if (!candidates.Contains(chosen))
            throw new InvalidOperationException($"'{chosen.Id}' is not among cardMenu's candidate cards.");

        var nestedName = props.GetProperty("gameAction").GetProperty("name").GetString()!;
        context.Target = chosen;
        _gameActions.Resolve(nestedName).Execute(context, null);
    }
}
