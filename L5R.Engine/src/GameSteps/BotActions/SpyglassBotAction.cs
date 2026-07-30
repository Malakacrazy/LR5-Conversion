using L5R.Engine.Abilities;
using L5R.Engine.Cards.Scripts;
using L5R.Engine.State;

namespace L5R.Engine.GameSteps.BotActions;

/// <summary>
/// Phase B adapter for spyglass - the first "triggered reaction" bucket card adopted, not a
/// player-activated action. Its Execute() has no actual event-tracking: the whole legality
/// check is "is the attached character currently participating" (SpyglassDrawOnParentJoiningConflict.cs),
/// a stable state fact rather than a transient "did it just now join" flag. That's a safe fit
/// for the existing action windows precisely because it naturally resets between conflicts -
/// checked at most once per window (the usual dedup), and "participating" only becomes true
/// again once a new conflict actually starts, so this can't fire more than once per conflict
/// the parent joins even without literal event detection.
/// </summary>
public sealed class SpyglassBotAction : IBotScriptAction
{
    public bool IsLegal(GameState game, Card source, Player actingPlayer) =>
        source.AttachedTo is { } parent && IsParticipating(game, parent);

    public void Invoke(GameState game, Card source, Player actingPlayer)
    {
        var context = new AbilityContext { Game = game, Player = actingPlayer, Source = source };
        new SpyglassDrawOnParentJoiningConflict().Execute(context);
    }

    private static bool IsParticipating(GameState game, Card card) =>
        game.CurrentConflict is { } conflict && (conflict.Attackers.Contains(card) || conflict.Defenders.Contains(card));
}
