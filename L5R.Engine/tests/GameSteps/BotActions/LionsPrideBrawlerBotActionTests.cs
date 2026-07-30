using L5R.Engine.GameSteps.BotActions;
using L5R.Engine.State;

namespace L5R.Engine.Tests.GameSteps.BotActions;

public class LionsPrideBrawlerBotActionTests
{
    [Test]
    public void IsLegal_WhileAttackingWithAQualifyingOpponentTarget_True()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        var brawler = new Card { Id = "lion-s-pride-brawler", Type = CardType.Character, Controller = p1, PrintedMilitarySkill = 3 };
        var weakOpponent = new Card { Id = "weak", Type = CardType.Character, Controller = p2, PrintedMilitarySkill = 2 };
        p1.PlayArea.Add(brawler);
        p2.PlayArea.Add(weakOpponent);

        var conflict = new Conflict { AttackingPlayer = p1, DefendingPlayer = p2 };
        conflict.Attackers.Add(brawler);
        game.CurrentConflict = conflict;

        Assert.That(new LionsPrideBrawlerBotAction().IsLegal(game, brawler, p1), Is.True);
    }

    [Test]
    public void IsLegal_WhenNotAttacking_False()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        var brawler = new Card { Id = "lion-s-pride-brawler", Type = CardType.Character, Controller = p1, PrintedMilitarySkill = 3 };
        p1.PlayArea.Add(brawler);

        Assert.That(new LionsPrideBrawlerBotAction().IsLegal(game, brawler, p1), Is.False);
    }

    [Test]
    public void IsLegal_WhenNoOpponentQualifies_False()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        var brawler = new Card { Id = "lion-s-pride-brawler", Type = CardType.Character, Controller = p1, PrintedMilitarySkill = 1 };
        var strongOpponent = new Card { Id = "strong", Type = CardType.Character, Controller = p2, PrintedMilitarySkill = 5 };
        p1.PlayArea.Add(brawler);
        p2.PlayArea.Add(strongOpponent);

        var conflict = new Conflict { AttackingPlayer = p1, DefendingPlayer = p2 };
        conflict.Attackers.Add(brawler);
        game.CurrentConflict = conflict;

        Assert.That(new LionsPrideBrawlerBotAction().IsLegal(game, brawler, p1), Is.False);
    }

    [Test]
    public void Invoke_BowsTheQualifyingOpponent()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        var brawler = new Card { Id = "lion-s-pride-brawler", Type = CardType.Character, Controller = p1, PrintedMilitarySkill = 3 };
        var weakOpponent = new Card { Id = "weak", Type = CardType.Character, Controller = p2, PrintedMilitarySkill = 2 };
        p1.PlayArea.Add(brawler);
        p2.PlayArea.Add(weakOpponent);

        var conflict = new Conflict { AttackingPlayer = p1, DefendingPlayer = p2 };
        conflict.Attackers.Add(brawler);
        game.CurrentConflict = conflict;

        new LionsPrideBrawlerBotAction().Invoke(game, brawler, p1);

        Assert.That(weakOpponent.Bowed, Is.True);
    }
}
