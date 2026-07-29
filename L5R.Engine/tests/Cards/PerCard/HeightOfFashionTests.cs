using L5R.Engine.Abilities;
using L5R.Engine.Cards.Scripts;
using L5R.Engine.Dsl.GameActions;
using L5R.Engine.State;

namespace L5R.Engine.Tests.Cards.PerCard;

public class HeightOfFashionTests
{
    [Test]
    public void WithNoActiveConflict_CanBePlayed()
    {
        var p1 = new Player { Name = "Player1", Fate = 5 };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Dynasty };
        var character = new Card { Id = "some-character", Type = CardType.Character, Controller = p1 };
        var heightOfFashion = new Card
        {
            Id = "height-of-fashion", Type = CardType.Attachment, Controller = p1,
            PrintedCost = 2, PlayScript = new HeightOfFashionCannotPlayDuringConflict()
        };
        p1.Hand.Add(heightOfFashion);
        p1.PlayArea.Add(character);

        var context = new AbilityContext { Game = game, Player = p1, Source = heightOfFashion, Target = heightOfFashion, PlayAttachTarget = character };

        new PlayCardGameActionHandler().Execute(context, null);

        Assert.That(p1.PlayArea, Does.Contain(heightOfFashion));
    }

    [Test]
    public void DuringAConflict_CannotBePlayed()
    {
        var p1 = new Player { Name = "Player1", Fate = 5 };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var character = new Card { Id = "some-character", Type = CardType.Character, Controller = p1 };
        var heightOfFashion = new Card
        {
            Id = "height-of-fashion", Type = CardType.Attachment, Controller = p1,
            PrintedCost = 2, PlayScript = new HeightOfFashionCannotPlayDuringConflict()
        };
        p1.Hand.Add(heightOfFashion);
        p1.PlayArea.Add(character);
        game.CurrentConflict = new Conflict { AttackingPlayer = p1, DefendingPlayer = p2 };

        var context = new AbilityContext { Game = game, Player = p1, Source = heightOfFashion, Target = heightOfFashion, PlayAttachTarget = character };

        Assert.Throws<InvalidOperationException>(() => new PlayCardGameActionHandler().Execute(context, null));
    }
}
