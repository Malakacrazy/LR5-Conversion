using L5R.Engine.Abilities;
using L5R.Engine.Cards.Scripts;
using L5R.Engine.State;

namespace L5R.Engine.Tests.Cards.PerCard;

public class ObstinateRecruitTests
{
    [Test]
    public void WhenTheOpponentIsMoreHonorable_DiscardsItself()
    {
        var p1 = new Player { Name = "Player1", Honor = 3 };
        var p2 = new Player { Name = "Player2", Honor = 5 };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        var recruit = new Card { Id = "obstinate-recruit", Type = CardType.Character, Controller = p1 };
        p1.PlayArea.Add(recruit);

        var context = new AbilityContext { Game = game, Player = p1, Source = recruit };

        new ObstinateRecruitDiscardWhenOpponentMoreHonorable().Execute(context);

        Assert.That(p1.Discard, Does.Contain(recruit));
    }

    [Test]
    public void WhenTheOpponentIsNotMoreHonorable_Throws()
    {
        var p1 = new Player { Name = "Player1", Honor = 5 };
        var p2 = new Player { Name = "Player2", Honor = 5 };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        var recruit = new Card { Id = "obstinate-recruit", Type = CardType.Character, Controller = p1 };
        p1.PlayArea.Add(recruit);

        var context = new AbilityContext { Game = game, Player = p1, Source = recruit };

        Assert.Throws<InvalidOperationException>(() => new ObstinateRecruitDiscardWhenOpponentMoreHonorable().Execute(context));
    }
}
