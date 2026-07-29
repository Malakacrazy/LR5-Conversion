using System.Text.Json;
using L5R.Engine.Abilities;
using L5R.Engine.Dsl;
using L5R.Engine.State;

namespace L5R.Engine.Tests.Cards.PerCard;

public class IsawaAtsukoTests
{
    private static ActionDefinition LoadFirstAction()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Cards", "01-Core", "isawa-atsuko.json");
        using var document = JsonDocument.Parse(File.ReadAllText(path));
        return AbilityDefinitionParser.ParseActions(document.RootElement).Single();
    }

    [Test]
    public void DuringAVoidConflict_BuffsHerSideAndDebuffsTheOpponentsParticipants()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var atsuko = new Card { Id = "isawa-atsuko", Type = CardType.Character, Controller = p1, PrintedMilitarySkill = 3, PrintedPoliticalSkill = 3 };
        var ally = new Card { Id = "ally-1", Type = CardType.Character, Controller = p1, PrintedMilitarySkill = 2, PrintedPoliticalSkill = 2 };
        var enemy = new Card { Id = "enemy-1", Type = CardType.Character, Controller = p2, PrintedMilitarySkill = 2, PrintedPoliticalSkill = 2 };
        p1.PlayArea.Add(atsuko);
        p1.PlayArea.Add(ally);
        p2.PlayArea.Add(enemy);

        var conflict = new Conflict { AttackingPlayer = p1, DefendingPlayer = p2, Elements = new List<string> { "void" } };
        conflict.Attackers.Add(atsuko);
        conflict.Attackers.Add(ally);
        conflict.Defenders.Add(enemy);
        game.CurrentConflict = conflict;

        var action = LoadFirstAction();
        var context = new AbilityContext { Game = game, Player = p1, Source = atsuko };
        var executor = new AbilityExecutor(new CostRegistry(), new GameActionRegistry());

        executor.Execute(action, context);

        Assert.That(game.EffectiveMilitarySkill(atsuko), Is.EqualTo(4));
        Assert.That(game.EffectivePoliticalSkill(ally), Is.EqualTo(3));
        Assert.That(game.EffectiveMilitarySkill(enemy), Is.EqualTo(1));
        Assert.That(game.EffectivePoliticalSkill(enemy), Is.EqualTo(1));
    }

    [Test]
    public void DuringANonVoidConflict_ConditionIsNotMet()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var atsuko = new Card { Id = "isawa-atsuko", Type = CardType.Character, Controller = p1 };

        var conflict = new Conflict { AttackingPlayer = p1, DefendingPlayer = p2, ConflictType = "military" };
        game.CurrentConflict = conflict;

        var action = LoadFirstAction();
        var context = new AbilityContext { Game = game, Player = p1, Source = atsuko };
        var executor = new AbilityExecutor(new CostRegistry(), new GameActionRegistry());

        Assert.That(executor.IsConditionMet(action, context), Is.False);
    }
}
