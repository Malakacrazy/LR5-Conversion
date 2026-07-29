using L5R.Engine.Abilities;
using L5R.Engine.Cards.Scripts;
using L5R.Engine.State;

namespace L5R.Engine.Tests.Cards.PerCard;

public class SecludedTempleTests
{
    [Test]
    public void WhenOutnumbered_RemovesFateFromAnOpponentCharacter()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var temple = new Card { Id = "secluded-temple", Type = CardType.Holding, Controller = p1 };
        var opponentCharacter1 = new Card { Id = "opponent-1", Type = CardType.Character, Controller = p2, Fate = 2 };
        var opponentCharacter2 = new Card { Id = "opponent-2", Type = CardType.Character, Controller = p2 };
        p2.PlayArea.Add(opponentCharacter1);
        p2.PlayArea.Add(opponentCharacter2);

        var context = new AbilityContext { Game = game, Player = p1, Source = temple, Target = opponentCharacter1 };

        new SecludedTempleRemoveFateWhenOutnumbered().Execute(context);

        Assert.That(opponentCharacter1.Fate, Is.EqualTo(1));
    }

    [Test]
    public void OutsideTheConflictPhase_Throws()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Dynasty };
        var temple = new Card { Id = "secluded-temple", Type = CardType.Holding, Controller = p1 };
        var opponentCharacter1 = new Card { Id = "opponent-1", Type = CardType.Character, Controller = p2 };
        var opponentCharacter2 = new Card { Id = "opponent-2", Type = CardType.Character, Controller = p2 };
        p2.PlayArea.Add(opponentCharacter1);
        p2.PlayArea.Add(opponentCharacter2);

        var context = new AbilityContext { Game = game, Player = p1, Source = temple, Target = opponentCharacter1 };

        Assert.Throws<InvalidOperationException>(() => new SecludedTempleRemoveFateWhenOutnumbered().Execute(context));
    }

    [Test]
    public void WhenNotOutnumbered_Throws()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var temple = new Card { Id = "secluded-temple", Type = CardType.Holding, Controller = p1 };
        var myCharacter = new Card { Id = "my-character", Type = CardType.Character, Controller = p1 };
        var opponentCharacter = new Card { Id = "opponent-1", Type = CardType.Character, Controller = p2 };
        p1.PlayArea.Add(myCharacter);
        p2.PlayArea.Add(opponentCharacter);

        var context = new AbilityContext { Game = game, Player = p1, Source = temple, Target = opponentCharacter };

        Assert.Throws<InvalidOperationException>(() => new SecludedTempleRemoveFateWhenOutnumbered().Execute(context));
    }
}
