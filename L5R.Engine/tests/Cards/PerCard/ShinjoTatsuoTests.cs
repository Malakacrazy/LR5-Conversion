using L5R.Engine.Abilities;
using L5R.Engine.Cards.Scripts;
using L5R.Engine.State;

namespace L5R.Engine.Tests.Cards.PerCard;

public class ShinjoTatsuoTests
{
    [Test]
    public void WithNoAllyChosen_MovesOnlyItselfToTheConflict()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var tatsuo = new Card { Id = "shinjo-tatsuo", Type = CardType.Character, Controller = p1 };
        p1.PlayArea.Add(tatsuo);

        var conflict = new Conflict { AttackingPlayer = p1, DefendingPlayer = p2 };
        game.CurrentConflict = conflict;

        var context = new AbilityContext { Game = game, Player = p1, Source = tatsuo };

        new ShinjoTatsuoMoveSelfAndOptionalAllyToConflict().Execute(context);

        Assert.That(conflict.Attackers, Does.Contain(tatsuo));
        Assert.That(conflict.Attackers, Has.Count.EqualTo(1));
    }

    [Test]
    public void WithAnAllyChosen_MovesBothToTheConflict()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var tatsuo = new Card { Id = "shinjo-tatsuo", Type = CardType.Character, Controller = p1 };
        var ally = new Card { Id = "ally", Type = CardType.Character, Controller = p1 };
        p1.PlayArea.Add(tatsuo);
        p1.PlayArea.Add(ally);

        var conflict = new Conflict { AttackingPlayer = p1, DefendingPlayer = p2 };
        game.CurrentConflict = conflict;

        var context = new AbilityContext { Game = game, Player = p1, Source = tatsuo, Target = ally };

        new ShinjoTatsuoMoveSelfAndOptionalAllyToConflict().Execute(context);

        Assert.That(conflict.Attackers, Does.Contain(tatsuo));
        Assert.That(conflict.Attackers, Does.Contain(ally));
    }

    [Test]
    public void WithAnAllyControlledByTheOpponent_Throws()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var tatsuo = new Card { Id = "shinjo-tatsuo", Type = CardType.Character, Controller = p1 };
        var opponentCharacter = new Card { Id = "opponent-character", Type = CardType.Character, Controller = p2 };
        p1.PlayArea.Add(tatsuo);
        p2.PlayArea.Add(opponentCharacter);

        var conflict = new Conflict { AttackingPlayer = p1, DefendingPlayer = p2 };
        game.CurrentConflict = conflict;

        var context = new AbilityContext { Game = game, Player = p1, Source = tatsuo, Target = opponentCharacter };

        Assert.Throws<InvalidOperationException>(() => new ShinjoTatsuoMoveSelfAndOptionalAllyToConflict().Execute(context));
    }
}
