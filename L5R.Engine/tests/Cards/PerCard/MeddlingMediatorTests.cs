using L5R.Engine.Abilities;
using L5R.Engine.Cards.Scripts;
using L5R.Engine.State;

namespace L5R.Engine.Tests.Cards.PerCard;

public class MeddlingMediatorTests
{
    private static (GameState Game, Card Mediator) NewGameWithOpponentDeclarations(int declarations)
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var mediator = new Card { Id = "meddling-mediator", Type = CardType.Character, Controller = p1 };
        p1.PlayArea.Add(mediator);

        for (var i = 0; i < declarations; i++)
            game.ConflictDeclarationsThisPhase.Add((p2, false));

        return (game, mediator);
    }

    [Test]
    public void WhenTheOpponentHasDeclaredMoreThanOneConflict_TakesAFate()
    {
        var (game, mediator) = NewGameWithOpponentDeclarations(2);
        game.Player2.Fate = 2;
        var context = new AbilityContext { Game = game, Player = game.Player1, Source = mediator, ChosenChoice = "Take 1 fate" };

        new MeddlingMediatorTakeFateOrHonorWhenDoublyAttacked().Execute(context);

        Assert.That(game.Player1.Fate, Is.EqualTo(1));
        Assert.That(game.Player2.Fate, Is.EqualTo(1));
    }

    [Test]
    public void CanTakeHonorInstead()
    {
        var (game, mediator) = NewGameWithOpponentDeclarations(2);
        game.Player2.Honor = 3;
        var context = new AbilityContext { Game = game, Player = game.Player1, Source = mediator, ChosenChoice = "Take 1 honor" };

        new MeddlingMediatorTakeFateOrHonorWhenDoublyAttacked().Execute(context);

        Assert.That(game.Player1.Honor, Is.EqualTo(1));
        Assert.That(game.Player2.Honor, Is.EqualTo(2));
    }

    [Test]
    public void WithOnlyOneOpponentDeclaration_Throws()
    {
        var (game, mediator) = NewGameWithOpponentDeclarations(1);
        var context = new AbilityContext { Game = game, Player = game.Player1, Source = mediator, ChosenChoice = "Take 1 fate" };

        Assert.Throws<InvalidOperationException>(() => new MeddlingMediatorTakeFateOrHonorWhenDoublyAttacked().Execute(context));
    }

    [Test]
    public void OutsideTheConflictPhase_Throws()
    {
        var (game, mediator) = NewGameWithOpponentDeclarations(2);
        game.CurrentPhase = Phase.Fate;
        var context = new AbilityContext { Game = game, Player = game.Player1, Source = mediator, ChosenChoice = "Take 1 fate" };

        Assert.Throws<InvalidOperationException>(() => new MeddlingMediatorTakeFateOrHonorWhenDoublyAttacked().Execute(context));
    }
}
