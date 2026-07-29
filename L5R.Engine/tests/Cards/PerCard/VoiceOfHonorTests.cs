using System.Text.Json;
using L5R.Engine.Abilities;
using L5R.Engine.Dsl;
using L5R.Engine.State;

namespace L5R.Engine.Tests.Cards.PerCard;

public class VoiceOfHonorTests
{
    private static TriggeredAbilityDefinition LoadFirstTriggeredAbility()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Cards", "01-Core", "voice-of-honor.json");
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
    public void WithMoreHonoredCharactersThanTheOpponent_CancelsAnEventBeforeItsEffectApplies()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2", Honor = 5 };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p2, CurrentPhase = Phase.Conflict };
        var honoredAlly = new Card { Id = "honored-ally", Type = CardType.Character, Controller = p1, IsHonored = true };
        p1.PlayArea.Add(honoredAlly);

        var assassination = new Card { Id = "assassination", Type = CardType.Event, Controller = p2 };
        var cheapCharacter = new Card { Id = "cheap-character", Type = CardType.Character, Controller = p1, PrintedCost = 2 };
        p1.PlayArea.Add(cheapCharacter);

        var executor = new AbilityExecutor(new CostRegistry(), new GameActionRegistry());
        var assassinationContext = new AbilityContext { Game = game, Player = p2, Source = assassination };
        var pending = executor.Prepare(LoadAssassinationAction(), assassinationContext, chosenTarget: cheapCharacter);

        Assert.That(p2.Honor, Is.EqualTo(2), "assassination's payHonor(3) cost was already paid by Prepare");

        var voiceOfHonor = new Card { Id = "voice-of-honor", Type = CardType.Event, Controller = p1 };
        var cancelContext = new AbilityContext { Game = game, Player = p1, Source = voiceOfHonor, InterruptedAbility = pending };
        executor.ExecuteTriggered(LoadFirstTriggeredAbility(), cancelContext, eventCard: assassination);

        executor.Resolve(pending);

        Assert.That(p1.Discard, Does.Not.Contain(cheapCharacter), "assassination's discardFromPlay effect never ran - it was cancelled before Resolve");
        Assert.That(p1.PlayArea, Does.Contain(cheapCharacter));
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

        Assert.That(p1.Discard, Does.Contain(cheapCharacter), "with no cancel, Resolve applies the effect exactly as Execute always has");
    }

    [Test]
    public void WithoutMoreHonoredCharacters_TheWhenConditionIsNotMet()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var voiceOfHonor = new Card { Id = "voice-of-honor", Type = CardType.Event, Controller = p1 };
        var assassination = new Card { Id = "assassination", Type = CardType.Event, Controller = p2 };

        var executor = new AbilityExecutor(new CostRegistry(), new GameActionRegistry());
        var context = new AbilityContext { Game = game, Player = p1, Source = voiceOfHonor };

        Assert.Throws<InvalidOperationException>(
            () => executor.ExecuteTriggered(LoadFirstTriggeredAbility(), context, eventCard: assassination));
    }
}
