using L5R.Engine.Abilities;
using L5R.Engine.Cards.Scripts;
using L5R.Engine.State;

namespace L5R.Engine.Tests.Cards.PerCard;

public class DefendTheWallTests
{
    [Test]
    public void WhenItsControllerWinsAConflictDeclaredAgainstIt_ResolvesTheRing()
    {
        var p1 = new Player { Name = "Player1", Honor = 3 };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var defendTheWall = new Card { Id = "defend-the-wall", Type = CardType.Province, Controller = p1 };

        var conflict = new Conflict { AttackingPlayer = p2, DefendingPlayer = p1, DeclaredProvince = defendTheWall, Winner = p1, Elements = new List<string> { "air" } };
        game.CurrentConflict = conflict;

        var context = new AbilityContext { Game = game, Player = p1, Source = defendTheWall, ChosenChoice = "Gain 2 Honor" };

        new DefendTheWallResolveRingAsAttacker().Execute(context);

        Assert.That(p1.Honor, Is.EqualTo(5));
    }

    [Test]
    public void WhenItsControllerLoses_Throws()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var defendTheWall = new Card { Id = "defend-the-wall", Type = CardType.Province, Controller = p1 };

        var conflict = new Conflict { AttackingPlayer = p2, DefendingPlayer = p1, DeclaredProvince = defendTheWall, Winner = p2 };
        game.CurrentConflict = conflict;

        var context = new AbilityContext { Game = game, Player = p1, Source = defendTheWall };

        Assert.Throws<InvalidOperationException>(() => new DefendTheWallResolveRingAsAttacker().Execute(context));
    }

    [Test]
    public void WhenDeclaredAgainstADifferentProvince_Throws()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var defendTheWall = new Card { Id = "defend-the-wall", Type = CardType.Province, Controller = p1 };
        var otherProvince = new Card { Id = "other-province", Type = CardType.Province, Controller = p1 };

        var conflict = new Conflict { AttackingPlayer = p2, DefendingPlayer = p1, DeclaredProvince = otherProvince, Winner = p1 };
        game.CurrentConflict = conflict;

        var context = new AbilityContext { Game = game, Player = p1, Source = defendTheWall };

        Assert.Throws<InvalidOperationException>(() => new DefendTheWallResolveRingAsAttacker().Execute(context));
    }
}
