using L5R.Engine.Abilities;
using L5R.Engine.Dsl;
using L5R.Engine.Dsl.GameActions;
using L5R.Engine.State;

namespace L5R.Engine.Tests.Dsl;

public class IkomaProdigyFirerTests
{
    [Test]
    public void FireIfLegal_WithFateOnIt_GainsOneHonor()
    {
        var p1 = new Player { Name = "Player1", Honor = 5 };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        var prodigy = new Card { Id = "ikoma-prodigy", Type = CardType.Character, Controller = p1, Fate = 1 };
        p1.PlayArea.Add(prodigy);

        IkomaProdigyFirer.FireIfLegal(game, prodigy);

        Assert.That(p1.Honor, Is.EqualTo(6));
    }

    [Test]
    public void FireIfLegal_WithNoFate_DoesNotFire()
    {
        var p1 = new Player { Name = "Player1", Honor = 5 };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        var prodigy = new Card { Id = "ikoma-prodigy", Type = CardType.Character, Controller = p1, Fate = 0 };
        p1.PlayArea.Add(prodigy);

        IkomaProdigyFirer.FireIfLegal(game, prodigy);

        Assert.That(p1.Honor, Is.EqualTo(5));
    }

    [Test]
    public void FireIfLegal_ForADifferentCard_DoesNotFire()
    {
        var p1 = new Player { Name = "Player1", Honor = 5 };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        var otherCard = new Card { Id = "some-other-character", Type = CardType.Character, Controller = p1, Fate = 3 };
        p1.PlayArea.Add(otherCard);

        IkomaProdigyFirer.FireIfLegal(game, otherCard);

        Assert.That(p1.Honor, Is.EqualTo(5));
    }

    [Test]
    public void PlaceFateGameActionHandler_PlacingFateOnIkomaProdigy_FiresItsReaction()
    {
        var p1 = new Player { Name = "Player1", Honor = 5 };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        var prodigy = new Card { Id = "ikoma-prodigy", Type = CardType.Character, Controller = p1 };
        p1.PlayArea.Add(prodigy);

        var context = new AbilityContext { Game = game, Player = p1, Source = prodigy, Target = prodigy };
        new PlaceFateGameActionHandler().Execute(context, null);

        Assert.That(prodigy.Fate, Is.EqualTo(1));
        Assert.That(p1.Honor, Is.EqualTo(6));
    }
}
