using L5R.Engine.Abilities;
using L5R.Engine.Cards.Scripts;
using L5R.Engine.State;

namespace L5R.Engine.Tests.Cards.PerCard;

public class EndlessPlainsTests
{
    [Test]
    public void WhenDeclaredAgainstIt_BreaksAndDiscardsAnAttacker()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var endlessPlains = new Card { Id = "endless-plains", Type = CardType.Province, Controller = p1 };
        var attacker = new Card { Id = "attacker", Type = CardType.Character, Controller = p2 };
        p2.PlayArea.Add(attacker);

        var conflict = new Conflict { AttackingPlayer = p2, DefendingPlayer = p1, DeclaredProvince = endlessPlains };
        conflict.Attackers.Add(attacker);
        game.CurrentConflict = conflict;

        var context = new AbilityContext { Game = game, Player = p1, Source = endlessPlains, Target = attacker };

        new EndlessPlainsBreakAndDiscardAttacker().Execute(context);

        Assert.That(endlessPlains.Broken, Is.True);
        Assert.That(p2.Discard, Does.Contain(attacker));
    }

    [Test]
    public void WhenAlreadyBroken_Throws()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var endlessPlains = new Card { Id = "endless-plains", Type = CardType.Province, Controller = p1, Broken = true };
        var attacker = new Card { Id = "attacker", Type = CardType.Character, Controller = p2 };
        p2.PlayArea.Add(attacker);

        var conflict = new Conflict { AttackingPlayer = p2, DefendingPlayer = p1, DeclaredProvince = endlessPlains };
        conflict.Attackers.Add(attacker);
        game.CurrentConflict = conflict;

        var context = new AbilityContext { Game = game, Player = p1, Source = endlessPlains, Target = attacker };

        Assert.Throws<InvalidOperationException>(() => new EndlessPlainsBreakAndDiscardAttacker().Execute(context));
    }

    [Test]
    public void WhenDeclaredAgainstADifferentProvince_Throws()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var endlessPlains = new Card { Id = "endless-plains", Type = CardType.Province, Controller = p1 };
        var otherProvince = new Card { Id = "other-province", Type = CardType.Province, Controller = p1 };
        var attacker = new Card { Id = "attacker", Type = CardType.Character, Controller = p2 };
        p2.PlayArea.Add(attacker);

        var conflict = new Conflict { AttackingPlayer = p2, DefendingPlayer = p1, DeclaredProvince = otherProvince };
        conflict.Attackers.Add(attacker);
        game.CurrentConflict = conflict;

        var context = new AbilityContext { Game = game, Player = p1, Source = endlessPlains, Target = attacker };

        Assert.Throws<InvalidOperationException>(() => new EndlessPlainsBreakAndDiscardAttacker().Execute(context));
    }
}
