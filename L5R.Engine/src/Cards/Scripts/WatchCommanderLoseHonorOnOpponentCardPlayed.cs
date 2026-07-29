using L5R.Engine.Abilities;
using L5R.Engine.Dsl.GameActions;
using L5R.Engine.State;

namespace L5R.Engine.Cards.Scripts;

/// <summary>
/// watch-commander: a reaction to the opponent playing any card - "event.player === context.
/// player.opponent" is inherently always about context.Player's own opponent (there's no
/// other player it could mean), so the caller only needs to invoke this when that fact
/// already holds (no eventCard needed either, since neither the condition nor the effect
/// references the played card itself) - same no-event-bus, caller-asserts-it-happened
/// convention as every JSON-driven triggeredAbility in this engine. Once triggered, the
/// effect ("make opponent lose 1 honor") is exactly LoseHonorGameActionHandler's own default
/// target (context.Game.Opponent(context.Player)), reused directly rather than reimplemented.
/// "limit: unlimitedPerConflict" needs no work - no reaction limit is enforced anywhere in
/// this engine yet (matches every "max"/"limit" JSON field's own established no-op
/// precedent). The attachmentConditions restrictions (copy limit 1, my control only) are
/// generic DSL territory, covered by the card's own persistentEffects block and
/// GameState.ExceedsAttachmentLimit/IsAttachRestricted's own tests elsewhere.
/// </summary>
public sealed class WatchCommanderLoseHonorOnOpponentCardPlayed : ICardScript
{
    public void Execute(AbilityContext context)
    {
        var watchCommander = context.Source;
        var parent = watchCommander.AttachedTo
            ?? throw new InvalidOperationException($"'{watchCommander.Id}' is not currently attached to anything.");

        if (!IsParticipating(context.Game, parent))
            throw new InvalidOperationException($"'{watchCommander.Id}' can only trigger while the attached character is participating.");

        new LoseHonorGameActionHandler().Execute(context, null);
    }

    private static bool IsParticipating(GameState game, Card card) =>
        game.CurrentConflict is { } conflict && (conflict.Attackers.Contains(card) || conflict.Defenders.Contains(card));
}
