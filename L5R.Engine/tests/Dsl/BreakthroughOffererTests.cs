using System.Linq;
using L5R.Engine.Dsl;
using L5R.Engine.GameSteps;
using L5R.Engine.State;

namespace L5R.Engine.Tests.Dsl;

public class BreakthroughOffererTests
{
    private static (GameState game, Player p1, Card card) NewScenario(bool provinceBroken = true, int declarationsThisPhase = 1)
    {
        var p1 = new Player { Name = "Player1", Fate = 5 };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };

        var card = new Card { Id = "breakthrough", Type = CardType.Event, Controller = p1, Location = "hand", PrintedCost = 0 };
        p1.Hand.Add(card);

        var freshAttacker = new Card { Id = "fresh-attacker", Type = CardType.Character, Controller = p1, PrintedMilitarySkill = 3 };
        p1.PlayArea.Add(freshAttacker);

        var brokenProvince = new Card { Id = "broken-province", Type = CardType.Province, Controller = p2, Broken = provinceBroken };
        var otherProvince = new Card { Id = "other-province", Type = CardType.Province, Controller = p2 };
        p2.Provinces.Add(otherProvince);

        var finishedConflict = new Conflict { AttackingPlayer = p1, DefendingPlayer = p2, Winner = p1, DeclaredProvince = brokenProvince };
        game.ConflictRecord.Add(finishedConflict);

        for (var i = 0; i < declarationsThisPhase; i++)
            game.ConflictDeclarationsThisPhase.Add((p1, false));

        return (game, p1, card);
    }

    [Test]
    public void IsEligible_AfterBreakingAProvinceAsTheOnlyDeclarationThisPhase_IsTrueAndCommitSpendsTheCard()
    {
        var (game, p1, card) = NewScenario();
        var policy = new FirstLegalActionBotPolicy();

        Assert.That(BreakthroughOfferer.IsEligible(game, p1, out var breakthroughCard, out var cost), Is.True);
        var declaration = policy.DeclareConflict(game, p1).Result;
        Assert.That(declaration, Is.Not.Null);

        BreakthroughOfferer.Commit(game, p1, breakthroughCard!, cost);

        Assert.That(p1.Hand, Does.Not.Contain(card));
        Assert.That(p1.Discard, Contains.Item(card));
    }

    [Test]
    public void IsEligible_WhenTheProvinceDidNotBreak_IsFalseAndKeepsTheCard()
    {
        var (game, p1, card) = NewScenario(provinceBroken: false);

        Assert.That(BreakthroughOfferer.IsEligible(game, p1, out _, out _), Is.False);
        Assert.That(p1.Hand, Contains.Item(card));
    }

    [Test]
    public void IsEligible_WhenThisWasNotTheOnlyDeclarationThisPhase_IsFalse()
    {
        var (game, p1, _) = NewScenario(declarationsThisPhase: 2);

        Assert.That(BreakthroughOfferer.IsEligible(game, p1, out _, out _), Is.False);
    }

    [Test]
    public void IsEligible_WithNoEligibleSecondAttacker_IsTrueButThePolicyDeclinesAndTheCardIsNeverSpent()
    {
        var (game, p1, card) = NewScenario();
        p1.PlayArea.Single().Bowed = true; // the only attacker is already bowed
        var policy = new FirstLegalActionBotPolicy();

        Assert.That(BreakthroughOfferer.IsEligible(game, p1, out _, out _), Is.True, "breakthrough's own preconditions don't check attacker eligibility - that's the policy's job");
        var declaration = policy.DeclareConflict(game, p1).Result;

        Assert.That(declaration, Is.Null);
        Assert.That(p1.Hand, Contains.Item(card), "never spent since Commit is only called when the policy actually returns a declaration");
    }
}
