using System.Text.Json;
using L5R.Engine.Abilities;
using L5R.Engine.Dsl;
using L5R.Engine.State;

namespace L5R.Engine.Tests.Cards.PerCard;

public class GoldenPlainsOutpostTests
{
    private static ActionDefinition LoadFirstAction()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Cards", "01-Core", "golden-plains-outpost.json");
        using var document = JsonDocument.Parse(File.ReadAllText(path));
        return AbilityDefinitionParser.ParseActions(document.RootElement).Single();
    }

    [Test]
    public void DuringAMilitaryConflict_MovesACavalryCharacterToTheConflict()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var outpost = new Card { Id = "golden-plains-outpost", Type = CardType.Stronghold, Controller = p1 };
        var cavalry = new Card { Id = "cavalry-1", Type = CardType.Character, Controller = p1, Traits = new List<string> { "cavalry" } };
        p1.PlayArea.Add(cavalry);

        var conflict = new Conflict { AttackingPlayer = p1, DefendingPlayer = p2, ConflictType = "military" };
        game.CurrentConflict = conflict;

        var action = LoadFirstAction();
        var context = new AbilityContext { Game = game, Player = p1, Source = outpost };
        var executor = new AbilityExecutor(new CostRegistry(), new GameActionRegistry());

        executor.Execute(action, context, chosenTarget: cavalry);

        Assert.That(outpost.Bowed, Is.True, "bowSelf cost was paid");
        Assert.That(conflict.Attackers, Does.Contain(cavalry));
    }

    [Test]
    public void DuringAPoliticalConflict_ConditionIsNotMet()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var outpost = new Card { Id = "golden-plains-outpost", Type = CardType.Stronghold, Controller = p1 };

        var conflict = new Conflict { AttackingPlayer = p1, DefendingPlayer = p2, ConflictType = "political" };
        game.CurrentConflict = conflict;

        var action = LoadFirstAction();
        var context = new AbilityContext { Game = game, Player = p1, Source = outpost };
        var executor = new AbilityExecutor(new CostRegistry(), new GameActionRegistry());

        Assert.That(executor.IsConditionMet(action, context), Is.False);
    }

    [Test]
    public void Provisions_StartingHonorAndFateIncomeAndZeroStrengthBonus()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        var outpost = new Card { Id = "golden-plains-outpost", Type = CardType.Stronghold, Controller = p1, PrintedHonor = 10, PrintedFateIncome = 7, PrintedStrengthBonus = 0 };
        p1.Stronghold = outpost;

        game.SetHonorFromStronghold(p1);

        Assert.That(p1.Honor, Is.EqualTo(10));
        Assert.That(game.FateIncomeFor(p1), Is.EqualTo(7));
        Assert.That(game.StrongholdStrengthBonusFor(p1), Is.EqualTo(0), "an explicit strengthBonus of 0");
    }
}
