using L5R.Engine.Abilities;
using L5R.Engine.Dsl;
using L5R.Engine.Dsl.GameActions;

namespace L5R.Engine.Cards.Scripts;

/// <summary>
/// deathseeker: after its controller loses a conflict it attacked in, sacrifice this
/// character to either remove 1 fate from an opponent's character (if it has any) or
/// discard it otherwise ("event.conflict.loser === context.player && context.source.
/// isAttacking()"). Ringteki itself has no generic action for this fate-or-discard branch
/// (its own source has a "TODO: RemoveFateOrDiscard action?" comment) - not a gap worth
/// generalizing here either, so the branch is just plain C# choosing between the two
/// existing handlers.
/// </summary>
public sealed class DeathseekerRemoveFateOrDiscardOnLoss : ICardScript
{
    public void Execute(AbilityContext context)
    {
        var deathseeker = context.Source;
        var conflict = context.Game.CurrentConflict
            ?? throw new InvalidOperationException($"'{deathseeker.Id}' requires an active conflict.");

        if (conflict.Loser != context.Player)
            throw new InvalidOperationException($"'{deathseeker.Id}' can only trigger when its controller loses the conflict.");

        if (!conflict.Attackers.Contains(deathseeker))
            throw new InvalidOperationException($"'{deathseeker.Id}' can only trigger while attacking.");

        var target = context.Target
            ?? throw new InvalidOperationException($"'{deathseeker.Id}' requires context.Target to be set.");

        if (target.Controller != context.Game.Opponent(context.Player))
            throw new InvalidOperationException($"'{target.Id}' must be controlled by the opponent.");

        ZoneMover.MoveTo(deathseeker, deathseeker.Controller.Discard, "discard");

        if (target.Fate > 0)
            new RemoveFateGameActionHandler().Execute(context, null);
        else
            new DiscardFromPlayGameActionHandler().Execute(context, null);
    }
}
