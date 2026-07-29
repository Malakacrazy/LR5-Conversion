using System.Text.Json;
using L5R.Engine.Abilities;
using L5R.Engine.Dsl;
using L5R.Engine.State;

namespace L5R.Engine.Tests.Cards.PerCard;

public class IsawaMasahiroTests
{
    private static ActionDefinition LoadFirstAction()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Cards", "01-Core", "isawa-masahiro.json");
        using var document = JsonDocument.Parse(File.ReadAllText(path));
        return AbilityDefinitionParser.ParseActions(document.RootElement).Single();
    }

    [Test]
    public void DuringAFireConflict_BowsToDiscardACheapParticipatingCharacter()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var masahiro = new Card { Id = "isawa-masahiro", Type = CardType.Character, Controller = p1 };
        var target = new Card { Id = "cheap-character", Type = CardType.Character, Controller = p2, PrintedCost = 2 };
        p1.PlayArea.Add(masahiro);
        p2.PlayArea.Add(target);

        var conflict = new Conflict { AttackingPlayer = p1, DefendingPlayer = p2, Elements = new List<string> { "fire" } };
        conflict.Defenders.Add(target);
        game.CurrentConflict = conflict;

        var action = LoadFirstAction();
        var context = new AbilityContext { Game = game, Player = p1, Source = masahiro };
        var executor = new AbilityExecutor(new CostRegistry(), new GameActionRegistry());

        executor.Execute(action, context, chosenTarget: target);

        Assert.That(masahiro.Bowed, Is.True, "bowSelf cost was paid");
        Assert.That(p2.Discard, Does.Contain(target));
    }

    [Test]
    public void DuringANonFireConflict_ConditionIsNotMet()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var masahiro = new Card { Id = "isawa-masahiro", Type = CardType.Character, Controller = p1 };

        var conflict = new Conflict { AttackingPlayer = p1, DefendingPlayer = p2, ConflictType = "military" };
        game.CurrentConflict = conflict;

        var action = LoadFirstAction();
        var context = new AbilityContext { Game = game, Player = p1, Source = masahiro };
        var executor = new AbilityExecutor(new CostRegistry(), new GameActionRegistry());

        Assert.That(executor.IsConditionMet(action, context), Is.False);
    }
}
