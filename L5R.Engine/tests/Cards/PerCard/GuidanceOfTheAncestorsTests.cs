using System.Text.Json;
using L5R.Engine.Abilities;
using L5R.Engine.Dsl;
using L5R.Engine.State;

namespace L5R.Engine.Tests.Cards.PerCard;

public class GuidanceOfTheAncestorsTests
{
    private static ActionDefinition LoadFirstAction()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Cards", "01-Core", "guidance-of-the-ancestors.json");
        using var document = JsonDocument.Parse(File.ReadAllText(path));
        return AbilityDefinitionParser.ParseActions(document.RootElement).Single();
    }

    [Test]
    public void PlayingFromTheConflictDiscardPile_PaysCostAndAttachesToTheChosenCharacter()
    {
        var p1 = new Player { Name = "Player1", Fate = 1 };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };

        var guidance = new Card { Id = "guidance-of-the-ancestors", Type = CardType.Attachment, Controller = p1, PrintedCost = 1, Location = "conflict discard pile" };
        p1.Discard.Add(guidance);

        var berserker = new Card { Id = "matsu-berserker", Type = CardType.Character, Controller = p1 };
        p1.PlayArea.Add(berserker);

        var action = LoadFirstAction();
        var context = new AbilityContext { Game = game, Player = p1, Source = guidance, PlayAttachTarget = berserker };
        var executor = new AbilityExecutor(new CostRegistry(), new GameActionRegistry());

        executor.Execute(action, context);

        Assert.That(p1.Fate, Is.EqualTo(0), "printed cost (1 fate) was paid automatically");
        Assert.That(guidance.Location, Is.EqualTo("play area"));
        Assert.That(p1.PlayArea, Does.Contain(guidance));
        Assert.That(guidance.AttachedTo, Is.EqualTo(berserker));
    }

    [Test]
    public void WithoutEnoughFate_ThrowsAndDoesNotMoveTheCard()
    {
        var p1 = new Player { Name = "Player1", Fate = 0 };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };

        var guidance = new Card { Id = "guidance-of-the-ancestors", Type = CardType.Attachment, Controller = p1, PrintedCost = 1, Location = "conflict discard pile" };
        p1.Discard.Add(guidance);

        var berserker = new Card { Id = "matsu-berserker", Type = CardType.Character, Controller = p1 };
        p1.PlayArea.Add(berserker);

        var action = LoadFirstAction();
        var context = new AbilityContext { Game = game, Player = p1, Source = guidance, PlayAttachTarget = berserker };
        var executor = new AbilityExecutor(new CostRegistry(), new GameActionRegistry());

        Assert.Throws<InvalidOperationException>(() => executor.Execute(action, context));
        Assert.That(guidance.Location, Is.EqualTo("conflict discard pile"), "the card was never actually played");
    }

    [Test]
    public void WhenThePlayerIsRestrictedFromPlayingIt_Throws()
    {
        var p1 = new Player { Name = "Player1", Fate = 1 };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };

        var guidance = new Card { Id = "guidance-of-the-ancestors", Type = CardType.Attachment, Controller = p1, PrintedCost = 1, Location = "conflict discard pile" };
        p1.Discard.Add(guidance);

        var berserker = new Card { Id = "matsu-berserker", Type = CardType.Character, Controller = p1 };
        p1.PlayArea.Add(berserker);

        var restrictor = new Card { Id = "restrictor", Type = CardType.Character, Controller = p2 };
        game.PlayerRestrictions.Add(new PlayerRestriction { Target = p1, Action = "play", Source = restrictor, Duration = "untilEndOfConflict" });

        var action = LoadFirstAction();
        var context = new AbilityContext { Game = game, Player = p1, Source = guidance, PlayAttachTarget = berserker };
        var executor = new AbilityExecutor(new CostRegistry(), new GameActionRegistry());

        Assert.Throws<InvalidOperationException>(() => executor.Execute(action, context));
        Assert.That(p1.Fate, Is.EqualTo(1), "the cost was never actually paid");
    }
}
