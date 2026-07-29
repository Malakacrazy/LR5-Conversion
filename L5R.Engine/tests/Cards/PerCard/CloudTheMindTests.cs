using System.Text.Json;
using L5R.Engine.Abilities;
using L5R.Engine.Cards.Scripts;
using L5R.Engine.Dsl;
using L5R.Engine.Dsl.GameActions;
using L5R.Engine.State;

namespace L5R.Engine.Tests.Cards.PerCard;

public class CloudTheMindTests
{
    private static IReadOnlyList<WhileAttachedDefinition> LoadWhileAttached()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Cards", "01-Core", "cloud-the-mind.json");
        using var document = JsonDocument.Parse(File.ReadAllText(path));
        return AbilityDefinitionParser.ParseWhileAttached(document.RootElement);
    }

    // The generic whileAttached "blank" effect is tested here against a synthetic minimal
    // ability rather than a real card's - blanking removes *whatever* text the host has,
    // so the specific ability doesn't matter, only that Prepare refuses to run any of them.
    // Its play-eligibility restriction (below) is now implemented via ICardScript.CanPlay.

    private static ActionDefinition SyntheticGainFateAction() =>
        new("Test ability", Array.Empty<CostDefinition>(), null, new[] { new GameActionDefinition("gainFate", null) }, null, null);

    [Test]
    public void WhileAttached_TheHostCannotUseItsOwnActions()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        var host = new Card { Id = "some-shugenja", Type = CardType.Character, Controller = p1 };
        var cloudTheMind = new Card
        {
            Id = "cloud-the-mind", Type = CardType.Attachment, Controller = p1,
            AttachedTo = host, WhileAttachedEffects = LoadWhileAttached()
        };
        p1.PlayArea.Add(host);
        p1.PlayArea.Add(cloudTheMind);

        var context = new AbilityContext { Game = game, Player = p1, Source = host };
        var executor = new AbilityExecutor(new CostRegistry(), new GameActionRegistry());

        Assert.Throws<InvalidOperationException>(() => executor.Execute(SyntheticGainFateAction(), context));
        Assert.That(p1.Fate, Is.EqualTo(0));
    }

    [Test]
    public void WithoutIt_TheHostsActionsWorkNormally()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        var host = new Card { Id = "some-shugenja", Type = CardType.Character, Controller = p1 };
        p1.PlayArea.Add(host);

        var context = new AbilityContext { Game = game, Player = p1, Source = host };
        var executor = new AbilityExecutor(new CostRegistry(), new GameActionRegistry());

        executor.Execute(SyntheticGainFateAction(), context);

        Assert.That(p1.Fate, Is.EqualTo(1));
    }

    [Test]
    public void WithNoShugenjaInPlay_CannotBePlayed()
    {
        var p1 = new Player { Name = "Player1", Fate = 5 };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        var host = new Card { Id = "non-shugenja", Type = CardType.Character, Controller = p1 };
        var cloudTheMind = new Card
        {
            Id = "cloud-the-mind", Type = CardType.Attachment, Controller = p1,
            PrintedCost = 1, PlayScript = new CloudTheMindPlayRestriction()
        };
        p1.Hand.Add(cloudTheMind);
        p1.PlayArea.Add(host);

        var context = new AbilityContext { Game = game, Player = p1, Source = cloudTheMind, Target = cloudTheMind, PlayAttachTarget = host };

        Assert.Throws<InvalidOperationException>(() => new PlayCardGameActionHandler().Execute(context, null));
    }

    [Test]
    public void WithAShugenjaInPlay_CanBePlayed()
    {
        var p1 = new Player { Name = "Player1", Fate = 5 };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        var shugenja = new Card { Id = "some-shugenja", Type = CardType.Character, Controller = p1, Traits = new[] { "shugenja" } };
        var cloudTheMind = new Card
        {
            Id = "cloud-the-mind", Type = CardType.Attachment, Controller = p1,
            PrintedCost = 1, PlayScript = new CloudTheMindPlayRestriction()
        };
        p1.Hand.Add(cloudTheMind);
        p1.PlayArea.Add(shugenja);

        var context = new AbilityContext { Game = game, Player = p1, Source = cloudTheMind, Target = cloudTheMind, PlayAttachTarget = shugenja };

        new PlayCardGameActionHandler().Execute(context, null);

        Assert.That(p1.PlayArea, Does.Contain(cloudTheMind));
    }
}
