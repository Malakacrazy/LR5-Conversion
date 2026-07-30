using L5R.Engine.Abilities;
using L5R.Engine.Cards.Scripts;
using L5R.Engine.State;

namespace L5R.Engine.Tests.Cards.PerCard;

public class HidaKisadaTests
{
    private static (GameState Game, Card Kisada) NewGameWithActiveConflict()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var kisada = new Card { Id = "hida-kisada", Type = CardType.Character, Controller = p1 };
        p1.PlayArea.Add(kisada);

        var conflict = new Conflict { AttackingPlayer = p2, DefendingPlayer = p1 };
        game.CurrentConflict = conflict;

        return (game, kisada);
    }

    [Test]
    public void CancelsTheFirstActionEachConflict()
    {
        var (game, kisada) = NewGameWithActiveConflict();
        var context = new AbilityContext { Game = game, Player = game.Player1, Source = kisada };

        new HidaKisadaCancelOpponentsFirstActionEachConflict().Execute(context);

        Assert.That(game.FirstActionCancelledThisConflict, Is.True);
    }

    [Test]
    public void CannotCancelASecondActionInTheSameConflict()
    {
        var (game, kisada) = NewGameWithActiveConflict();
        var context = new AbilityContext { Game = game, Player = game.Player1, Source = kisada };
        new HidaKisadaCancelOpponentsFirstActionEachConflict().Execute(context);

        Assert.Throws<InvalidOperationException>(() => new HidaKisadaCancelOpponentsFirstActionEachConflict().Execute(context));
    }

    [Test]
    public void ResetsForANewConflict()
    {
        var (game, kisada) = NewGameWithActiveConflict();
        var firstContext = new AbilityContext { Game = game, Player = game.Player1, Source = kisada };
        new HidaKisadaCancelOpponentsFirstActionEachConflict().Execute(firstContext);

        game.EndConflict();
        game.CurrentConflict = new Conflict { AttackingPlayer = game.Player2, DefendingPlayer = game.Player1 };

        var secondContext = new AbilityContext { Game = game, Player = game.Player1, Source = kisada };
        new HidaKisadaCancelOpponentsFirstActionEachConflict().Execute(secondContext);

        Assert.That(game.FirstActionCancelledThisConflict, Is.True);
    }

    [Test]
    public void WhenTheOpponentAlreadyWonAConflictThisRound_Throws()
    {
        var (game, kisada) = NewGameWithActiveConflict();
        game.ConflictRecord.Add(new Conflict { AttackingPlayer = game.Player2, DefendingPlayer = game.Player1, Winner = game.Player2 });

        var context = new AbilityContext { Game = game, Player = game.Player1, Source = kisada };

        Assert.Throws<InvalidOperationException>(() => new HidaKisadaCancelOpponentsFirstActionEachConflict().Execute(context));
    }

    [Test]
    public void WhenBlanked_Throws()
    {
        var (game, kisada) = NewGameWithActiveConflict();
        var blankEffect = new L5R.Engine.Dsl.WhileAttachedDefinition(
            null, null, new[] { System.Text.Json.JsonDocument.Parse("{\"name\":\"blank\"}").RootElement });
        var blanker = new Card
        {
            Id = "blanker", Type = CardType.Attachment, Controller = game.Player1, AttachedTo = kisada,
            WhileAttachedEffects = new[] { blankEffect }
        };
        game.Player1.PlayArea.Add(blanker);

        var context = new AbilityContext { Game = game, Player = game.Player1, Source = kisada };

        Assert.Throws<InvalidOperationException>(() => new HidaKisadaCancelOpponentsFirstActionEachConflict().Execute(context));
    }
}
