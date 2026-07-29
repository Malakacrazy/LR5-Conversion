using System.Text.Json;
using L5R.Engine.Abilities;
using L5R.Engine.Dsl;
using L5R.Engine.State;

namespace L5R.Engine.Tests.Cards.PerCard;

public class TranquilityTests
{
    private static ActionDefinition LoadFirstAction()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Cards", "01-Core", "tranquility.json");
        using var document = JsonDocument.Parse(File.ReadAllText(path));
        return AbilityDefinitionParser.ParseActions(document.RootElement).Single();
    }

    [Test]
    public void DuringConflict_DisablesOpponentsNonParticipatingCharacters()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var source = new Card { Id = "tranquility", Type = CardType.Event, Controller = p1 };
        var homeCharacter = new Card { Id = "home-character", Type = CardType.Character, Controller = p2 };
        var participatingCharacter = new Card { Id = "participating-character", Type = CardType.Character, Controller = p2 };
        p2.PlayArea.Add(homeCharacter);
        p2.PlayArea.Add(participatingCharacter);

        var conflict = new Conflict { AttackingPlayer = p1, DefendingPlayer = p2 };
        conflict.Defenders.Add(participatingCharacter);
        game.CurrentConflict = conflict;

        var action = LoadFirstAction();
        var context = new AbilityContext { Game = game, Player = p1, Source = source };
        var executor = new AbilityExecutor(new CostRegistry(), new GameActionRegistry());

        executor.Execute(action, context);

        Assert.That(game.IsRestrictedFrom(homeCharacter, "triggerAbilities"), Is.True, "not participating, so it's at home");
        Assert.That(game.IsRestrictedFrom(participatingCharacter, "triggerAbilities"), Is.False, "participating characters are unaffected");
    }
}
