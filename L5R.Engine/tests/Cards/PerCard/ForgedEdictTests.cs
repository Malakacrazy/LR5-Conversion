using System.Text.Json;
using L5R.Engine.Abilities;
using L5R.Engine.Dsl;
using L5R.Engine.State;

namespace L5R.Engine.Tests.Cards.PerCard;

public class ForgedEdictTests
{
    private static TriggeredAbilityDefinition LoadFirstTriggeredAbility()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Cards", "01-Core", "forged-edict.json");
        using var document = JsonDocument.Parse(File.ReadAllText(path));
        return AbilityDefinitionParser.ParseTriggeredAbilities(document.RootElement).Single();
    }

    private static ActionDefinition LoadAssassinationAction()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Cards", "01-Core", "assassination.json");
        using var document = JsonDocument.Parse(File.ReadAllText(path));
        return AbilityDefinitionParser.ParseActions(document.RootElement).Single();
    }

    [Test]
    public void DishonoringACourtier_CancelsAnEventBeforeItsEffectApplies()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2", Honor = 5 };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p2, CurrentPhase = Phase.Conflict };
        var courtier = new Card { Id = "courtier-ally", Type = CardType.Character, Controller = p1, Traits = new List<string> { "courtier" } };
        p1.PlayArea.Add(courtier);

        var assassination = new Card { Id = "assassination", Type = CardType.Event, Controller = p2 };
        var cheapCharacter = new Card { Id = "cheap-character", Type = CardType.Character, Controller = p1, PrintedCost = 2 };
        p1.PlayArea.Add(cheapCharacter);

        var executor = new AbilityExecutor(new CostRegistry(), new GameActionRegistry());
        var assassinationContext = new AbilityContext { Game = game, Player = p2, Source = assassination };
        var pending = executor.Prepare(LoadAssassinationAction(), assassinationContext, chosenTarget: cheapCharacter);

        var forgedEdict = new Card { Id = "forged-edict", Type = CardType.Event, Controller = p1 };
        var cancelContext = new AbilityContext { Game = game, Player = p1, Source = forgedEdict, InterruptedAbility = pending };
        executor.ExecuteTriggered(LoadFirstTriggeredAbility(), cancelContext, eventCard: assassination, chosenCostTarget: courtier);

        executor.Resolve(pending);

        Assert.That(courtier.IsDishonored, Is.True, "dishonor cost was paid");
        Assert.That(p1.Discard, Does.Not.Contain(cheapCharacter), "assassination's effect never ran - it was cancelled before Resolve");
    }

    [Test]
    public void WithNoCourtierToDishonor_CostCannotBePaid()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2", Honor = 5 };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p2, CurrentPhase = Phase.Conflict };
        var nonCourtier = new Card { Id = "non-courtier-ally", Type = CardType.Character, Controller = p1 };
        p1.PlayArea.Add(nonCourtier);

        var assassination = new Card { Id = "assassination", Type = CardType.Event, Controller = p2 };
        var forgedEdict = new Card { Id = "forged-edict", Type = CardType.Event, Controller = p1 };

        var executor = new AbilityExecutor(new CostRegistry(), new GameActionRegistry());
        var context = new AbilityContext { Game = game, Player = p1, Source = forgedEdict };

        Assert.Throws<InvalidOperationException>(
            () => executor.ExecuteTriggered(LoadFirstTriggeredAbility(), context, eventCard: assassination, chosenCostTarget: nonCourtier));
    }

    [Test]
    public void WithoutCancelling_TheOriginalAbilityResolvesNormally()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2", Honor = 5 };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p2, CurrentPhase = Phase.Conflict };
        var assassination = new Card { Id = "assassination", Type = CardType.Event, Controller = p2 };
        var cheapCharacter = new Card { Id = "cheap-character", Type = CardType.Character, Controller = p1, PrintedCost = 2 };
        p1.PlayArea.Add(cheapCharacter);

        var executor = new AbilityExecutor(new CostRegistry(), new GameActionRegistry());
        var assassinationContext = new AbilityContext { Game = game, Player = p2, Source = assassination };
        var pending = executor.Prepare(LoadAssassinationAction(), assassinationContext, chosenTarget: cheapCharacter);

        executor.Resolve(pending);

        Assert.That(p1.Discard, Does.Contain(cheapCharacter));
    }
}
