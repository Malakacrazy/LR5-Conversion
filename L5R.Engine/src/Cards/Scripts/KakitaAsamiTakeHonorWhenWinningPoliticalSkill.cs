using L5R.Engine.Abilities;
using L5R.Engine.Dsl.GameActions;

namespace L5R.Engine.Cards.Scripts;

/// <summary>
/// kakita-asami: during a political conflict, take 1 honor from the opponent if your side
/// currently has more political skill than the other's - the comparison direction flips
/// depending on whether you're the attacking or defending player, beyond compareValues'
/// fixed-direction shape. Conflict.AttackerSkill/DefenderSkill are caller-set facts, same
/// convention as Winner/Loser/SkillDifference - no skill-totaling pipeline sums
/// participants' effective skill automatically. Effect reuses TakeHonorGameActionHandler
/// directly.
/// </summary>
public sealed class KakitaAsamiTakeHonorWhenWinningPoliticalSkill : ICardScript
{
    public void Execute(AbilityContext context)
    {
        var asami = context.Source;
        var conflict = context.Game.CurrentConflict
            ?? throw new InvalidOperationException($"'{asami.Id}' requires an active conflict.");

        if (conflict.ConflictType != "political")
            throw new InvalidOperationException($"'{asami.Id}' can only be used during a political conflict.");

        var difference = conflict.AttackerSkill - conflict.DefenderSkill;
        var isWinningForController = context.Player == conflict.AttackingPlayer ? difference > 0 : difference < 0;

        if (!isWinningForController)
            throw new InvalidOperationException($"'{asami.Id}' can only be used while its controller's side has more political skill.");

        new TakeHonorGameActionHandler().Execute(context, null);
    }
}
