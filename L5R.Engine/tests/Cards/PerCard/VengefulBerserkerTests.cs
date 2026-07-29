using L5R.Engine.Abilities;
using L5R.Engine.Cards.Scripts;
using L5R.Engine.State;

namespace L5R.Engine.Tests.Cards.PerCard;

public class VengefulBerserkerTests
{
    [Test]
    public void WhenAnAllyLeavesPlayDuringAConflict_DoublesOwnMilitarySkill()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var berserker = new Card { Id = "vengeful-berserker", Type = CardType.Character, Controller = p1, PrintedMilitarySkill = 3 };
        var departedAlly = new Card { Id = "departed-ally", Type = CardType.Character, Controller = p1 };
        p1.PlayArea.Add(berserker);

        var conflict = new Conflict { AttackingPlayer = p1, DefendingPlayer = p2 };
        game.CurrentConflict = conflict;

        var context = new AbilityContext { Game = game, Player = p1, Source = berserker, Target = departedAlly };

        new VengefulBerserkerDoubleMilitaryOnAllyLeavingPlay().Execute(context);

        Assert.That(game.EffectiveMilitarySkill(berserker), Is.EqualTo(6));
    }

    [Test]
    public void OutsideAConflict_Throws()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Dynasty };
        var berserker = new Card { Id = "vengeful-berserker", Type = CardType.Character, Controller = p1, PrintedMilitarySkill = 3 };
        var departedAlly = new Card { Id = "departed-ally", Type = CardType.Character, Controller = p1 };
        p1.PlayArea.Add(berserker);

        var context = new AbilityContext { Game = game, Player = p1, Source = berserker, Target = departedAlly };

        Assert.Throws<InvalidOperationException>(() => new VengefulBerserkerDoubleMilitaryOnAllyLeavingPlay().Execute(context));
    }

    [Test]
    public void WhenTheDepartedCharacterBelongsToTheOpponent_Throws()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var berserker = new Card { Id = "vengeful-berserker", Type = CardType.Character, Controller = p1, PrintedMilitarySkill = 3 };
        var opponentCharacter = new Card { Id = "opponent-character", Type = CardType.Character, Controller = p2 };
        p1.PlayArea.Add(berserker);

        var conflict = new Conflict { AttackingPlayer = p1, DefendingPlayer = p2 };
        game.CurrentConflict = conflict;

        var context = new AbilityContext { Game = game, Player = p1, Source = berserker, Target = opponentCharacter };

        Assert.Throws<InvalidOperationException>(() => new VengefulBerserkerDoubleMilitaryOnAllyLeavingPlay().Execute(context));
    }
}
