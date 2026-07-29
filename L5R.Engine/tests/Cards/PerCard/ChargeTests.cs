using System.Text.Json;
using L5R.Engine.Abilities;
using L5R.Engine.Dsl;
using L5R.Engine.State;

namespace L5R.Engine.Tests.Cards.PerCard;

public class ChargeTests
{
    private static ActionDefinition LoadFirstAction()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Cards", "01-Core", "charge.json");
        using var document = JsonDocument.Parse(File.ReadAllText(path));
        return AbilityDefinitionParser.ParseActions(document.RootElement).Single();
    }

    [Test]
    public void DuringAMilitaryConflict_PutsAProvinceCharacterIntoPlayAsAnAttacker()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var source = new Card { Id = "charge", Type = CardType.Event, Controller = p1 };
        var provinceCharacter = new Card { Id = "province-character", Type = CardType.Character, Controller = p1, Location = "province" };

        var conflict = new Conflict { AttackingPlayer = p1, DefendingPlayer = p2, ConflictType = "military" };
        game.CurrentConflict = conflict;

        var action = LoadFirstAction();
        var context = new AbilityContext { Game = game, Player = p1, Source = source };
        var executor = new AbilityExecutor(new CostRegistry(), new GameActionRegistry());

        executor.Execute(action, context, chosenTarget: provinceCharacter);

        Assert.That(provinceCharacter.Location, Is.EqualTo("play area"));
        Assert.That(p1.PlayArea, Does.Contain(provinceCharacter));
        Assert.That(conflict.Attackers, Does.Contain(provinceCharacter));
    }
}
