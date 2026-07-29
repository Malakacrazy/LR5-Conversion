using L5R.Engine.Abilities;
using L5R.Engine.Cards.Scripts;
using L5R.Engine.State;

namespace L5R.Engine.Tests.Cards.PerCard;

public class IkomaProdigyTests
{
    [Test]
    public void WithFateOnItself_GainsOneHonor()
    {
        var p1 = new Player { Name = "Player1", Honor = 3 };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Dynasty };
        var prodigy = new Card { Id = "ikoma-prodigy", Type = CardType.Character, Controller = p1, Fate = 1 };
        p1.PlayArea.Add(prodigy);

        var context = new AbilityContext { Game = game, Player = p1, Source = prodigy };

        new IkomaProdigyGainHonorWhenFateAddedOrMoved().Execute(context);

        Assert.That(p1.Honor, Is.EqualTo(4));
    }

    [Test]
    public void WithNoFateOnItself_Throws()
    {
        var p1 = new Player { Name = "Player1", Honor = 3 };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Dynasty };
        var prodigy = new Card { Id = "ikoma-prodigy", Type = CardType.Character, Controller = p1, Fate = 0 };
        p1.PlayArea.Add(prodigy);

        var context = new AbilityContext { Game = game, Player = p1, Source = prodigy };

        Assert.Throws<InvalidOperationException>(() => new IkomaProdigyGainHonorWhenFateAddedOrMoved().Execute(context));
    }
}
