using System.Text.Json;
using L5R.Engine.Abilities;
using L5R.Engine.Dsl;
using L5R.Engine.State;

namespace L5R.Engine.Tests.Cards.PerCard;

public class ShosuroActressTests
{
    private static ActionDefinition LoadFirstAction()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Cards", "01-Core", "shosuro-actress.json");
        using var document = JsonDocument.Parse(File.ReadAllText(path));
        return AbilityDefinitionParser.ParseActions(document.RootElement).Single();
    }

    [Test]
    public void SacrificingHerself_PutsACheapOpponentDiscardedCharacterIntoPlayOnHerSide()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var actress = new Card { Id = "shosuro-actress", Type = CardType.Character, Controller = p1 };
        var discardedEnemy = new Card { Id = "discarded-enemy", Type = CardType.Character, Controller = p2, PrintedCost = 3, Location = "conflict discard pile" };
        p1.PlayArea.Add(actress);
        p2.Discard.Add(discardedEnemy);

        var conflict = new Conflict { AttackingPlayer = p1, DefendingPlayer = p2 };
        game.CurrentConflict = conflict;

        var action = LoadFirstAction();
        var context = new AbilityContext { Game = game, Player = p1, Source = actress };
        var executor = new AbilityExecutor(new CostRegistry(), new GameActionRegistry());

        executor.Execute(action, context, chosenTarget: discardedEnemy);

        Assert.That(p1.Discard, Does.Contain(actress), "sacrificeSelf cost was paid");
        Assert.That(discardedEnemy.Location, Is.EqualTo("play area"));
        Assert.That(p2.PlayArea, Does.Contain(discardedEnemy), "the card's own controller (its owner) is unchanged");
        Assert.That(conflict.Attackers, Does.Contain(discardedEnemy), "joins the ability controller's side (attacker), not its own controller's side");
    }
}
