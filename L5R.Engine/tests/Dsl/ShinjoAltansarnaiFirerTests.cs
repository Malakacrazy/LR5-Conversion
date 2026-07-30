using L5R.Engine.Dsl;
using L5R.Engine.State;

namespace L5R.Engine.Tests.Dsl;

public class ShinjoAltansarnaiFirerTests
{
    [Test]
    public void FireIfLegal_AfterBreakingAMilitaryProvinceWhileAttacking_TheOpponentDiscardsACharacter()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        var altansarnai = new Card { Id = "shinjo-altansarnai", Type = CardType.Character, Controller = p1 };
        var opponentCharacter = new Card { Id = "opponent-character", Type = CardType.Character, Controller = p2 };
        p1.PlayArea.Add(altansarnai);
        p2.PlayArea.Add(opponentCharacter);

        var conflict = new Conflict { AttackingPlayer = p1, DefendingPlayer = p2, ConflictType = "military" };
        conflict.Attackers.Add(altansarnai);
        game.CurrentConflict = conflict;

        ShinjoAltansarnaiFirer.FireIfLegal(game, conflict);

        Assert.That(p2.Discard, Does.Contain(opponentCharacter));
    }

    [Test]
    public void FireIfLegal_DuringAPoliticalConflict_DoesNothing()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        var altansarnai = new Card { Id = "shinjo-altansarnai", Type = CardType.Character, Controller = p1 };
        var opponentCharacter = new Card { Id = "opponent-character", Type = CardType.Character, Controller = p2 };
        p1.PlayArea.Add(altansarnai);
        p2.PlayArea.Add(opponentCharacter);

        var conflict = new Conflict { AttackingPlayer = p1, DefendingPlayer = p2, ConflictType = "political" };
        conflict.Attackers.Add(altansarnai);

        ShinjoAltansarnaiFirer.FireIfLegal(game, conflict);

        Assert.That(p2.PlayArea, Does.Contain(opponentCharacter));
    }

    [Test]
    public void FireIfLegal_WhenTheOpponentHasNoCharacter_DoesNotThrow()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        var altansarnai = new Card { Id = "shinjo-altansarnai", Type = CardType.Character, Controller = p1 };
        p1.PlayArea.Add(altansarnai);

        var conflict = new Conflict { AttackingPlayer = p1, DefendingPlayer = p2, ConflictType = "military" };
        conflict.Attackers.Add(altansarnai);

        Assert.DoesNotThrow(() => ShinjoAltansarnaiFirer.FireIfLegal(game, conflict));
    }
}
