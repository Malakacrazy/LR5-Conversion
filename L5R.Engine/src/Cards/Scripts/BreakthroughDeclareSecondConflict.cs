using L5R.Engine.Abilities;
using L5R.Engine.State;

namespace L5R.Engine.Cards.Scripts;

/// <summary>
/// breakthrough: after winning a conflict as attacker by breaking its province, if this
/// was the controller's only conflict declaration this phase, immediately declare a
/// second conflict. Needs event.conflict field inspection and a conflict-collection query,
/// both beyond the closed predicate vocabulary. context.Game.ConflictRecord.LastOrDefault()
/// is the just-finished conflict (the caller appends it there before invoking this script,
/// the natural sequencing for an "afterConflict"-shaped reaction - see ConflictRecord's own
/// doc comment). "initiateConflict" needs no legality pipeline to declare - it's just a
/// fresh Conflict set as CurrentConflict, same trust-the-caller convention as every other
/// conflict fact this session.
///
/// CanPlay is unconditionally false - same reasoning as ready-for-battle's own doc comment:
/// without it, the generic hand-play window (offered during the very conflict this reacts
/// to, via ActionWindowRunner's mid-conflict/post-resolution steps) would happily discard
/// this with no effect before the conflict it reacts to has even finished resolving.
/// BreakthroughOfferer (GameLoop.ConflictPhaseStep's own hook, the card's real trigger)
/// bypasses PlayCardGameActionHandler entirely, so this restriction only blocks the generic
/// path.
/// </summary>
public sealed class BreakthroughDeclareSecondConflict : ICardScript
{
    public bool CanPlay(AbilityContext context) => false;

    public void Execute(AbilityContext context)
    {
        var breakthrough = context.Source;

        var finishedConflict = context.Game.ConflictRecord.LastOrDefault()
            ?? throw new InvalidOperationException($"'{breakthrough.Id}' requires a finished conflict in context.Game.ConflictRecord.");

        if (finishedConflict.AttackingPlayer != context.Player)
            throw new InvalidOperationException($"'{breakthrough.Id}' can only trigger when its controller was the attacker.");

        if (finishedConflict.Winner != context.Player)
            throw new InvalidOperationException($"'{breakthrough.Id}' can only trigger when its controller won the conflict.");

        var province = finishedConflict.DeclaredProvince
            ?? throw new InvalidOperationException($"'{breakthrough.Id}' requires the finished conflict's DeclaredProvince to be set.");

        if (!province.Broken)
            throw new InvalidOperationException($"'{province.Id}' must be broken.");

        var declarationsThisPhase = context.Game.ConflictDeclarationsThisPhase.Count(d => d.Player == context.Player && !d.Passed);
        if (declarationsThisPhase != 1)
            throw new InvalidOperationException($"'{breakthrough.Id}' can only trigger when this was its controller's only conflict declaration this phase.");

        var opponent = context.Game.Opponent(context.Player);
        context.Game.CurrentConflict = new Conflict { AttackingPlayer = context.Player, DefendingPlayer = opponent };
        context.Game.ConflictDeclarationsThisPhase.Add((context.Player, false));
    }
}
