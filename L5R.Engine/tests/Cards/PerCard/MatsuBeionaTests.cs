using System.Text.Json;
using L5R.Engine.Abilities;
using L5R.Engine.Dsl;
using L5R.Engine.State;

namespace L5R.Engine.Tests.Cards.PerCard;

public class MatsuBeionaTests
{
    private static TriggeredAbilityDefinition LoadFirstTriggeredAbility()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Cards", "01-Core", "matsu-beiona.json");
        using var document = JsonDocument.Parse(File.ReadAllText(path));
        return AbilityDefinitionParser.ParseTriggeredAbilities(document.RootElement).Single();
    }

    [Test]
    public void WhenEnteringPlay_WithThreeOtherBushi_GainsTwoFate()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Dynasty };
        var beiona = new Card { Id = "matsu-beiona", Type = CardType.Character, Controller = p1, Fate = 0 };
        p1.PlayArea.Add(beiona);
        p1.PlayArea.Add(new Card { Id = "bushi-1", Type = CardType.Character, Controller = p1, Traits = new[] { "bushi" } });
        p1.PlayArea.Add(new Card { Id = "bushi-2", Type = CardType.Character, Controller = p1, Traits = new[] { "bushi" } });
        p1.PlayArea.Add(new Card { Id = "bushi-3", Type = CardType.Character, Controller = p1, Traits = new[] { "bushi" } });

        var ability = LoadFirstTriggeredAbility();
        var context = new AbilityContext { Game = game, Player = p1, Source = beiona };
        var executor = new AbilityExecutor(new CostRegistry(), new GameActionRegistry());

        executor.ExecuteTriggered(ability, context, eventCard: beiona);

        Assert.That(beiona.Fate, Is.EqualTo(2));
    }

    [Test]
    public void DoesNotFire_WithFewerThanThreeOtherBushi()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Dynasty };
        var beiona = new Card { Id = "matsu-beiona", Type = CardType.Character, Controller = p1, Fate = 0 };
        p1.PlayArea.Add(beiona);
        p1.PlayArea.Add(new Card { Id = "bushi-1", Type = CardType.Character, Controller = p1, Traits = new[] { "bushi" } });

        var ability = LoadFirstTriggeredAbility();
        var context = new AbilityContext { Game = game, Player = p1, Source = beiona };
        var executor = new AbilityExecutor(new CostRegistry(), new GameActionRegistry());

        Assert.Throws<InvalidOperationException>(() => executor.ExecuteTriggered(ability, context, eventCard: beiona));
        Assert.That(beiona.Fate, Is.EqualTo(0));
    }
}
