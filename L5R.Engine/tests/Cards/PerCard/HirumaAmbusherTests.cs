using System.Text.Json;
using L5R.Engine.Abilities;
using L5R.Engine.Dsl;
using L5R.Engine.State;

namespace L5R.Engine.Tests.Cards.PerCard;

public class HirumaAmbusherTests
{
    private static TriggeredAbilityDefinition LoadFirstTriggeredAbility()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Cards", "01-Core", "hiruma-ambusher.json");
        using var document = JsonDocument.Parse(File.ReadAllText(path));
        return AbilityDefinitionParser.ParseTriggeredAbilities(document.RootElement).Single();
    }

    [Test]
    public void WhenItEntersPlayAsADefender_DisablesATargetCharactersTriggeredAbilities()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var ambusher = new Card { Id = "hiruma-ambusher", Type = CardType.Character, Controller = p1 };
        var target = new Card { Id = "enemy-attacker", Type = CardType.Character, Controller = p2 };
        p1.PlayArea.Add(ambusher);
        p2.PlayArea.Add(target);

        var conflict = new Conflict { AttackingPlayer = p2, DefendingPlayer = p1 };
        conflict.Defenders.Add(ambusher);
        conflict.Attackers.Add(target);
        game.CurrentConflict = conflict;

        var ability = LoadFirstTriggeredAbility();
        var context = new AbilityContext { Game = game, Player = p1, Source = ambusher };
        var executor = new AbilityExecutor(new CostRegistry(), new GameActionRegistry());

        executor.ExecuteTriggered(ability, context, eventCard: ambusher, chosenTarget: target);

        Assert.That(game.IsRestrictedFrom(target, "triggerAbilities"), Is.True);

        // Prove the restriction actually blocks the target from triggering abilities
        // afterward, not just that the marker was recorded.
        var laterContext = new AbilityContext { Game = game, Player = p2, Source = target };
        Assert.Throws<InvalidOperationException>(
            () => executor.ExecuteTriggered(ability, laterContext, eventCard: target, chosenTarget: ambusher));
    }

    [Test]
    public void WhenItEntersPlayAsAnAttacker_ConditionIsNotMet()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var ambusher = new Card { Id = "hiruma-ambusher", Type = CardType.Character, Controller = p1 };
        p1.PlayArea.Add(ambusher);

        var conflict = new Conflict { AttackingPlayer = p1, DefendingPlayer = p2 };
        conflict.Attackers.Add(ambusher);
        game.CurrentConflict = conflict;

        var ability = LoadFirstTriggeredAbility();
        var context = new AbilityContext { Game = game, Player = p1, Source = ambusher };

        Assert.Throws<InvalidOperationException>(
            () => new AbilityExecutor(new CostRegistry(), new GameActionRegistry())
                .ExecuteTriggered(ability, context, eventCard: ambusher, chosenTarget: ambusher));
    }
}
