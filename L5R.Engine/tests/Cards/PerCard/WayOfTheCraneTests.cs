using System.Text.Json;
using L5R.Engine.Abilities;
using L5R.Engine.Dsl;
using L5R.Engine.State;

namespace L5R.Engine.Tests.Cards.PerCard;

public class WayOfTheCraneTests
{
    private static ActionDefinition LoadFirstAction()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Cards", "01-Core", "way-of-the-crane.json");
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
    public void OnlyTargetsCraneCharactersYouControl()
    {
        var game = NewGame(out var p1, out var p2);
        var wayOfTheCrane = new Card { Id = "way-of-the-crane", Type = CardType.Event, Controller = p1 };
        var ownCrane = new Card { Id = "own-crane", Type = CardType.Character, Controller = p1, Faction = "crane" };
        var opponentCrane = new Card { Id = "opponent-crane", Type = CardType.Character, Controller = p2, Faction = "crane" };
        var ownScorpion = new Card { Id = "own-scorpion", Type = CardType.Character, Controller = p1, Faction = "scorpion" };
        p1.PlayArea.Add(ownCrane);
        p2.PlayArea.Add(opponentCrane);
        p1.PlayArea.Add(ownScorpion);

        var action = LoadFirstAction();
        var legalTargets = TargetResolver.ResolveLegalTargets(
            action.Target!,
            new AbilityContext { Game = game, Player = p1, Source = wayOfTheCrane });

        Assert.That(legalTargets, Is.EquivalentTo(new[] { ownCrane }));
    }

    [Test]
    public void HonorsTheChosenCharacter()
    {
        var game = NewGame(out var p1, out _);
        var wayOfTheCrane = new Card { Id = "way-of-the-crane", Type = CardType.Event, Controller = p1 };
        var target = new Card { Id = "own-crane", Type = CardType.Character, Controller = p1, Faction = "crane", IsDishonored = true };
        p1.PlayArea.Add(target);

        var action = LoadFirstAction();
        var context = new AbilityContext { Game = game, Player = p1, Source = wayOfTheCrane };
        var executor = new AbilityExecutor(new CostRegistry(), new GameActionRegistry());

        executor.Execute(action, context, chosenTarget: target);

        Assert.That(target.IsHonored, Is.True);
        Assert.That(target.IsDishonored, Is.False, "honor and dishonor are mutually exclusive");
    }
}
