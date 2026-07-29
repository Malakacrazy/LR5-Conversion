using System.Text.Json;
using L5R.Engine.Abilities;
using L5R.Engine.Dsl;
using L5R.Engine.State;

namespace L5R.Engine.Tests.Cards.PerCard;

public class MirumotosFuryTests
{
    private static ActionDefinition LoadFirstAction()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Cards", "01-Core", "mirumoto-s-fury.json");
        using var document = JsonDocument.Parse(File.ReadAllText(path));
        return AbilityDefinitionParser.ParseActions(document.RootElement).Single();
    }

    private static (GameState Game, Card Attacker) NewGameWithAttacker(int glory, int facedownProvinces)
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        for (var i = 0; i < facedownProvinces; i++)
            p1.Provinces.Add(new Card { Id = $"province-{i}", Type = CardType.Holding, Controller = p1, Location = "province", Facedown = true });

        var attacker = new Card { Id = "attacker", Type = CardType.Character, Controller = p2, PrintedGlory = glory };
        p2.PlayArea.Add(attacker);

        var conflict = new Conflict { AttackingPlayer = p2, DefendingPlayer = p1 };
        conflict.Attackers.Add(attacker);
        game.CurrentConflict = conflict;

        return (game, attacker);
    }

    [Test]
    public void GloryAtOrBelowFacedownProvinceCount_BowsTheAttacker()
    {
        var (game, attacker) = NewGameWithAttacker(glory: 2, facedownProvinces: 2);
        var source = new Card { Id = "mirumoto-s-fury", Type = CardType.Event, Controller = game.Player1 };

        var action = LoadFirstAction();
        var context = new AbilityContext { Game = game, Player = game.Player1, Source = source };
        var executor = new AbilityExecutor(new CostRegistry(), new GameActionRegistry());

        executor.Execute(action, context, chosenTarget: attacker);

        Assert.That(attacker.Bowed, Is.True);
    }

    [Test]
    public void GloryAboveFacedownProvinceCount_IsNotALegalTarget()
    {
        var (game, attacker) = NewGameWithAttacker(glory: 3, facedownProvinces: 2);
        var source = new Card { Id = "mirumoto-s-fury", Type = CardType.Event, Controller = game.Player1 };

        var action = LoadFirstAction();
        var context = new AbilityContext { Game = game, Player = game.Player1, Source = source };
        var legalTargets = TargetResolver.ResolveLegalTargets(action.Target!, context);

        Assert.That(legalTargets, Does.Not.Contain(attacker));
    }

    [Test]
    public void ADefendingCharacter_IsNotALegalTargetRegardlessOfGlory()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        p1.Provinces.Add(new Card { Id = "province-0", Type = CardType.Holding, Controller = p1, Location = "province", Facedown = true });
        var defender = new Card { Id = "defender", Type = CardType.Character, Controller = p2, PrintedGlory = 0 };
        p2.PlayArea.Add(defender);

        var conflict = new Conflict { AttackingPlayer = p1, DefendingPlayer = p2 };
        conflict.Defenders.Add(defender);
        game.CurrentConflict = conflict;

        var source = new Card { Id = "mirumoto-s-fury", Type = CardType.Event, Controller = p1 };
        var action = LoadFirstAction();
        var context = new AbilityContext { Game = game, Player = p1, Source = source };
        var legalTargets = TargetResolver.ResolveLegalTargets(action.Target!, context);

        Assert.That(legalTargets, Does.Not.Contain(defender), "not attacking");
    }
}
