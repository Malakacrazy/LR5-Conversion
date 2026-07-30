using L5R.Engine.GameSteps.BotActions;
using L5R.Engine.State;

namespace L5R.Engine.Tests.GameSteps.BotActions;

public class DeathseekerBotActionTests
{
    private static GameState NewScenario(out Player p1, out Card deathseeker, out Card opponentCharacter)
    {
        p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };

        deathseeker = new Card { Id = "deathseeker", Type = CardType.Character, Controller = p1 };
        opponentCharacter = new Card { Id = "opponent-character", Type = CardType.Character, Controller = p2, Fate = 2 };
        p1.PlayArea.Add(deathseeker);
        p2.PlayArea.Add(opponentCharacter);

        var conflict = new Conflict { AttackingPlayer = p1, DefendingPlayer = p2, Loser = p1 };
        conflict.Attackers.Add(deathseeker);
        game.CurrentConflict = conflict;

        return game;
    }

    [Test]
    public void IsLegal_WhenItsControllerLostWhileAttacking_True()
    {
        var game = NewScenario(out var p1, out var deathseeker, out _);

        Assert.That(new DeathseekerBotAction().IsLegal(game, deathseeker, p1), Is.True);
    }

    [Test]
    public void IsLegal_WhenItsControllerDidNotLose_False()
    {
        var game = NewScenario(out var p1, out var deathseeker, out _);
        game.CurrentConflict!.Loser = game.Player2;

        Assert.That(new DeathseekerBotAction().IsLegal(game, deathseeker, p1), Is.False);
    }

    [Test]
    public void Invoke_SacrificesItselfAndRemovesFateFromTheTarget()
    {
        var game = NewScenario(out var p1, out var deathseeker, out var opponentCharacter);

        new DeathseekerBotAction().Invoke(game, deathseeker, p1);

        Assert.That(p1.Discard, Contains.Item(deathseeker));
        Assert.That(opponentCharacter.Fate, Is.EqualTo(1));
    }
}
