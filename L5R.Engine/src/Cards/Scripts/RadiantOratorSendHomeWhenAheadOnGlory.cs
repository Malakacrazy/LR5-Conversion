using L5R.Engine.Abilities;
using L5R.Engine.Dsl.GameActions;
using L5R.Engine.State;

namespace L5R.Engine.Cards.Scripts;

/// <summary>
/// radiant-orator: while participating, if your ready participating characters' total
/// (effective) glory exceeds your opponent's, send an opponent's character home. Needs a
/// sum-of-stat aggregate over a set of cards, beyond countCardsMatching's plain count -
/// not generalizing a sum-aggregate concept from a single card (matches this card's own
/// scriptOverride reason).
/// </summary>
public sealed class RadiantOratorSendHomeWhenAheadOnGlory : ICardScript
{
    public void Execute(AbilityContext context)
    {
        var orator = context.Source;

        if (!IsParticipating(context.Game, orator))
            throw new InvalidOperationException($"'{orator.Id}' can only be used while participating.");

        var opponent = context.Game.Opponent(context.Player);

        var myGlory = ReadyParticipatingGlory(context.Game, context.Player);
        var opponentGlory = ReadyParticipatingGlory(context.Game, opponent);

        if (myGlory <= opponentGlory)
            throw new InvalidOperationException($"'{orator.Id}' requires its controller's ready participating characters to have more total glory than the opponent's.");

        var target = context.Target
            ?? throw new InvalidOperationException($"'{orator.Id}' requires context.Target to be set.");

        if (target.Controller != opponent)
            throw new InvalidOperationException($"'{target.Id}' must be controlled by the opponent.");

        new SendHomeGameActionHandler().Execute(context, null);
    }

    private static int ReadyParticipatingGlory(GameState game, Player player) =>
        game.CurrentConflict is { } conflict
            ? conflict.Attackers.Concat(conflict.Defenders)
                .Where(c => c.Controller == player && c.Type == CardType.Character && !c.Bowed)
                .Sum(game.EffectiveGlory)
            : 0;

    private static bool IsParticipating(GameState game, Card card) =>
        game.CurrentConflict is { } conflict && (conflict.Attackers.Contains(card) || conflict.Defenders.Contains(card));
}
