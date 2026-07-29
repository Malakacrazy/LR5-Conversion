using System.Text.Json;
using L5R.Engine.Abilities;
using L5R.Engine.Dsl;
using L5R.Engine.State;

namespace L5R.Engine.Tests.Cards.PerCard;

public class RallyToTheCauseTests
{
    private static TriggeredAbilityDefinition LoadFirstTriggeredAbility()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Cards", "01-Core", "rally-to-the-cause.json");
        using var document = JsonDocument.Parse(File.ReadAllText(path));
        return AbilityDefinitionParser.ParseTriggeredAbilities(document.RootElement).Single();
    }

    [Test]
    public void WhenRevealedDuringAConflict_SwitchesTheConflictType()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var province = new Card { Id = "rally-to-the-cause", Type = CardType.Province, Controller = p1 };
        var conflict = new Conflict { AttackingPlayer = p2, DefendingPlayer = p1, ConflictType = "political" };
        game.CurrentConflict = conflict;

        var ability = LoadFirstTriggeredAbility();
        var context = new AbilityContext { Game = game, Player = p1, Source = province };
        var executor = new AbilityExecutor(new CostRegistry(), new GameActionRegistry());

        executor.ExecuteTriggered(ability, context, eventCard: province);

        Assert.That(conflict.ConflictType, Is.EqualTo("military"));
    }
}
