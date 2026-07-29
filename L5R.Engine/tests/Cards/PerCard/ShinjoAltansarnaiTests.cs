using L5R.Engine.Abilities;
using L5R.Engine.Cards.Scripts;
using L5R.Engine.State;

namespace L5R.Engine.Tests.Cards.PerCard;

public class ShinjoAltansarnaiTests
{
    private static (GameState Game, Card Altansarnai, Card OpponentCharacter) NewGameAttackingInAMilitaryConflict()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var altansarnai = new Card { Id = "shinjo-altansarnai", Type = CardType.Character, Controller = p1 };
        var opponentCharacter = new Card { Id = "opponent-character", Type = CardType.Character, Controller = p2 };
        p1.PlayArea.Add(altansarnai);
        p2.PlayArea.Add(opponentCharacter);

        var conflict = new Conflict { AttackingPlayer = p1, DefendingPlayer = p2, ConflictType = "military" };
        conflict.Attackers.Add(altansarnai);
        game.CurrentConflict = conflict;

        return (game, altansarnai, opponentCharacter);
    }

    [Test]
    public void AfterBreakingAMilitaryProvinceWhileAttacking_TheOpponentDiscardsTheChosenCharacter()
    {
        var (game, altansarnai, opponentCharacter) = NewGameAttackingInAMilitaryConflict();

        var context = new AbilityContext { Game = game, Player = game.Player1, Source = altansarnai, Target = opponentCharacter };

        new ShinjoAltansarnaiDiscardOnMilitaryProvinceBreak().Execute(context);

        Assert.That(game.Player2.Discard, Does.Contain(opponentCharacter));
    }

    [Test]
    public void CannotChooseYourOwnCharacter()
    {
        var (game, altansarnai, _) = NewGameAttackingInAMilitaryConflict();

        var context = new AbilityContext { Game = game, Player = game.Player1, Source = altansarnai, Target = altansarnai };

        Assert.Throws<InvalidOperationException>(() => new ShinjoAltansarnaiDiscardOnMilitaryProvinceBreak().Execute(context));
    }

    [Test]
    public void DuringAPoliticalConflict_Throws()
    {
        var (game, altansarnai, opponentCharacter) = NewGameAttackingInAMilitaryConflict();
        game.CurrentConflict!.ConflictType = "political";

        var context = new AbilityContext { Game = game, Player = game.Player1, Source = altansarnai, Target = opponentCharacter };

        Assert.Throws<InvalidOperationException>(() => new ShinjoAltansarnaiDiscardOnMilitaryProvinceBreak().Execute(context));
    }

    [Test]
    public void WhileDefending_Throws()
    {
        var (game, altansarnai, opponentCharacter) = NewGameAttackingInAMilitaryConflict();
        game.CurrentConflict!.Attackers.Remove(altansarnai);
        game.CurrentConflict!.Defenders.Add(altansarnai);

        var context = new AbilityContext { Game = game, Player = game.Player1, Source = altansarnai, Target = opponentCharacter };

        Assert.Throws<InvalidOperationException>(() => new ShinjoAltansarnaiDiscardOnMilitaryProvinceBreak().Execute(context));
    }
}
