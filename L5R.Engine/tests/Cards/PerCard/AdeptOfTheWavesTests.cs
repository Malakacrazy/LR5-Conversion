using System.Text.Json;
using L5R.Engine.Abilities;
using L5R.Engine.Dsl;
using L5R.Engine.State;

namespace L5R.Engine.Tests.Cards.PerCard;

public class AdeptOfTheWavesTests
{
    private static ActionDefinition LoadFirstAction()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Cards", "01-Core", "adept-of-the-waves.json");
        using var document = JsonDocument.Parse(File.ReadAllText(path));
        return AbilityDefinitionParser.ParseActions(document.RootElement).Single();
    }

    [Test]
    public void DuringAWaterConflict_GrantsCovertToTheChosenCharacter()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var adept = new Card { Id = "adept-of-the-waves", Type = CardType.Character, Controller = p1 };
        var recipient = new Card { Id = "recipient", Type = CardType.Character, Controller = p1 };
        p1.PlayArea.Add(adept);
        p1.PlayArea.Add(recipient);
        game.CurrentConflict = new Conflict { AttackingPlayer = p1, DefendingPlayer = p2, ConflictType = "water" };

        var action = LoadFirstAction();
        var context = new AbilityContext { Game = game, Player = p1, Source = adept };
        var executor = new AbilityExecutor(new CostRegistry(), new GameActionRegistry());

        executor.Execute(action, context, chosenTarget: recipient);

        Assert.That(game.HasKeyword(recipient, "covert"), Is.True);
    }

    [Test]
    public void OutsideOfAWaterConflict_DoesNotGrantCovert()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var adept = new Card { Id = "adept-of-the-waves", Type = CardType.Character, Controller = p1 };
        var recipient = new Card { Id = "recipient", Type = CardType.Character, Controller = p1 };
        p1.PlayArea.Add(adept);
        p1.PlayArea.Add(recipient);
        game.CurrentConflict = new Conflict { AttackingPlayer = p1, DefendingPlayer = p2, ConflictType = "military" };

        var action = LoadFirstAction();
        var context = new AbilityContext { Game = game, Player = p1, Source = adept };
        var executor = new AbilityExecutor(new CostRegistry(), new GameActionRegistry());

        executor.Execute(action, context, chosenTarget: recipient);

        Assert.That(game.HasKeyword(recipient, "covert"), Is.False, "the grant is re-checked live, not just at the moment it was applied");
    }

    [Test]
    public void ExpiresAtEndOfPhase()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var adept = new Card { Id = "adept-of-the-waves", Type = CardType.Character, Controller = p1 };
        var recipient = new Card { Id = "recipient", Type = CardType.Character, Controller = p1 };
        p1.PlayArea.Add(adept);
        p1.PlayArea.Add(recipient);
        game.CurrentConflict = new Conflict { AttackingPlayer = p1, DefendingPlayer = p2, ConflictType = "water" };

        var action = LoadFirstAction();
        var context = new AbilityContext { Game = game, Player = p1, Source = adept };
        var executor = new AbilityExecutor(new CostRegistry(), new GameActionRegistry());

        executor.Execute(action, context, chosenTarget: recipient);
        game.AdvancePhase();

        Assert.That(game.HasKeyword(recipient, "covert"), Is.False);
    }
}
