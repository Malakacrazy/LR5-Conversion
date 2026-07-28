using System.Text.Json;
using L5R.Engine.Abilities;
using L5R.Engine.Dsl;
using L5R.Engine.State;

namespace L5R.Engine.Tests.Cards.PerCard;

/// <summary>
/// First card in the "one test file per card" suite (task 9), mirroring ringteki's
/// test/server/cards/ - Jasmine there, NUnit here. Proves the whole load-JSON ->
/// parse-abilities -> pay-cost -> execute-gameAction pipeline actually runs for one real,
/// simple ported card: vanguard-warrior sacrifices itself to place 1 fate on a chosen
/// character.
/// </summary>
public class VanguardWarriorTests
{
    private static ActionDefinition LoadFirstAction()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Cards", "01-Core", "vanguard-warrior.json");
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
    public void SacrificesItselfAndPlacesOneFateOnTheChosenCharacter()
    {
        var game = NewGame(out var p1, out _);
        var vanguardWarrior = new Card { Id = "vanguard-warrior", Type = CardType.Character, Controller = p1 };
        var recipient = new Card { Id = "recipient", Type = CardType.Character, Controller = p1, Fate = 0 };
        p1.PlayArea.Add(vanguardWarrior);
        p1.PlayArea.Add(recipient);

        var action = LoadFirstAction();
        var context = new AbilityContext { Game = game, Player = p1, Source = vanguardWarrior };
        var executor = new AbilityExecutor(new CostRegistry(), new GameActionRegistry());

        executor.Execute(action, context, chosenTarget: recipient);

        Assert.That(p1.PlayArea, Does.Not.Contain(vanguardWarrior), "sacrificed as the cost");
        Assert.That(p1.Discard, Does.Contain(vanguardWarrior));
        Assert.That(recipient.Fate, Is.EqualTo(1), "placeFate defaults to 1 when no amount param is given");
    }

    [Test]
    public void CanPlaceFateOnItselfWhenNoOtherCharacterIsAvailable()
    {
        // Neither the JSON nor the source ringteki VanguardWarrior.js excludes the source
        // from its own target - controller defaults to "any" and there is no cardCondition
        // excluding self, so this is legal, not just an artifact of the test setup.
        var game = NewGame(out var p1, out _);
        var vanguardWarrior = new Card { Id = "vanguard-warrior", Type = CardType.Character, Controller = p1 };
        p1.PlayArea.Add(vanguardWarrior);

        var action = LoadFirstAction();
        var legalTargets = TargetResolver.ResolveLegalTargets(
            action.Target!,
            new AbilityContext { Game = game, Player = p1, Source = vanguardWarrior });

        Assert.That(legalTargets, Does.Contain(vanguardWarrior));
    }
}
