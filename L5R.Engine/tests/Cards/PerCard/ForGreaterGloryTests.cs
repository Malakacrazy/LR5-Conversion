using L5R.Engine.Abilities;
using L5R.Engine.Cards.Scripts;
using L5R.Engine.State;

namespace L5R.Engine.Tests.Cards.PerCard;

public class ForGreaterGloryTests
{
    [Test]
    public void PlacesOneFateOnEachBushiOnTheAttackersSide()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var source = new Card { Id = "for-greater-glory", Type = CardType.Event, Controller = p1 };
        var myBushi = new Card { Id = "my-bushi", Type = CardType.Character, Controller = p1, Traits = new[] { "bushi" } };
        var myCourtier = new Card { Id = "my-courtier", Type = CardType.Character, Controller = p1, Traits = new[] { "courtier" } };
        var opponentBushi = new Card { Id = "opponent-bushi", Type = CardType.Character, Controller = p2, Traits = new[] { "bushi" } };
        p1.PlayArea.Add(myBushi);
        p1.PlayArea.Add(myCourtier);
        p2.PlayArea.Add(opponentBushi);

        var conflict = new Conflict { AttackingPlayer = p1, DefendingPlayer = p2, ConflictType = "military" };
        conflict.Attackers.Add(myBushi);
        conflict.Attackers.Add(myCourtier);
        conflict.Defenders.Add(opponentBushi);
        game.CurrentConflict = conflict;

        var context = new AbilityContext { Game = game, Player = p1, Source = source };

        new ForGreaterGloryPlaceFateOnBushi().Execute(context);

        Assert.That(myBushi.Fate, Is.EqualTo(1));
        Assert.That(myCourtier.Fate, Is.EqualTo(0), "not a bushi");
        Assert.That(opponentBushi.Fate, Is.EqualTo(0), "not on the attacker's side");
    }

    [Test]
    public void WhenTheControllerIsNotAttacking_Throws()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var source = new Card { Id = "for-greater-glory", Type = CardType.Event, Controller = p1 };
        var myBushi = new Card { Id = "my-bushi", Type = CardType.Character, Controller = p1, Traits = new[] { "bushi" } };
        p1.PlayArea.Add(myBushi);

        var conflict = new Conflict { AttackingPlayer = p2, DefendingPlayer = p1, ConflictType = "military" };
        conflict.Defenders.Add(myBushi);
        game.CurrentConflict = conflict;

        var context = new AbilityContext { Game = game, Player = p1, Source = source };

        Assert.Throws<InvalidOperationException>(() => new ForGreaterGloryPlaceFateOnBushi().Execute(context));
    }

    [Test]
    public void DuringAPoliticalConflict_Throws()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var source = new Card { Id = "for-greater-glory", Type = CardType.Event, Controller = p1 };

        var conflict = new Conflict { AttackingPlayer = p1, DefendingPlayer = p2, ConflictType = "political" };
        game.CurrentConflict = conflict;

        var context = new AbilityContext { Game = game, Player = p1, Source = source };

        Assert.Throws<InvalidOperationException>(() => new ForGreaterGloryPlaceFateOnBushi().Execute(context));
    }
}
