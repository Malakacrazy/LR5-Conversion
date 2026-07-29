using L5R.Engine.Abilities;
using L5R.Engine.Dsl.GameActions;
using L5R.Engine.State;

namespace L5R.Engine.Cards.Scripts;

/// <summary>
/// blackmail-artist: after this character wins a political conflict it's participating in,
/// take 1 honor from the opponent ("event.conflict.winner === context.player && event.
/// conflict.conflictType === 'political'"). The effect reuses TakeHonorGameActionHandler
/// directly (already targets context.Game.Opponent(context.Player)).
/// </summary>
public sealed class BlackmailArtistTakeHonorOnPoliticalWin : ICardScript
{
    public void Execute(AbilityContext context)
    {
        var blackmailArtist = context.Source;

        if (!IsParticipating(context.Game, blackmailArtist))
            throw new InvalidOperationException($"'{blackmailArtist.Id}' can only trigger while participating.");

        var conflict = context.Game.CurrentConflict
            ?? throw new InvalidOperationException($"'{blackmailArtist.Id}' requires an active conflict.");

        if (conflict.Winner != context.Player)
            throw new InvalidOperationException($"'{blackmailArtist.Id}' can only trigger when its controller wins the conflict.");

        if (conflict.ConflictType != "political")
            throw new InvalidOperationException($"'{blackmailArtist.Id}' can only trigger after a political conflict.");

        new TakeHonorGameActionHandler().Execute(context, null);
    }

    private static bool IsParticipating(GameState game, Card card) =>
        game.CurrentConflict is { } conflict && (conflict.Attackers.Contains(card) || conflict.Defenders.Contains(card));
}
