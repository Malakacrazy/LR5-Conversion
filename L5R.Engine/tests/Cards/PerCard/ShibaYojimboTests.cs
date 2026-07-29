using L5R.Engine.Abilities;
using L5R.Engine.Cards.Scripts;
using L5R.Engine.Dsl;
using L5R.Engine.State;

namespace L5R.Engine.Tests.Cards.PerCard;

public class ShibaYojimboTests
{
    private static PendingAbility NewPendingAbility(AbilityContext otherContext, Card? chosenTarget = null) =>
        new()
        {
            Title = "Some other ability",
            Target = null,
            GameActions = Array.Empty<GameActionDefinition>(),
            Context = otherContext,
            ChosenTarget = chosenTarget
        };

    [Test]
    public void CancelsAnAbilityTargetingAFriendlyInPlayShugenja()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        var yojimbo = new Card { Id = "shiba-yojimbo", Type = CardType.Character, Controller = p1 };
        var shugenja = new Card { Id = "my-shugenja", Type = CardType.Character, Controller = p1, Traits = new[] { "shugenja" }, Location = "play area" };
        p1.PlayArea.Add(yojimbo);
        p1.PlayArea.Add(shugenja);

        var otherSource = new Card { Id = "opponent-source", Type = CardType.Character, Controller = p2 };
        var otherContext = new AbilityContext { Game = game, Player = p2, Source = otherSource };
        var pending = NewPendingAbility(otherContext, chosenTarget: shugenja);

        var context = new AbilityContext { Game = game, Player = p1, Source = yojimbo, InterruptedAbility = pending };

        new ShibaYojimboCancelShugenjaTargetedAbility().Execute(context);

        Assert.That(pending.Cancelled, Is.True);
    }

    [Test]
    public void AnAbilityNotTargetingAShugenja_Throws()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        var yojimbo = new Card { Id = "shiba-yojimbo", Type = CardType.Character, Controller = p1 };
        var nonShugenja = new Card { Id = "my-bushi", Type = CardType.Character, Controller = p1, Location = "play area" };
        p1.PlayArea.Add(yojimbo);
        p1.PlayArea.Add(nonShugenja);

        var otherSource = new Card { Id = "opponent-source", Type = CardType.Character, Controller = p2 };
        var otherContext = new AbilityContext { Game = game, Player = p2, Source = otherSource };
        var pending = NewPendingAbility(otherContext, chosenTarget: nonShugenja);

        var context = new AbilityContext { Game = game, Player = p1, Source = yojimbo, InterruptedAbility = pending };

        Assert.Throws<InvalidOperationException>(() => new ShibaYojimboCancelShugenjaTargetedAbility().Execute(context));
        Assert.That(pending.Cancelled, Is.False);
    }

    [Test]
    public void AShugenjaControlledByTheOpponent_DoesNotCount()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        var yojimbo = new Card { Id = "shiba-yojimbo", Type = CardType.Character, Controller = p1 };
        var opponentShugenja = new Card { Id = "opponent-shugenja", Type = CardType.Character, Controller = p2, Traits = new[] { "shugenja" }, Location = "play area" };
        p1.PlayArea.Add(yojimbo);
        p2.PlayArea.Add(opponentShugenja);

        var otherContext = new AbilityContext { Game = game, Player = p2, Source = opponentShugenja };
        var pending = NewPendingAbility(otherContext, chosenTarget: opponentShugenja);

        var context = new AbilityContext { Game = game, Player = p1, Source = yojimbo, InterruptedAbility = pending };

        Assert.Throws<InvalidOperationException>(() => new ShibaYojimboCancelShugenjaTargetedAbility().Execute(context));
    }
}
