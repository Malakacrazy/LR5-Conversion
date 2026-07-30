using System.Linq;
using L5R.Engine.Abilities;
using L5R.Engine.Cards.Scripts;
using L5R.Engine.State;

namespace L5R.Engine.GameSteps.BotActions;

/// <summary>Phase B adapter for niten-adept. Two inputs: one of its own unbowed attachments (the cost) and a participating character with no attachments of its own (the target).</summary>
public sealed class NitenAdeptBotAction : IBotScriptAction
{
    public bool IsLegal(GameState game, Card source, Player actingPlayer) =>
        FindMove(game, source) is not null;

    public void Invoke(GameState game, Card source, Player actingPlayer)
    {
        var move = FindMove(game, source)
            ?? throw new InvalidOperationException($"'{source.Id}' has no legal bot move.");

        var context = new AbilityContext { Game = game, Player = actingPlayer, Source = source, CostTarget = move.CostTarget, Target = move.Target };
        new NitenAdeptBowAttachmentToBowUnattachedParticipant().Execute(context);
    }

    private static (Card CostTarget, Card Target)? FindMove(GameState game, Card source)
    {
        if (!IsParticipating(game, source))
            return null;

        var costTarget = game.AllCards().FirstOrDefault(c => c.AttachedTo == source && !c.Bowed);
        if (costTarget is null)
            return null;

        var conflict = game.CurrentConflict!;
        var target = conflict.Attackers.Concat(conflict.Defenders)
            .FirstOrDefault(c => game.AllCards().All(a => a.AttachedTo != c));

        return target is not null ? (costTarget, target) : null;
    }

    private static bool IsParticipating(GameState game, Card card) =>
        game.CurrentConflict is { } conflict && (conflict.Attackers.Contains(card) || conflict.Defenders.Contains(card));
}
