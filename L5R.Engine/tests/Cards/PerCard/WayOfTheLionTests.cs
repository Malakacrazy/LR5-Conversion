using System.Text.Json;
using L5R.Engine.Abilities;
using L5R.Engine.Dsl;
using L5R.Engine.State;

namespace L5R.Engine.Tests.Cards.PerCard;

public class WayOfTheLionTests
{
    private static ActionDefinition LoadFirstAction()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Cards", "01-Core", "way-of-the-lion.json");
        using var document = JsonDocument.Parse(File.ReadAllText(path));
        return AbilityDefinitionParser.ParseActions(document.RootElement).Single();
    }

    [Test]
    public void DoublesTheBaseMilitarySkillOfAFriendlyLionCharacter()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var source = new Card { Id = "way-of-the-lion", Type = CardType.Event, Controller = p1 };
        var lion = new Card { Id = "lion-character", Type = CardType.Character, Controller = p1, Faction = "lion", PrintedMilitarySkill = 3 };
        p1.PlayArea.Add(lion);
        game.CurrentConflict = new Conflict { AttackingPlayer = p1, DefendingPlayer = p2 };

        var action = LoadFirstAction();
        var context = new AbilityContext { Game = game, Player = p1, Source = source };
        var executor = new AbilityExecutor(new CostRegistry(), new GameActionRegistry());

        executor.Execute(action, context, chosenTarget: lion);

        Assert.That(game.EffectiveMilitarySkill(lion), Is.EqualTo(6));
    }

    [Test]
    public void StacksAdditiveModifiersOnTopOfTheDoubledBase()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var source = new Card { Id = "way-of-the-lion", Type = CardType.Event, Controller = p1 };
        var lion = new Card { Id = "lion-character", Type = CardType.Character, Controller = p1, Faction = "lion", PrintedMilitarySkill = 3 };
        p1.PlayArea.Add(lion);
        game.CurrentConflict = new Conflict { AttackingPlayer = p1, DefendingPlayer = p2 };
        game.LastingEffects.Add(new LastingEffect { Target = lion, Stat = "military", Value = 1, Duration = "untilEndOfConflict" });

        var action = LoadFirstAction();
        var context = new AbilityContext { Game = game, Player = p1, Source = source };
        var executor = new AbilityExecutor(new CostRegistry(), new GameActionRegistry());

        executor.Execute(action, context, chosenTarget: lion);

        Assert.That(game.EffectiveMilitarySkill(lion), Is.EqualTo(7), "(3 base * 2) + 1 additive bonus");
    }

    [Test]
    public void ANonLionCharacterIsNotALegalTarget()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var source = new Card { Id = "way-of-the-lion", Type = CardType.Event, Controller = p1 };
        var crab = new Card { Id = "crab-character", Type = CardType.Character, Controller = p1, Faction = "crab", PrintedMilitarySkill = 3 };
        p1.PlayArea.Add(crab);

        var action = LoadFirstAction();
        var context = new AbilityContext { Game = game, Player = p1, Source = source };
        var legalTargets = TargetResolver.ResolveLegalTargets(action.Target!, context);

        Assert.That(legalTargets, Does.Not.Contain(crab), "not Lion");
    }

    [Test]
    public void ACharacterWithNoBaseMilitarySkillIsNotALegalTarget()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var source = new Card { Id = "way-of-the-lion", Type = CardType.Event, Controller = p1 };
        var courtier = new Card { Id = "lion-courtier", Type = CardType.Character, Controller = p1, Faction = "lion", PrintedMilitarySkill = 0 };
        p1.PlayArea.Add(courtier);

        var action = LoadFirstAction();
        var context = new AbilityContext { Game = game, Player = p1, Source = source };
        var legalTargets = TargetResolver.ResolveLegalTargets(action.Target!, context);

        Assert.That(legalTargets, Does.Not.Contain(courtier), "doubling 0 does nothing, so ringteki's cardCondition excludes it");
    }
}
