using System.Text.Json;
using L5R.Engine.Abilities;
using L5R.Engine.Dsl;
using L5R.Engine.State;

namespace L5R.Engine.Tests.Cards.PerCard;

public class GraspOfEarthTests
{
    private static ActionDefinition LoadFirstAction()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Cards", "01-Core", "grasp-of-earth.json");
        using var document = JsonDocument.Parse(File.ReadAllText(path));
        return AbilityDefinitionParser.ParseActions(document.RootElement).Single();
    }

    [Test]
    public void DuringConflict_StopsTheOpponentBringingCharactersToTheConflictOrHand()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var graspOfEarth = new Card { Id = "grasp-of-earth", Type = CardType.Attachment, Controller = p1 };
        var enemyInPlay1 = new Card { Id = "enemy-in-play-1", Type = CardType.Character, Controller = p2 };
        var enemyInPlay2 = new Card { Id = "enemy-in-play-2", Type = CardType.Character, Controller = p2 };
        p1.PlayArea.Add(graspOfEarth);
        p2.PlayArea.Add(enemyInPlay1);
        p2.PlayArea.Add(enemyInPlay2);

        var conflict = new Conflict { AttackingPlayer = p1, DefendingPlayer = p2 };
        game.CurrentConflict = conflict;

        var action = LoadFirstAction();
        var context = new AbilityContext { Game = game, Player = p1, Source = graspOfEarth };
        var executor = new AbilityExecutor(new CostRegistry(), new GameActionRegistry());

        executor.Execute(action, context);

        Assert.That(graspOfEarth.Bowed, Is.True, "bowSelf cost was paid");
        Assert.That(game.IsRestrictedFrom(enemyInPlay1, "moveToConflict"), Is.True);
        Assert.That(game.IsRestrictedFrom(enemyInPlay2, "moveToConflict"), Is.True);

        var handCharacter = new Card { Id = "enemy-hand-character", Type = CardType.Character, Controller = p2 };
        var handEvent = new Card { Id = "enemy-hand-event", Type = CardType.Event, Controller = p2 };
        Assert.That(game.IsPlayerRestrictedFrom(p2, "playFromHand", handCharacter), Is.True);
        Assert.That(game.IsPlayerRestrictedFrom(p2, "playFromHand", handEvent), Is.False, "restricts: characters only covers character cards");
        Assert.That(game.IsPlayerRestrictedFrom(p1, "playFromHand", handCharacter), Is.False, "targets the opponent, not grasp-of-earth's own controller");
    }
}
