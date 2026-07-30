using System.Linq;
using L5R.Engine.Abilities;
using L5R.Engine.Cards.Scripts;
using L5R.Engine.State;

namespace L5R.Engine.GameSteps.BotActions;

/// <summary>
/// Phase B adapter for togashi-kazue's in-play "steal a fate" action (TogashiKazuePlayAsAttachmentOrCharacter.StealFate)
/// - a second plain method beyond ICardScript, not Execute (which is its play-as-attachment
/// mode, an entirely different action not wired into Phase B at all). Only targets a
/// participant with fate to actually steal, rather than the script's own "any other
/// participant" range, avoiding a pointless zero-fate activation.
/// </summary>
public sealed class TogashiKazueStealFateBotAction : IBotScriptAction
{
    public bool IsLegal(GameState game, Card source, Player actingPlayer) =>
        FindTarget(game, source) is not null;

    public void Invoke(GameState game, Card source, Player actingPlayer)
    {
        var target = FindTarget(game, source)
            ?? throw new InvalidOperationException($"'{source.Id}' has no legal bot target.");

        var context = new AbilityContext { Game = game, Player = actingPlayer, Source = source, Target = target };
        new TogashiKazuePlayAsAttachmentOrCharacter().StealFate(context);
    }

    private static Card? FindTarget(GameState game, Card source)
    {
        var parent = source.AttachedTo;
        if (parent is null || !IsParticipating(game, parent))
            return null;

        var conflict = game.CurrentConflict!;
        return conflict.Attackers.Concat(conflict.Defenders).FirstOrDefault(c => c != parent && c.Fate > 0);
    }

    private static bool IsParticipating(GameState game, Card card) =>
        game.CurrentConflict is { } conflict && (conflict.Attackers.Contains(card) || conflict.Defenders.Contains(card));
}
