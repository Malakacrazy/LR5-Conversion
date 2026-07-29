using System.Text.Json;
using L5R.Engine.Abilities;
using L5R.Engine.Dsl;
using L5R.Engine.State;

namespace L5R.Engine.Tests.Cards.PerCard;

public class ForShameTests
{
    private static ActionDefinition LoadFirstAction()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Cards", "01-Core", "for-shame.json");
        using var document = JsonDocument.Parse(File.ReadAllText(path));
        return AbilityDefinitionParser.ParseActions(document.RootElement).Single();
    }

    private static (GameState Game, Card Source, Card OpponentCharacter) NewGameWithLegalCondition()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var source = new Card { Id = "for-shame", Type = CardType.Event, Controller = p1 };
        var ownCourtier = new Card { Id = "own-courtier", Type = CardType.Character, Controller = p1, Traits = new[] { "courtier" } };
        var opponentCharacter = new Card { Id = "opponent-character", Type = CardType.Character, Controller = p2 };
        p1.PlayArea.Add(ownCourtier);
        p2.PlayArea.Add(opponentCharacter);

        var conflict = new Conflict { AttackingPlayer = p1, DefendingPlayer = p2 };
        conflict.Attackers.Add(ownCourtier);
        conflict.Defenders.Add(opponentCharacter);
        game.CurrentConflict = conflict;

        return (game, source, opponentCharacter);
    }

    [Test]
    public void ChoosingDishonor_DishonorsTheChosenCharacter()
    {
        var (game, source, opponentCharacter) = NewGameWithLegalCondition();

        var action = LoadFirstAction();
        var context = new AbilityContext { Game = game, Player = game.Player1, Source = source };
        var executor = new AbilityExecutor(new CostRegistry(), new GameActionRegistry());

        executor.Execute(action, context, chosenTarget: opponentCharacter, chosenChoice: "Dishonor this character");

        Assert.That(opponentCharacter.IsDishonored, Is.True);
        Assert.That(opponentCharacter.Bowed, Is.False);
    }

    [Test]
    public void ChoosingBow_BowsTheChosenCharacterInstead()
    {
        var (game, source, opponentCharacter) = NewGameWithLegalCondition();

        var action = LoadFirstAction();
        var context = new AbilityContext { Game = game, Player = game.Player1, Source = source };
        var executor = new AbilityExecutor(new CostRegistry(), new GameActionRegistry());

        executor.Execute(action, context, chosenTarget: opponentCharacter, chosenChoice: "Bow this character");

        Assert.That(opponentCharacter.Bowed, Is.True);
        Assert.That(opponentCharacter.IsDishonored, Is.False);
    }

    [Test]
    public void ANonParticipatingOpponentCharacter_IsNotALegalTarget()
    {
        var (game, source, _) = NewGameWithLegalCondition();
        var homeCharacter = new Card { Id = "home-character", Type = CardType.Character, Controller = game.Player2 };
        game.Player2.PlayArea.Add(homeCharacter);

        var action = LoadFirstAction();
        var context = new AbilityContext { Game = game, Player = game.Player1, Source = source };
        var legalTargets = TargetResolver.ResolveLegalTargets(action.SelectDependsOnTargets!.DependencyTarget, context);

        Assert.That(legalTargets, Does.Not.Contain(homeCharacter), "not participating");
    }

    [Test]
    public void WithoutAParticipatingCourtierOnTheCastersSide_ConditionIsNotMet()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var source = new Card { Id = "for-shame", Type = CardType.Event, Controller = p1 };
        var nonCourtier = new Card { Id = "non-courtier", Type = CardType.Character, Controller = p1 };
        p1.PlayArea.Add(nonCourtier);

        var conflict = new Conflict { AttackingPlayer = p1, DefendingPlayer = p2 };
        conflict.Attackers.Add(nonCourtier);
        game.CurrentConflict = conflict;

        var action = LoadFirstAction();
        var context = new AbilityContext { Game = game, Player = p1, Source = source };

        Assert.That(new AbilityExecutor(new CostRegistry(), new GameActionRegistry()).IsConditionMet(action, context), Is.False);
    }
}
