using L5R.Engine.Abilities;
using L5R.Engine.Cards.Scripts;
using L5R.Engine.State;

namespace L5R.Engine.Tests.Cards.PerCard;

public class KeeperOfEarthTests
{
    [Test]
    public void WhenDefendingAndWinningAnEarthConflict_GainsOneFate()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var role = new Card { Id = "keeper-of-earth", Type = CardType.Role, Controller = p1 };
        var conflict = new Conflict { AttackingPlayer = p2, DefendingPlayer = p1, Winner = p1, Elements = new List<string> { "earth" } };
        game.CurrentConflict = conflict;

        var context = new AbilityContext { Game = game, Player = p1, Source = role };

        new KeeperOfEarthGainFateOnEarthDefenseWin().Execute(context);

        Assert.That(p1.Fate, Is.EqualTo(1));
    }

    [Test]
    public void WhenTheConflictHasNoEarthElement_Throws()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var role = new Card { Id = "keeper-of-earth", Type = CardType.Role, Controller = p1 };
        var conflict = new Conflict { AttackingPlayer = p2, DefendingPlayer = p1, Winner = p1, Elements = new List<string> { "fire" } };
        game.CurrentConflict = conflict;

        var context = new AbilityContext { Game = game, Player = p1, Source = role };

        Assert.Throws<InvalidOperationException>(() => new KeeperOfEarthGainFateOnEarthDefenseWin().Execute(context));
    }

    [Test]
    public void WhenAttackingInsteadOfDefending_Throws()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var role = new Card { Id = "keeper-of-earth", Type = CardType.Role, Controller = p1 };
        var conflict = new Conflict { AttackingPlayer = p1, DefendingPlayer = p2, Winner = p1, Elements = new List<string> { "earth" } };
        game.CurrentConflict = conflict;

        var context = new AbilityContext { Game = game, Player = p1, Source = role };

        Assert.Throws<InvalidOperationException>(() => new KeeperOfEarthGainFateOnEarthDefenseWin().Execute(context));
    }

    [Test]
    public void WhenLosing_Throws()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var role = new Card { Id = "keeper-of-earth", Type = CardType.Role, Controller = p1 };
        var conflict = new Conflict { AttackingPlayer = p2, DefendingPlayer = p1, Winner = p2, Elements = new List<string> { "earth" } };
        game.CurrentConflict = conflict;

        var context = new AbilityContext { Game = game, Player = p1, Source = role };

        Assert.Throws<InvalidOperationException>(() => new KeeperOfEarthGainFateOnEarthDefenseWin().Execute(context));
    }
}
