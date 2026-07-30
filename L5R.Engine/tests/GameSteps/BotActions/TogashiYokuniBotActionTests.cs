using L5R.Engine.Abilities;
using L5R.Engine.Dsl;
using L5R.Engine.GameSteps.BotActions;
using L5R.Engine.State;

namespace L5R.Engine.Tests.GameSteps.BotActions;

public class TogashiYokuniBotActionTests
{
    private static ActionDefinition SyntheticGainFateAction() =>
        new("Some character's ability", Array.Empty<CostDefinition>(), null, new[] { new GameActionDefinition("gainFate", null) }, null, null);

    private static (GameState game, Card yokuni, Card otherCharacter) NewScenario()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        var yokuni = new Card { Id = "togashi-yokuni", Type = CardType.Character, Controller = p1 };
        var otherCharacter = new Card { Id = "other-character", Type = CardType.Character, Controller = p2 };
        otherCharacter.Actions.Add(new CardAction { Title = "Some Ability", Card = otherCharacter, Definition = SyntheticGainFateAction() });
        p1.PlayArea.Add(yokuni);
        p2.PlayArea.Add(otherCharacter);

        return (game, yokuni, otherCharacter);
    }

    [Test]
    public void IsLegal_WithAnotherCharacterThatHasABridgedAction_True()
    {
        var (game, yokuni, _) = NewScenario();
        Assert.That(new TogashiYokuniBotAction().IsLegal(game, yokuni, game.Player1), Is.True);
    }

    [Test]
    public void IsLegal_WithNoOtherCharacterHavingABridgedAction_False()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        var yokuni = new Card { Id = "togashi-yokuni", Type = CardType.Character, Controller = p1 };
        var otherCharacter = new Card { Id = "other-character", Type = CardType.Character, Controller = p2 };
        p1.PlayArea.Add(yokuni);
        p2.PlayArea.Add(otherCharacter);

        Assert.That(new TogashiYokuniBotAction().IsLegal(game, yokuni, game.Player1), Is.False);
    }

    [Test]
    public void Invoke_CopiesTheOtherCharactersAbility()
    {
        var (game, yokuni, otherCharacter) = NewScenario();

        new TogashiYokuniBotAction().Invoke(game, yokuni, game.Player1);

        var (grantedTo, grantedAbility) = game.GainedAbilities.Single();
        Assert.That(grantedTo, Is.EqualTo(yokuni));
        Assert.That(grantedAbility, Is.EqualTo(otherCharacter.Actions[0].Definition));
    }
}
