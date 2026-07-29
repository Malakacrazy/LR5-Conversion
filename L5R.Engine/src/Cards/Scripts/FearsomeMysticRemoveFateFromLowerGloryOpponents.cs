using L5R.Engine.Abilities;
using L5R.Engine.Dsl.GameActions;
using L5R.Engine.State;

namespace L5R.Engine.Cards.Scripts;

/// <summary>
/// fearsome-mystic's action only (the +2 glory persistent effect is expressed generically
/// in the card's JSON): while participating, remove 1 fate from each participating
/// opponent character with lower (effective) glory than this character. allCardsMatching's
/// "of" predicate has no way to reference "the candidate's own stat" against another
/// card's stat (only source/player/target/targets contextPaths and literals) - matches
/// this card's own scriptOverride reason. No throw for zero matching opponents, same
/// "bulk target with nothing to affect is a legal no-op" precedent as for-greater-glory.
/// </summary>
public sealed class FearsomeMysticRemoveFateFromLowerGloryOpponents : ICardScript
{
    public void Execute(AbilityContext context)
    {
        var mystic = context.Source;

        if (!IsParticipating(context.Game, mystic))
            throw new InvalidOperationException($"'{mystic.Id}' can only be used while participating.");

        var conflict = context.Game.CurrentConflict!;
        var opponent = context.Game.Opponent(context.Player);
        var myGlory = context.Game.EffectiveGlory(mystic);

        var targets = conflict.Attackers.Concat(conflict.Defenders)
            .Where(c => c.Controller == opponent && context.Game.EffectiveGlory(c) < myGlory)
            .ToList();

        foreach (var target in targets)
        {
            context.Target = target;
            new RemoveFateGameActionHandler().Execute(context, null);
        }
    }

    private static bool IsParticipating(GameState game, Card card) =>
        game.CurrentConflict is { } conflict && (conflict.Attackers.Contains(card) || conflict.Defenders.Contains(card));
}
