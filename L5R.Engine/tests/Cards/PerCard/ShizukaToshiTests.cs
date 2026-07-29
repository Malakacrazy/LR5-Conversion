using System.Text.Json;
using L5R.Engine.Abilities;
using L5R.Engine.Dsl;
using L5R.Engine.State;

namespace L5R.Engine.Tests.Cards.PerCard;

public class ShizukaToshiTests
{
    private static ActionDefinition LoadFirstAction()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Cards", "01-Core", "shizuka-toshi.json");
        using var document = JsonDocument.Parse(File.ReadAllText(path));
        return AbilityDefinitionParser.ParseActions(document.RootElement).Single();
    }

    [Test]
    public void DuringAPoliticalConflict_BowsAParticipatingLowPoliticalSkillCharacter()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var stronghold = new Card { Id = "shizuka-toshi", Type = CardType.Stronghold, Controller = p1 };
        var weakCourtier = new Card { Id = "weak-courtier", Type = CardType.Character, Controller = p2, PrintedPoliticalSkill = 2 };
        p2.PlayArea.Add(weakCourtier);

        var conflict = new Conflict { AttackingPlayer = p2, DefendingPlayer = p1, ConflictType = "political" };
        conflict.Attackers.Add(weakCourtier);
        game.CurrentConflict = conflict;

        var action = LoadFirstAction();
        var context = new AbilityContext { Game = game, Player = p1, Source = stronghold };
        var executor = new AbilityExecutor(new CostRegistry(), new GameActionRegistry());

        executor.Execute(action, context, chosenTarget: weakCourtier);

        Assert.That(stronghold.Bowed, Is.True, "bowSelf cost was paid");
        Assert.That(weakCourtier.Bowed, Is.True);
    }

    [Test]
    public void AHighPoliticalSkillCharacter_IsNotALegalTarget()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var stronghold = new Card { Id = "shizuka-toshi", Type = CardType.Stronghold, Controller = p1 };
        var strongCourtier = new Card { Id = "strong-courtier", Type = CardType.Character, Controller = p2, PrintedPoliticalSkill = 3 };
        p2.PlayArea.Add(strongCourtier);

        var conflict = new Conflict { AttackingPlayer = p2, DefendingPlayer = p1, ConflictType = "political" };
        conflict.Attackers.Add(strongCourtier);
        game.CurrentConflict = conflict;

        var action = LoadFirstAction();
        var context = new AbilityContext { Game = game, Player = p1, Source = stronghold };
        var legalTargets = TargetResolver.ResolveLegalTargets(action.Target!, context);

        Assert.That(legalTargets, Does.Not.Contain(strongCourtier), "politicalSkill 3 > 2");
    }

    [Test]
    public void DuringAMilitaryConflict_ConditionIsNotMet()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var stronghold = new Card { Id = "shizuka-toshi", Type = CardType.Stronghold, Controller = p1 };

        var conflict = new Conflict { AttackingPlayer = p1, DefendingPlayer = p2, ConflictType = "military" };
        game.CurrentConflict = conflict;

        var action = LoadFirstAction();
        var context = new AbilityContext { Game = game, Player = p1, Source = stronghold };
        var executor = new AbilityExecutor(new CostRegistry(), new GameActionRegistry());

        Assert.That(executor.IsConditionMet(action, context), Is.False);
    }

    [Test]
    public void Provisions_StartingHonorAndFateIncomeAndStrengthBonus()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        var stronghold = new Card { Id = "shizuka-toshi", Type = CardType.Stronghold, Controller = p1, PrintedHonor = 11, PrintedFateIncome = 7, PrintedStrengthBonus = 2 };
        p1.Stronghold = stronghold;

        game.SetHonorFromStronghold(p1);

        Assert.That(p1.Honor, Is.EqualTo(11));
        Assert.That(game.FateIncomeFor(p1), Is.EqualTo(7));
        Assert.That(game.StrongholdStrengthBonusFor(p1), Is.EqualTo(2));
    }
}
