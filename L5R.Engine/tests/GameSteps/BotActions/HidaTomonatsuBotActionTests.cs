using L5R.Engine.GameSteps.BotActions;
using L5R.Engine.State;

namespace L5R.Engine.Tests.GameSteps.BotActions;

public class HidaTomonatsuBotActionTests
{
    private static GameState NewScenario(out Player p1, out Card tomonatsu, out Card attacker)
    {
        p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p2 };

        tomonatsu = new Card { Id = "hida-tomonatsu", Type = CardType.Character, Controller = p1 };
        attacker = new Card { Id = "attacker", Type = CardType.Character, Controller = p2 };
        p1.PlayArea.Add(tomonatsu);
        p2.PlayArea.Add(attacker);
        p2.Deck.Add(new Card { Id = "deck-filler", Type = CardType.Character, Controller = p2 });

        var conflict = new Conflict { AttackingPlayer = p2, DefendingPlayer = p1, Winner = p1 };
        conflict.Defenders.Add(tomonatsu);
        conflict.Attackers.Add(attacker);
        game.CurrentConflict = conflict;

        return game;
    }

    [Test]
    public void IsLegal_WhenDefendingAndWinning_True()
    {
        var game = NewScenario(out var p1, out var tomonatsu, out _);

        Assert.That(new HidaTomonatsuBotAction().IsLegal(game, tomonatsu, p1), Is.True);
    }

    [Test]
    public void IsLegal_WhenTheOnlyAttackerIsUnique_False()
    {
        var game = NewScenario(out var p1, out var tomonatsu, out var attacker);
        game.CurrentConflict!.Attackers.Remove(attacker);
        game.CurrentConflict!.Attackers.Add(new Card { Id = "unique-attacker", Type = CardType.Character, Controller = game.Player2, Unique = true });

        Assert.That(new HidaTomonatsuBotAction().IsLegal(game, tomonatsu, p1), Is.False);
    }

    [Test]
    public void Invoke_SacrificesItselfAndReturnsTheAttackerToTheTopOfItsDeck()
    {
        var game = NewScenario(out var p1, out var tomonatsu, out var attacker);

        new HidaTomonatsuBotAction().Invoke(game, tomonatsu, p1);

        Assert.That(p1.Discard, Contains.Item(tomonatsu));
        Assert.That(game.Player2.Deck[0], Is.EqualTo(attacker));
    }
}
