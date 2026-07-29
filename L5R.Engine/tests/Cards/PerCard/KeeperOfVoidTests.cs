using L5R.Engine.Abilities;
using L5R.Engine.Cards.Scripts;
using L5R.Engine.State;

namespace L5R.Engine.Tests.Cards.PerCard;

public class KeeperOfVoidTests
{
    [Test]
    public void WhenDefendingAndWinningAVoidConflict_GainsOneFate()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var role = new Card { Id = "keeper-of-void", Type = CardType.Role, Controller = p1 };
        var conflict = new Conflict { AttackingPlayer = p2, DefendingPlayer = p1, Winner = p1, Elements = new List<string> { "void" } };
        game.CurrentConflict = conflict;

        var context = new AbilityContext { Game = game, Player = p1, Source = role };

        new KeeperOfVoidGainFateOnVoidDefenseWin().Execute(context);

        Assert.That(p1.Fate, Is.EqualTo(1));
    }

    [Test]
    public void WhenTheConflictHasNoVoidElement_Throws()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var role = new Card { Id = "keeper-of-void", Type = CardType.Role, Controller = p1 };
        var conflict = new Conflict { AttackingPlayer = p2, DefendingPlayer = p1, Winner = p1, Elements = new List<string> { "air" } };
        game.CurrentConflict = conflict;

        var context = new AbilityContext { Game = game, Player = p1, Source = role };

        Assert.Throws<InvalidOperationException>(() => new KeeperOfVoidGainFateOnVoidDefenseWin().Execute(context));
    }

    [Test]
    public void WhenAttackingInsteadOfDefending_Throws()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var role = new Card { Id = "keeper-of-void", Type = CardType.Role, Controller = p1 };
        var conflict = new Conflict { AttackingPlayer = p1, DefendingPlayer = p2, Winner = p1, Elements = new List<string> { "void" } };
        game.CurrentConflict = conflict;

        var context = new AbilityContext { Game = game, Player = p1, Source = role };

        Assert.Throws<InvalidOperationException>(() => new KeeperOfVoidGainFateOnVoidDefenseWin().Execute(context));
    }

    [Test]
    public void WhenLosing_Throws()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var role = new Card { Id = "keeper-of-void", Type = CardType.Role, Controller = p1 };
        var conflict = new Conflict { AttackingPlayer = p2, DefendingPlayer = p1, Winner = p2, Elements = new List<string> { "void" } };
        game.CurrentConflict = conflict;

        var context = new AbilityContext { Game = game, Player = p1, Source = role };

        Assert.Throws<InvalidOperationException>(() => new KeeperOfVoidGainFateOnVoidDefenseWin().Execute(context));
    }
}
