using System.Text.Json;
using L5R.Engine.Abilities;
using L5R.Engine.Dsl;
using L5R.Engine.State;

namespace L5R.Engine.Tests.Cards.PerCard;

public class ShiroNishiyamaTests
{
    private static ActionDefinition LoadFirstAction()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Cards", "01-Core", "shiro-nishiyama.json");
        using var document = JsonDocument.Parse(File.ReadAllText(path));
        return AbilityDefinitionParser.ParseActions(document.RootElement).Single();
    }

    [Test]
    public void DuringConflict_GivesEveryDefenderPlusOnePlusOne()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var stronghold = new Card { Id = "shiro-nishiyama", Type = CardType.Stronghold, Controller = p1 };
        var defender = new Card { Id = "defender-1", Type = CardType.Character, Controller = p1, PrintedMilitarySkill = 2, PrintedPoliticalSkill = 2 };
        var attacker = new Card { Id = "attacker-1", Type = CardType.Character, Controller = p2, PrintedMilitarySkill = 2, PrintedPoliticalSkill = 2 };
        p1.PlayArea.Add(defender);
        p2.PlayArea.Add(attacker);

        var conflict = new Conflict { AttackingPlayer = p2, DefendingPlayer = p1 };
        conflict.Attackers.Add(attacker);
        conflict.Defenders.Add(defender);
        game.CurrentConflict = conflict;

        var action = LoadFirstAction();
        var context = new AbilityContext { Game = game, Player = p1, Source = stronghold };
        var executor = new AbilityExecutor(new CostRegistry(), new GameActionRegistry());

        executor.Execute(action, context);

        Assert.That(stronghold.Bowed, Is.True, "bowSelf cost was paid");
        Assert.That(game.EffectiveMilitarySkill(defender), Is.EqualTo(3));
        Assert.That(game.EffectivePoliticalSkill(defender), Is.EqualTo(3));
        Assert.That(game.EffectiveMilitarySkill(attacker), Is.EqualTo(2), "the attacker is not a defender");
    }
}
