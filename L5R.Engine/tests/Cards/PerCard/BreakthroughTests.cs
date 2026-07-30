using L5R.Engine.Abilities;
using L5R.Engine.Cards.Scripts;
using L5R.Engine.State;

namespace L5R.Engine.Tests.Cards.PerCard;

public class BreakthroughTests
{
    private static (GameState Game, Card Breakthrough) NewGameWithFinishedConflict(bool attackerWon, bool provinceBroken, int myDeclarations)
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var breakthrough = new Card { Id = "breakthrough", Type = CardType.Event, Controller = p1 };
        var province = new Card { Id = "some-province", Type = CardType.Province, Controller = p2, Broken = provinceBroken };

        var finishedConflict = new Conflict { AttackingPlayer = p1, DefendingPlayer = p2, Winner = attackerWon ? p1 : p2, DeclaredProvince = province };
        game.ConflictRecord.Add(finishedConflict);

        for (var i = 0; i < myDeclarations; i++)
            game.ConflictDeclarationsThisPhase.Add((p1, false));

        return (game, breakthrough);
    }

    [Test]
    public void WhenAttackerWonAndBrokeTheProvinceAsTheOnlyDeclarationThisPhase_DeclaresASecondConflict()
    {
        var (game, breakthrough) = NewGameWithFinishedConflict(attackerWon: true, provinceBroken: true, myDeclarations: 1);
        var context = new AbilityContext { Game = game, Player = game.Player1, Source = breakthrough };

        new BreakthroughDeclareSecondConflict().Execute(context);

        Assert.That(game.CurrentConflict, Is.Not.Null);
        Assert.That(game.CurrentConflict!.AttackingPlayer, Is.EqualTo(game.Player1));
        Assert.That(game.ConflictDeclarationsThisPhase.Count(d => d.Player == game.Player1 && !d.Passed), Is.EqualTo(2));
    }

    [Test]
    public void WhenTheProvinceWasNotBroken_Throws()
    {
        var (game, breakthrough) = NewGameWithFinishedConflict(attackerWon: true, provinceBroken: false, myDeclarations: 1);
        var context = new AbilityContext { Game = game, Player = game.Player1, Source = breakthrough };

        Assert.Throws<InvalidOperationException>(() => new BreakthroughDeclareSecondConflict().Execute(context));
    }

    [Test]
    public void WhenItAlreadyDeclaredMoreThanOneConflictThisPhase_Throws()
    {
        var (game, breakthrough) = NewGameWithFinishedConflict(attackerWon: true, provinceBroken: true, myDeclarations: 2);
        var context = new AbilityContext { Game = game, Player = game.Player1, Source = breakthrough };

        Assert.Throws<InvalidOperationException>(() => new BreakthroughDeclareSecondConflict().Execute(context));
    }

    [Test]
    public void WhenItsControllerLost_Throws()
    {
        var (game, breakthrough) = NewGameWithFinishedConflict(attackerWon: false, provinceBroken: true, myDeclarations: 1);
        var context = new AbilityContext { Game = game, Player = game.Player1, Source = breakthrough };

        Assert.Throws<InvalidOperationException>(() => new BreakthroughDeclareSecondConflict().Execute(context));
    }
}
