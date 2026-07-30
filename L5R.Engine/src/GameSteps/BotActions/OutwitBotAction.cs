using System.Linq;
using L5R.Engine.Abilities;
using L5R.Engine.Cards.Scripts;
using L5R.Engine.State;

namespace L5R.Engine.GameSteps.BotActions;

/// <summary>
/// Phase B adapter for the event card outwit - invoked via IBotPolicy.ResolveEventScript
/// (EventResolver's scriptOverride'd-event fallback), never via ChooseScriptedAction: it only
/// makes sense as part of playing the card, and both isLegal/target selection require an
/// already-active conflict (the script's own IsParticipating check), so this can only ever
/// fire from the mid-conflict action window, not the pre-conflict one.
/// </summary>
public sealed class OutwitBotAction : IBotScriptAction
{
    public bool IsLegal(GameState game, Card source, Player actingPlayer) =>
        FindTarget(game, actingPlayer) is not null;

    public void Invoke(GameState game, Card source, Player actingPlayer)
    {
        var target = FindTarget(game, actingPlayer)
            ?? throw new InvalidOperationException($"'{source.Id}' has no legal bot target.");

        var context = new AbilityContext { Game = game, Player = actingPlayer, Source = source, Target = target };
        new OutwitSendHomeOutclassedByCourtier().Execute(context);
    }

    private static Card? FindTarget(GameState game, Player actingPlayer) =>
        game.Opponent(actingPlayer).PlayArea.FirstOrDefault(candidate =>
            actingPlayer.PlayArea.Any(myCard =>
                myCard.Traits.Contains("courtier") && IsParticipating(game, myCard)
                && game.EffectivePoliticalSkill(myCard) > game.EffectivePoliticalSkill(candidate)));

    private static bool IsParticipating(GameState game, Card card) =>
        game.CurrentConflict is { } conflict && (conflict.Attackers.Contains(card) || conflict.Defenders.Contains(card));
}
