using System.Text.Json;
using L5R.Engine.Abilities;
using L5R.Engine.Dsl;
using L5R.Engine.Dsl.GameActions;
using L5R.Engine.State;

namespace L5R.Engine.Tests.Cards.PerCard;

public class BorderlandsDefenderTests
{
    private static IReadOnlyList<PersistentEffectDefinition> LoadPersistentEffects()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Cards", "01-Core", "borderlands-defender.json");
        using var document = JsonDocument.Parse(File.ReadAllText(path));
        return AbilityDefinitionParser.ParsePersistentEffects(document.RootElement);
    }

    private static (GameState Game, Card Defender) NewDefendingGame()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var defender = new Card { Id = "borderlands-defender", Type = CardType.Character, Controller = p1, PersistentEffects = LoadPersistentEffects() };
        p1.PlayArea.Add(defender);

        var conflict = new Conflict { AttackingPlayer = p2, DefendingPlayer = p1 };
        conflict.Defenders.Add(defender);
        game.CurrentConflict = conflict;

        return (game, defender);
    }

    [Test]
    public void WhileDefending_OpponentCardEffectsCannotSendItHomeOrBowIt()
    {
        var (game, defender) = NewDefendingGame();
        var opponentSource = new Card { Id = "opponent-card", Type = CardType.Event, Controller = game.Player2 };

        Assert.Throws<InvalidOperationException>(
            () => new SendHomeGameActionHandler().Execute(new AbilityContext { Game = game, Player = game.Player2, Source = opponentSource, Target = defender }, null));
        Assert.Throws<InvalidOperationException>(
            () => new BowGameActionHandler().Execute(new AbilityContext { Game = game, Player = game.Player2, Source = opponentSource, Target = defender }, null));
    }

    [Test]
    public void ItsControllersOwnCardEffectsCanStillSendItHomeOrBowIt()
    {
        var (game, defender) = NewDefendingGame();
        var ownSource = new Card { Id = "own-card", Type = CardType.Event, Controller = game.Player1 };

        new BowGameActionHandler().Execute(new AbilityContext { Game = game, Player = game.Player1, Source = ownSource, Target = defender }, null);

        Assert.That(defender.Bowed, Is.True);
    }

    [Test]
    public void WhileNotDefending_NotRestricted()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var defender = new Card { Id = "borderlands-defender", Type = CardType.Character, Controller = p1, PersistentEffects = LoadPersistentEffects() };
        p1.PlayArea.Add(defender);
        var opponentSource = new Card { Id = "opponent-card", Type = CardType.Event, Controller = p2 };

        Assert.That(game.IsRestrictedFrom(defender, "bow", opponentSource), Is.False, "not currently defending");
    }
}
