using System.Text.Json;
using L5R.Engine.Abilities;
using L5R.Engine.Dsl;
using L5R.Engine.State;

namespace L5R.Engine.Tests.Cards.PerCard;

public class AgainstTheWavesTests
{
    private static ActionDefinition LoadFirstAction()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Cards", "01-Core", "against-the-waves.json");
        using var document = JsonDocument.Parse(File.ReadAllText(path));
        return AbilityDefinitionParser.ParseActions(document.RootElement).Single();
    }

    private static GameState NewGame(out Player p1, out Player p2)
    {
        p1 = new Player { Name = "Player1" };
        p2 = new Player { Name = "Player2" };
        return new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
    }

    [Test]
    public void OnlyTargetsShugenjaYouControl()
    {
        var game = NewGame(out var p1, out var p2);
        var source = new Card { Id = "against-the-waves", Type = CardType.Event, Controller = p1 };
        var ownShugenja = new Card { Id = "own-shugenja", Type = CardType.Character, Controller = p1, Traits = new[] { "shugenja" } };
        var opponentShugenja = new Card { Id = "opponent-shugenja", Type = CardType.Character, Controller = p2, Traits = new[] { "shugenja" } };
        var ownBushi = new Card { Id = "own-bushi", Type = CardType.Character, Controller = p1, Traits = new[] { "bushi" } };
        p1.PlayArea.Add(ownShugenja);
        p2.PlayArea.Add(opponentShugenja);
        p1.PlayArea.Add(ownBushi);

        var action = LoadFirstAction();
        var legalTargets = TargetResolver.ResolveLegalTargets(
            action.Target!,
            new AbilityContext { Game = game, Player = p1, Source = source });

        Assert.That(legalTargets, Is.EquivalentTo(new[] { ownShugenja }));
    }

    [Test]
    public void BowsAnUnbowedShugenja()
    {
        var game = NewGame(out var p1, out _);
        var source = new Card { Id = "against-the-waves", Type = CardType.Event, Controller = p1 };
        var target = new Card { Id = "own-shugenja", Type = CardType.Character, Controller = p1, Traits = new[] { "shugenja" }, Bowed = false };
        p1.PlayArea.Add(target);

        var action = LoadFirstAction();
        var context = new AbilityContext { Game = game, Player = p1, Source = source };
        var executor = new AbilityExecutor(new CostRegistry(), new GameActionRegistry());

        executor.Execute(action, context, chosenTarget: target);

        Assert.That(target.Bowed, Is.True, "only bow can affect an unbowed character - ready is a no-op here");
    }

    [Test]
    public void ReadiesABowedShugenja()
    {
        var game = NewGame(out var p1, out _);
        var source = new Card { Id = "against-the-waves", Type = CardType.Event, Controller = p1 };
        var target = new Card { Id = "own-shugenja", Type = CardType.Character, Controller = p1, Traits = new[] { "shugenja" }, Bowed = true };
        p1.PlayArea.Add(target);

        var action = LoadFirstAction();
        var context = new AbilityContext { Game = game, Player = p1, Source = source };
        var executor = new AbilityExecutor(new CostRegistry(), new GameActionRegistry());

        executor.Execute(action, context, chosenTarget: target);

        Assert.That(target.Bowed, Is.False, "only ready can affect a bowed character - bow is a no-op here");
    }
}
