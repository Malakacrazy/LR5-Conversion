using L5R.Engine.GameSteps.BotActions;
using L5R.Engine.State;

namespace L5R.Engine.Tests.GameSteps.BotActions;

public class MantraOfFireBotActionTests
{
    private static (GameState game, Card mof, Card monk) NewScenario()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        var mof = new Card { Id = "mantra-of-fire", Type = CardType.Event, Controller = p1 };
        var monk = new Card { Id = "monk-ally", Type = CardType.Character, Controller = p1, Traits = new[] { "monk" } };
        p1.PlayArea.Add(monk);
        p1.Deck.Add(new Card { Id = "deck-card", Type = CardType.Character, Controller = p1 });
        var conflict = new Conflict { AttackingPlayer = p2, DefendingPlayer = p1 };
        conflict.Elements.Add("fire");
        game.CurrentConflict = conflict;
        return (game, mof, monk);
    }

    [Test]
    public void IsLegal_OnOpponentsFireConflictWithAMonkAlly_True()
    {
        var (game, mof, _) = NewScenario();
        Assert.That(new MantraOfFireBotAction().IsLegal(game, mof, game.Player1), Is.True);
    }

    [Test]
    public void Invoke_PlacesFateOnTheMonkAndDraws()
    {
        var (game, mof, monk) = NewScenario();
        new MantraOfFireBotAction().Invoke(game, mof, game.Player1);

        Assert.That(monk.Fate, Is.EqualTo(1));
        Assert.That(game.Player1.Hand, Has.Count.EqualTo(1));
    }
}
