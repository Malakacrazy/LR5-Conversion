using System.Text.Json;
using L5R.Engine.Abilities;
using L5R.Engine.Dsl;
using L5R.Engine.State;

namespace L5R.Engine.Tests.Cards.PerCard;

public class WayOfTheScorpionTests
{
    private static ActionDefinition LoadFirstAction()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Cards", "01-Core", "way-of-the-scorpion.json");
        using var document = JsonDocument.Parse(File.ReadAllText(path));
        return AbilityDefinitionParser.ParseActions(document.RootElement).Single();
    }

    [Test]
    public void OnlyTargetsParticipatingNonScorpionCharacters()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var source = new Card { Id = "way-of-the-scorpion", Type = CardType.Event, Controller = p1 };
        var participatingLion = new Card { Id = "participating-lion", Type = CardType.Character, Controller = p2, Faction = "lion" };
        var participatingScorpion = new Card { Id = "participating-scorpion", Type = CardType.Character, Controller = p1, Faction = "scorpion" };
        var nonParticipatingLion = new Card { Id = "non-participating-lion", Type = CardType.Character, Controller = p2, Faction = "lion" };
        p1.PlayArea.Add(participatingScorpion);
        p2.PlayArea.Add(participatingLion);
        p2.PlayArea.Add(nonParticipatingLion);

        var conflict = new Conflict { AttackingPlayer = p1, DefendingPlayer = p2 };
        conflict.Attackers.Add(participatingScorpion);
        conflict.Defenders.Add(participatingLion);
        game.CurrentConflict = conflict;

        var action = LoadFirstAction();
        var legalTargets = TargetResolver.ResolveLegalTargets(
            action.Target!,
            new AbilityContext { Game = game, Player = p1, Source = source });

        Assert.That(legalTargets, Is.EquivalentTo(new[] { participatingLion }));
    }

    [Test]
    public void DishonorsTheChosenParticipant()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var source = new Card { Id = "way-of-the-scorpion", Type = CardType.Event, Controller = p1 };
        var target = new Card { Id = "participating-lion", Type = CardType.Character, Controller = p2, Faction = "lion" };
        p2.PlayArea.Add(target);

        var conflict = new Conflict { AttackingPlayer = p1, DefendingPlayer = p2 };
        conflict.Defenders.Add(target);
        game.CurrentConflict = conflict;

        var action = LoadFirstAction();
        var context = new AbilityContext { Game = game, Player = p1, Source = source };
        var executor = new AbilityExecutor(new CostRegistry(), new GameActionRegistry());

        executor.Execute(action, context, chosenTarget: target);

        Assert.That(target.IsDishonored, Is.True);
    }
}
