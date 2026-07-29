using System.Text.Json;
using L5R.Engine.Abilities;
using L5R.Engine.Dsl;
using L5R.Engine.State;

namespace L5R.Engine.Tests.Cards.PerCard;

public class UtakuYuminoTests
{
    private static ActionDefinition LoadFirstAction()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Cards", "01-Core", "utaku-yumino.json");
        using var document = JsonDocument.Parse(File.ReadAllText(path));
        return AbilityDefinitionParser.ParseActions(document.RootElement).Single();
    }

    [Test]
    public void DuringConflict_DiscardingACardFromHandGivesHerselfPlusTwoPlusTwo()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var yumino = new Card { Id = "utaku-yumino", Type = CardType.Character, Controller = p1, PrintedMilitarySkill = 2, PrintedPoliticalSkill = 2 };
        var handCard = new Card { Id = "hand-card", Type = CardType.Character, Controller = p1, Location = "hand" };
        p1.PlayArea.Add(yumino);
        p1.Hand.Add(handCard);

        var action = LoadFirstAction();
        var context = new AbilityContext { Game = game, Player = p1, Source = yumino };
        var executor = new AbilityExecutor(new CostRegistry(), new GameActionRegistry());

        executor.Execute(action, context, chosenCostTarget: handCard);

        Assert.That(p1.Discard, Does.Contain(handCard), "discardCard cost was paid");
        Assert.That(game.EffectiveMilitarySkill(yumino), Is.EqualTo(4));
        Assert.That(game.EffectivePoliticalSkill(yumino), Is.EqualTo(4));
    }
}
