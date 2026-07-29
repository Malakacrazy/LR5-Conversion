using L5R.Engine.Abilities;
using L5R.Engine.Cards.Scripts;
using L5R.Engine.State;

namespace L5R.Engine.Tests.Cards.PerCard;

public class BayushiShojuTests
{
    private static (GameState Game, Card Shoju, Card Target) NewGameParticipating(int targetPoliticalSkill)
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var shoju = new Card { Id = "bayushi-shoju", Type = CardType.Character, Controller = p1 };
        var target = new Card { Id = "opponent-character", Type = CardType.Character, Controller = p2, PrintedPoliticalSkill = targetPoliticalSkill };
        p1.PlayArea.Add(shoju);
        p2.PlayArea.Add(target);

        var conflict = new Conflict { AttackingPlayer = p1, DefendingPlayer = p2, ConflictType = "political" };
        conflict.Attackers.Add(shoju);
        conflict.Defenders.Add(target);
        game.CurrentConflict = conflict;

        return (game, shoju, target);
    }

    [Test]
    public void ReducesTargetsPoliticalSkillByOne()
    {
        var (game, shoju, target) = NewGameParticipating(targetPoliticalSkill: 3);
        var context = new AbilityContext { Game = game, Player = game.Player1, Source = shoju, Target = target };

        new BayushiShojuReducePoliticalSkillWithDeathCheck().Execute(context);

        Assert.That(game.EffectivePoliticalSkill(target), Is.EqualTo(2));
        Assert.That(game.Player2.PlayArea, Does.Contain(target));
    }

    [Test]
    public void WhenPoliticalSkillDropsBelowOne_DiscardsTheTarget()
    {
        var (game, shoju, target) = NewGameParticipating(targetPoliticalSkill: 1);
        var context = new AbilityContext { Game = game, Player = game.Player1, Source = shoju, Target = target };

        new BayushiShojuReducePoliticalSkillWithDeathCheck().Execute(context);

        Assert.That(game.Player2.Discard, Does.Contain(target));
    }

    [Test]
    public void DuringAMilitaryConflict_Throws()
    {
        var (game, shoju, target) = NewGameParticipating(targetPoliticalSkill: 3);
        game.CurrentConflict!.ConflictType = "military";
        var context = new AbilityContext { Game = game, Player = game.Player1, Source = shoju, Target = target };

        Assert.Throws<InvalidOperationException>(() => new BayushiShojuReducePoliticalSkillWithDeathCheck().Execute(context));
    }
}
