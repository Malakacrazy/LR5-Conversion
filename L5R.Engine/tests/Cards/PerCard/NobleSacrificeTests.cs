using System.Text.Json;
using L5R.Engine.Abilities;
using L5R.Engine.Dsl;
using L5R.Engine.State;

namespace L5R.Engine.Tests.Cards.PerCard;

public class NobleSacrificeTests
{
    private static ActionDefinition LoadFirstAction()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Cards", "01-Core", "noble-sacrifice.json");
        using var document = JsonDocument.Parse(File.ReadAllText(path));
        return AbilityDefinitionParser.ParseActions(document.RootElement).Single();
    }

    [Test]
    public void SacrificingHonoredCharacterDiscardsChosenDishonoredCharacter()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var source = new Card { Id = "noble-sacrifice", Type = CardType.Event, Controller = p1 };
        var honoredCharacter = new Card { Id = "honored-character", Type = CardType.Character, Controller = p1, IsHonored = true };
        var dishonoredCharacter = new Card { Id = "dishonored-character", Type = CardType.Character, Controller = p2, IsDishonored = true };
        p1.PlayArea.Add(honoredCharacter);
        p2.PlayArea.Add(dishonoredCharacter);

        var action = LoadFirstAction();
        var context = new AbilityContext { Game = game, Player = p1, Source = source };
        var executor = new AbilityExecutor(new CostRegistry(), new GameActionRegistry());

        executor.Execute(action, context, chosenTarget: dishonoredCharacter, chosenCostTarget: honoredCharacter);

        Assert.That(p1.Discard, Does.Contain(honoredCharacter));
        Assert.That(p2.Discard, Does.Contain(dishonoredCharacter));
    }

    [Test]
    public void CannotPayCost_WhenOwnHonoredCharacterIsAbsent()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var source = new Card { Id = "noble-sacrifice", Type = CardType.Event, Controller = p1 };
        var unhonoredCharacter = new Card { Id = "unhonored-character", Type = CardType.Character, Controller = p1 };
        var dishonoredCharacter = new Card { Id = "dishonored-character", Type = CardType.Character, Controller = p2, IsDishonored = true };
        p1.PlayArea.Add(unhonoredCharacter);
        p2.PlayArea.Add(dishonoredCharacter);

        var action = LoadFirstAction();
        var context = new AbilityContext { Game = game, Player = p1, Source = source };
        var executor = new AbilityExecutor(new CostRegistry(), new GameActionRegistry());

        Assert.Throws<InvalidOperationException>(
            () => executor.Execute(action, context, chosenTarget: dishonoredCharacter));
    }
}
