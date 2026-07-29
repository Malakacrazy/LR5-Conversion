using L5R.Engine.Abilities;
using L5R.Engine.Cards.Scripts;
using L5R.Engine.State;

namespace L5R.Engine.Tests.Cards.PerCard;

public class SolemnScholarTests
{
    [Test]
    public void WhileTheEarthRingIsClaimed_BowsAnAttackingCharacterEvenFromHome()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var scholar = new Card { Id = "solemn-scholar", Type = CardType.Character, Controller = p1 };
        var attacker = new Card { Id = "opponent-attacker", Type = CardType.Character, Controller = p2 };
        p1.PlayArea.Add(scholar);
        p2.PlayArea.Add(attacker);

        game.Rings.Single(r => r.Element == "earth").ClaimedBy = p1;

        var conflict = new Conflict { AttackingPlayer = p2, DefendingPlayer = p1 };
        conflict.Attackers.Add(attacker);
        game.CurrentConflict = conflict;

        var context = new AbilityContext { Game = game, Player = p1, Source = scholar, Target = attacker };

        new SolemnScholarBowAttackerIfEarthClaimed().Execute(context);

        Assert.That(attacker.Bowed, Is.True);
    }

    [Test]
    public void WhenTheEarthRingIsNotClaimedByTheController_Throws()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var scholar = new Card { Id = "solemn-scholar", Type = CardType.Character, Controller = p1 };
        var attacker = new Card { Id = "opponent-attacker", Type = CardType.Character, Controller = p2 };
        p1.PlayArea.Add(scholar);
        p2.PlayArea.Add(attacker);

        game.Rings.Single(r => r.Element == "earth").ClaimedBy = p2;

        var conflict = new Conflict { AttackingPlayer = p2, DefendingPlayer = p1 };
        conflict.Attackers.Add(attacker);
        game.CurrentConflict = conflict;

        var context = new AbilityContext { Game = game, Player = p1, Source = scholar, Target = attacker };

        Assert.Throws<InvalidOperationException>(() => new SolemnScholarBowAttackerIfEarthClaimed().Execute(context));
        Assert.That(attacker.Bowed, Is.False);
    }

    [Test]
    public void ADefendingCharacter_Throws()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var scholar = new Card { Id = "solemn-scholar", Type = CardType.Character, Controller = p1 };
        var defender = new Card { Id = "opponent-defender", Type = CardType.Character, Controller = p2 };
        p1.PlayArea.Add(scholar);
        p2.PlayArea.Add(defender);

        game.Rings.Single(r => r.Element == "earth").ClaimedBy = p1;

        var conflict = new Conflict { AttackingPlayer = p1, DefendingPlayer = p2 };
        conflict.Defenders.Add(defender);
        game.CurrentConflict = conflict;

        var context = new AbilityContext { Game = game, Player = p1, Source = scholar, Target = defender };

        Assert.Throws<InvalidOperationException>(() => new SolemnScholarBowAttackerIfEarthClaimed().Execute(context));
    }
}
