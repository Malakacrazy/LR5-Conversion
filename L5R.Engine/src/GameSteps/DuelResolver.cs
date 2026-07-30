using L5R.Engine.State;

namespace L5R.Engine.GameSteps;

/// <summary>
/// Minimal duel resolution (ringteki Duel.js: Duel.getChallengerStatisticTotal/
/// getTargetStatisticTotal - military skill plus the duelist's controller's honor bid for
/// the round, Player.ShowBid, already tracked for the honor-bid step of the Draw phase).
/// Ties favor the challenger, same convention as conflict resolution favoring the attacker.
/// Only military duels are modeled - none of the three ported duel-triggering cards
/// (duelist-training, kakita-kaezin, mirumoto-raitsugu) are political.
/// </summary>
public static class DuelResolver
{
    public static Card Resolve(GameState game, Card challenger, Card target)
    {
        var challengerTotal = game.EffectiveMilitarySkill(challenger) + challenger.Controller.ShowBid;
        var targetTotal = game.EffectiveMilitarySkill(target) + target.Controller.ShowBid;

        return challengerTotal >= targetTotal ? challenger : target;
    }
}
