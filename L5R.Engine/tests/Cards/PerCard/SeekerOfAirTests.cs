using L5R.Engine.Abilities;
using L5R.Engine.Cards.Scripts;
using L5R.Engine.State;

namespace L5R.Engine.Tests.Cards.PerCard;

public class SeekerOfAirTests
{
    [Test]
    public void WhenAnAirProvinceItControlsIsRevealed_GainsOneFate()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Dynasty };
        var seeker = new Card { Id = "seeker-of-air", Type = CardType.Role, Controller = p1 };
        var province = new Card { Id = "some-province", Type = CardType.Province, Controller = p1, Traits = new[] { "air" } };

        var context = new AbilityContext { Game = game, Player = p1, Source = seeker, Target = province };

        new SeekerOfAirGainFateOnMatchingProvinceReveal().Execute(context);

        Assert.That(p1.Fate, Is.EqualTo(1));
    }

    [Test]
    public void WhenTheRevealedProvinceIsNotAir_Throws()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Dynasty };
        var seeker = new Card { Id = "seeker-of-air", Type = CardType.Role, Controller = p1 };
        var province = new Card { Id = "some-province", Type = CardType.Province, Controller = p1, Traits = new[] { "fire" } };

        var context = new AbilityContext { Game = game, Player = p1, Source = seeker, Target = province };

        Assert.Throws<InvalidOperationException>(() => new SeekerOfAirGainFateOnMatchingProvinceReveal().Execute(context));
    }

    [Test]
    public void WhenTheProvinceBelongsToTheOpponent_Throws()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Dynasty };
        var seeker = new Card { Id = "seeker-of-air", Type = CardType.Role, Controller = p1 };
        var province = new Card { Id = "opponent-province", Type = CardType.Province, Controller = p2, Traits = new[] { "air" } };

        var context = new AbilityContext { Game = game, Player = p1, Source = seeker, Target = province };

        Assert.Throws<InvalidOperationException>(() => new SeekerOfAirGainFateOnMatchingProvinceReveal().Execute(context));
    }
}
