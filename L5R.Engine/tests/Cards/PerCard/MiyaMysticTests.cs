using System.Text.Json;
using L5R.Engine.Abilities;
using L5R.Engine.Dsl;
using L5R.Engine.State;

namespace L5R.Engine.Tests.Cards.PerCard;

public class MiyaMysticTests
{
    private static ActionDefinition LoadFirstAction()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Cards", "01-Core", "miya-mystic.json");
        using var document = JsonDocument.Parse(File.ReadAllText(path));
        return AbilityDefinitionParser.ParseActions(document.RootElement).Single();
    }

    [Test]
    public void DuringConflictPhase_SacrificingDiscardsTheChosenAttachment()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var mystic = new Card { Id = "miya-mystic", Type = CardType.Character, Controller = p1 };
        var attachment = new Card { Id = "some-attachment", Type = CardType.Attachment, Controller = p2 };
        p1.PlayArea.Add(mystic);
        p2.PlayArea.Add(attachment);

        var action = LoadFirstAction();
        var context = new AbilityContext { Game = game, Player = p1, Source = mystic };
        var executor = new AbilityExecutor(new CostRegistry(), new GameActionRegistry());

        executor.Execute(action, context, chosenTarget: attachment);

        Assert.That(p1.PlayArea, Does.Not.Contain(mystic), "sacrificeSelf discards the mystic itself as its cost");
        Assert.That(p2.Discard, Does.Contain(attachment));
    }

    [Test]
    public void OutsideConflictPhase_CannotBeUsed()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Dynasty };
        var mystic = new Card { Id = "miya-mystic", Type = CardType.Character, Controller = p1 };
        var attachment = new Card { Id = "some-attachment", Type = CardType.Attachment, Controller = p2 };
        p1.PlayArea.Add(mystic);
        p2.PlayArea.Add(attachment);

        var action = LoadFirstAction();
        var context = new AbilityContext { Game = game, Player = p1, Source = mystic };
        var executor = new AbilityExecutor(new CostRegistry(), new GameActionRegistry());

        // ringteki CardAction.js: this.phase !== 'any' && this.phase !== game.currentPhase
        // gates the whole action, checked before any cost is paid.
        Assert.Throws<InvalidOperationException>(() => executor.Execute(action, context, chosenTarget: attachment));
        Assert.That(p1.PlayArea, Does.Contain(mystic), "action was rejected, so no cost was paid");
    }
}
