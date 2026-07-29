using System.Text.Json;
using L5R.Engine.Abilities;
using L5R.Engine.Dsl;
using L5R.Engine.State;

namespace L5R.Engine.Tests.Cards.PerCard;

public class IsawaMoriSeidoTests
{
    private static ActionDefinition LoadFirstAction()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Cards", "01-Core", "isawa-mori-seido.json");
        using var document = JsonDocument.Parse(File.ReadAllText(path));
        return AbilityDefinitionParser.ParseActions(document.RootElement).Single();
    }

    [Test]
    public void BowingTheStrongholdGrantsPlus2GloryUntilEndOfPhase()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var stronghold = new Card { Id = "isawa-mori-seido", Type = CardType.Stronghold, Controller = p1 };
        var target = new Card { Id = "some-character", Type = CardType.Character, Controller = p2, PrintedGlory = 1 };
        p2.PlayArea.Add(target);

        var action = LoadFirstAction();
        var context = new AbilityContext { Game = game, Player = p1, Source = stronghold };
        var executor = new AbilityExecutor(new CostRegistry(), new GameActionRegistry());

        executor.Execute(action, context, chosenTarget: target);

        Assert.That(stronghold.Bowed, Is.True, "bowSelf cost was paid");
        Assert.That(game.EffectiveGlory(target), Is.EqualTo(3), "printed 1 + the lasting effect's 2");

        game.AdvancePhase();

        Assert.That(game.EffectiveGlory(target), Is.EqualTo(1), "untilEndOfPhase effect expired on the next phase transition");
    }

    [Test]
    public void CannotPayCost_WhenAlreadyBowed()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var stronghold = new Card { Id = "isawa-mori-seido", Type = CardType.Stronghold, Controller = p1, Bowed = true };
        var target = new Card { Id = "some-character", Type = CardType.Character, Controller = p2, PrintedGlory = 1 };
        p2.PlayArea.Add(target);

        var action = LoadFirstAction();
        var context = new AbilityContext { Game = game, Player = p1, Source = stronghold };
        var executor = new AbilityExecutor(new CostRegistry(), new GameActionRegistry());

        Assert.Throws<InvalidOperationException>(() => executor.Execute(action, context, chosenTarget: target));
    }

    [Test]
    public void Provisions_StartingHonorAndFateIncomeAndStrengthBonus()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        var stronghold = new Card { Id = "isawa-mori-seido", Type = CardType.Stronghold, Controller = p1, PrintedHonor = 11, PrintedFateIncome = 7, PrintedStrengthBonus = 2 };
        p1.Stronghold = stronghold;

        game.SetHonorFromStronghold(p1);

        Assert.That(p1.Honor, Is.EqualTo(11));
        Assert.That(game.FateIncomeFor(p1), Is.EqualTo(7));
        Assert.That(game.StrongholdStrengthBonusFor(p1), Is.EqualTo(2));
    }
}
