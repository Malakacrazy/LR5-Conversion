using System.Text.Json;
using L5R.Engine.Abilities;
using L5R.Engine.Dsl;
using L5R.Engine.State;

namespace L5R.Engine.Tests.Cards.PerCard;

public class LetGoTests
{
    private static ActionDefinition LoadFirstAction()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Cards", "01-Core", "let-go.json");
        using var document = JsonDocument.Parse(File.ReadAllText(path));
        return AbilityDefinitionParser.ParseActions(document.RootElement).Single();
    }

    [Test]
    public void DiscardsTheChosenAttachment()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var letGo = new Card { Id = "let-go", Type = CardType.Event, Controller = p1 };
        var attachment = new Card { Id = "some-attachment", Type = CardType.Attachment, Controller = p2 };
        p1.Discard.Add(letGo); // events resolve from wherever they're played; irrelevant to this test
        p2.PlayArea.Add(attachment);

        var action = LoadFirstAction();
        var context = new AbilityContext { Game = game, Player = p1, Source = letGo };
        var executor = new AbilityExecutor(new CostRegistry(), new GameActionRegistry());

        executor.Execute(action, context, chosenTarget: attachment);

        Assert.That(p2.PlayArea, Does.Not.Contain(attachment));
        Assert.That(p2.Discard, Does.Contain(attachment));
    }

    [Test]
    public void TargetsEitherPlayersAttachments_SinceControllerDefaultsToAny()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var letGo = new Card { Id = "let-go", Type = CardType.Event, Controller = p1 };
        var ownAttachment = new Card { Id = "own-attachment", Type = CardType.Attachment, Controller = p1 };
        var opponentAttachment = new Card { Id = "opponent-attachment", Type = CardType.Attachment, Controller = p2 };
        p1.PlayArea.Add(ownAttachment);
        p2.PlayArea.Add(opponentAttachment);

        var action = LoadFirstAction();
        var legalTargets = TargetResolver.ResolveLegalTargets(
            action.Target!,
            new AbilityContext { Game = game, Player = p1, Source = letGo });

        Assert.That(legalTargets, Does.Contain(ownAttachment));
        Assert.That(legalTargets, Does.Contain(opponentAttachment));
    }
}
