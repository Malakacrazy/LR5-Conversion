using System.Text.Json;
using L5R.Engine.Abilities;
using L5R.Engine.Dsl;
using L5R.Engine.State;

namespace L5R.Engine.Tests.Cards.PerCard;

public class SoshiIllusionistTests
{
    private static ActionDefinition LoadFirstAction()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Cards", "01-Core", "soshi-illusionist.json");
        using var document = JsonDocument.Parse(File.ReadAllText(path));
        return AbilityDefinitionParser.ParseActions(document.RootElement).Single();
    }

    [Test]
    public void PayingOneFate_ClearsAnHonoredCharactersStatus()
    {
        var p1 = new Player { Name = "Player1", Fate = 1 };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        var illusionist = new Card { Id = "soshi-illusionist", Type = CardType.Character, Controller = p1 };
        var honoredCharacter = new Card { Id = "honored-character", Type = CardType.Character, Controller = p2, IsHonored = true };
        p1.PlayArea.Add(illusionist);
        p2.PlayArea.Add(honoredCharacter);

        var action = LoadFirstAction();
        var context = new AbilityContext { Game = game, Player = p1, Source = illusionist };
        var executor = new AbilityExecutor(new CostRegistry(), new GameActionRegistry());

        executor.Execute(action, context, chosenTarget: honoredCharacter);

        Assert.That(p1.Fate, Is.EqualTo(0));
        Assert.That(honoredCharacter.IsHonored, Is.False);
    }

    [Test]
    public void ClearsADishonoredCharactersStatus()
    {
        var p1 = new Player { Name = "Player1", Fate = 1 };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        var illusionist = new Card { Id = "soshi-illusionist", Type = CardType.Character, Controller = p1 };
        var dishonoredCharacter = new Card { Id = "dishonored-character", Type = CardType.Character, Controller = p2, IsDishonored = true };
        p1.PlayArea.Add(illusionist);
        p2.PlayArea.Add(dishonoredCharacter);

        var action = LoadFirstAction();
        var context = new AbilityContext { Game = game, Player = p1, Source = illusionist };
        var executor = new AbilityExecutor(new CostRegistry(), new GameActionRegistry());

        executor.Execute(action, context, chosenTarget: dishonoredCharacter);

        Assert.That(dishonoredCharacter.IsDishonored, Is.False);
    }

    [Test]
    public void ACharacterWithNoStatus_IsUnaffected()
    {
        var p1 = new Player { Name = "Player1", Fate = 1 };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        var illusionist = new Card { Id = "soshi-illusionist", Type = CardType.Character, Controller = p1 };
        var ordinaryCharacter = new Card { Id = "ordinary-character", Type = CardType.Character, Controller = p2 };
        p1.PlayArea.Add(illusionist);
        p2.PlayArea.Add(ordinaryCharacter);

        var action = LoadFirstAction();
        var context = new AbilityContext { Game = game, Player = p1, Source = illusionist };
        var executor = new AbilityExecutor(new CostRegistry(), new GameActionRegistry());

        executor.Execute(action, context, chosenTarget: ordinaryCharacter);

        Assert.That(ordinaryCharacter.IsHonored, Is.False);
        Assert.That(ordinaryCharacter.IsDishonored, Is.False);
    }

    [Test]
    public void WithoutEnoughFate_ThrowsAndDoesNotClearTheStatus()
    {
        var p1 = new Player { Name = "Player1", Fate = 0 };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        var illusionist = new Card { Id = "soshi-illusionist", Type = CardType.Character, Controller = p1 };
        var honoredCharacter = new Card { Id = "honored-character", Type = CardType.Character, Controller = p2, IsHonored = true };
        p1.PlayArea.Add(illusionist);
        p2.PlayArea.Add(honoredCharacter);

        var action = LoadFirstAction();
        var context = new AbilityContext { Game = game, Player = p1, Source = illusionist };
        var executor = new AbilityExecutor(new CostRegistry(), new GameActionRegistry());

        Assert.Throws<InvalidOperationException>(() => executor.Execute(action, context, chosenTarget: honoredCharacter));
        Assert.That(honoredCharacter.IsHonored, Is.True, "the cost was never actually paid, so the effect never ran");
    }
}
