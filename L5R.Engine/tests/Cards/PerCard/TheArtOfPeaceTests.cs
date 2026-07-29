using System.Text.Json;
using L5R.Engine.Abilities;
using L5R.Engine.Dsl;
using L5R.Engine.State;

namespace L5R.Engine.Tests.Cards.PerCard;

public class TheArtOfPeaceTests
{
    private static TriggeredAbilityDefinition LoadFirstTriggeredAbility()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Cards", "01-Core", "the-art-of-peace.json");
        using var document = JsonDocument.Parse(File.ReadAllText(path));
        return AbilityDefinitionParser.ParseTriggeredAbilities(document.RootElement).Single();
    }

    [Test]
    public void WhenThisProvinceBreaks_HonorsAllDefendersAndDishonorsAllAttackers()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var province = new Card { Id = "the-art-of-peace", Type = CardType.Province, Controller = p2 };
        var attacker1 = new Card { Id = "attacker-1", Type = CardType.Character, Controller = p1 };
        var attacker2 = new Card { Id = "attacker-2", Type = CardType.Character, Controller = p1 };
        var defender = new Card { Id = "defender-1", Type = CardType.Character, Controller = p2 };
        p1.PlayArea.Add(attacker1);
        p1.PlayArea.Add(attacker2);
        p2.PlayArea.Add(defender);

        var conflict = new Conflict { AttackingPlayer = p1, DefendingPlayer = p2 };
        conflict.Attackers.Add(attacker1);
        conflict.Attackers.Add(attacker2);
        conflict.Defenders.Add(defender);
        game.CurrentConflict = conflict;

        var ability = LoadFirstTriggeredAbility();
        var context = new AbilityContext { Game = game, Player = p2, Source = province };
        var executor = new AbilityExecutor(new CostRegistry(), new GameActionRegistry());

        executor.ExecuteTriggered(ability, context, eventCard: province);

        Assert.That(attacker1.IsDishonored, Is.True);
        Assert.That(attacker2.IsDishonored, Is.True);
        Assert.That(defender.IsHonored, Is.True);
        Assert.That(attacker1.IsHonored, Is.False);
        Assert.That(defender.IsDishonored, Is.False);
    }
}
