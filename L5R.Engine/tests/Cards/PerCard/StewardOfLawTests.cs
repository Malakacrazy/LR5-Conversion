using System.Text.Json;
using L5R.Engine.Abilities;
using L5R.Engine.Dsl;
using L5R.Engine.Dsl.GameActions;
using L5R.Engine.State;

namespace L5R.Engine.Tests.Cards.PerCard;

public class StewardOfLawTests
{
    private static IReadOnlyList<PersistentEffectDefinition> LoadPersistentEffects()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Cards", "01-Core", "steward-of-law.json");
        using var document = JsonDocument.Parse(File.ReadAllText(path));
        return AbilityDefinitionParser.ParsePersistentEffects(document.RootElement);
    }

    [Test]
    public void WhileParticipating_NoCharacterCanReceiveADishonorToken()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var steward = new Card
        {
            Id = "steward-of-law", Type = CardType.Character, Controller = p1, PersistentEffects = LoadPersistentEffects()
        };
        var enemy = new Card { Id = "enemy-character", Type = CardType.Character, Controller = p2 };
        p1.PlayArea.Add(steward);
        p2.PlayArea.Add(enemy);

        var conflict = new Conflict { AttackingPlayer = p1, DefendingPlayer = p2 };
        conflict.Attackers.Add(steward);
        conflict.Defenders.Add(enemy);
        game.CurrentConflict = conflict;

        var context = new AbilityContext { Game = game, Player = p1, Source = steward, Target = enemy };
        var handler = new DishonorGameActionHandler();

        Assert.Throws<InvalidOperationException>(() => handler.Execute(context, parameters: null));
        Assert.That(enemy.IsDishonored, Is.False);
    }

    [Test]
    public void WhileNotParticipating_DishonorWorksNormally()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var steward = new Card
        {
            Id = "steward-of-law", Type = CardType.Character, Controller = p1, PersistentEffects = LoadPersistentEffects()
        };
        var enemy = new Card { Id = "enemy-character", Type = CardType.Character, Controller = p2 };
        p1.PlayArea.Add(steward);
        p2.PlayArea.Add(enemy);

        var context = new AbilityContext { Game = game, Player = p1, Source = steward, Target = enemy };
        new DishonorGameActionHandler().Execute(context, parameters: null);

        Assert.That(enemy.IsDishonored, Is.True);
    }
}
