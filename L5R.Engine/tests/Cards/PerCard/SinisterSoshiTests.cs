using System.Text.Json;
using L5R.Engine.Abilities;
using L5R.Engine.Dsl;
using L5R.Engine.State;

namespace L5R.Engine.Tests.Cards.PerCard;

public class SinisterSoshiTests
{
    private static ActionDefinition LoadFirstAction()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Cards", "01-Core", "sinister-soshi.json");
        using var document = JsonDocument.Parse(File.ReadAllText(path));
        return AbilityDefinitionParser.ParseActions(document.RootElement).Single();
    }

    [Test]
    public void DuringConflict_GivesAParticipatingCharacterMinusTwoMinusTwo()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var soshi = new Card { Id = "sinister-soshi", Type = CardType.Character, Controller = p1 };
        var target = new Card { Id = "enemy-attacker", Type = CardType.Character, Controller = p2, PrintedMilitarySkill = 3, PrintedPoliticalSkill = 3 };
        p1.PlayArea.Add(soshi);
        p2.PlayArea.Add(target);

        var conflict = new Conflict { AttackingPlayer = p2, DefendingPlayer = p1 };
        conflict.Attackers.Add(target);
        game.CurrentConflict = conflict;

        var action = LoadFirstAction();
        var context = new AbilityContext { Game = game, Player = p1, Source = soshi };
        var executor = new AbilityExecutor(new CostRegistry(), new GameActionRegistry());

        executor.Execute(action, context, chosenTarget: target);

        Assert.That(game.EffectiveMilitarySkill(target), Is.EqualTo(1));
        Assert.That(game.EffectivePoliticalSkill(target), Is.EqualTo(1));
    }
}
