using L5R.Engine.Abilities;
using L5R.Engine.Cards.Scripts;
using L5R.Engine.State;

namespace L5R.Engine.Tests.Cards.PerCard;

public class ReadyForBattleTests
{
    [Test]
    public void WhenTheOpponentBowsMyCharacter_ReadiesIt()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var readyForBattle = new Card { Id = "ready-for-battle", Type = CardType.Event, Controller = p1 };
        var bowedCharacter = new Card { Id = "bowed-character", Type = CardType.Character, Controller = p1, Bowed = true };
        p1.PlayArea.Add(bowedCharacter);

        var context = new AbilityContext { Game = game, Player = p1, Source = readyForBattle, Target = bowedCharacter, BowCausedBySelf = false };

        new ReadyForBattleReadyOnOpponentOrRingBow().Execute(context);

        Assert.That(bowedCharacter.Bowed, Is.False);
    }

    [Test]
    public void WhenTheBowWasCausedByItsOwnController_Throws()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var readyForBattle = new Card { Id = "ready-for-battle", Type = CardType.Event, Controller = p1 };
        var bowedCharacter = new Card { Id = "bowed-character", Type = CardType.Character, Controller = p1, Bowed = true };
        p1.PlayArea.Add(bowedCharacter);

        var context = new AbilityContext { Game = game, Player = p1, Source = readyForBattle, Target = bowedCharacter, BowCausedBySelf = true };

        Assert.Throws<InvalidOperationException>(() => new ReadyForBattleReadyOnOpponentOrRingBow().Execute(context));
    }

    [Test]
    public void WhenTheBowedCharacterBelongsToTheOpponent_Throws()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var readyForBattle = new Card { Id = "ready-for-battle", Type = CardType.Event, Controller = p1 };
        var opponentBowedCharacter = new Card { Id = "opponent-bowed", Type = CardType.Character, Controller = p2, Bowed = true };
        p2.PlayArea.Add(opponentBowedCharacter);

        var context = new AbilityContext { Game = game, Player = p1, Source = readyForBattle, Target = opponentBowedCharacter };

        Assert.Throws<InvalidOperationException>(() => new ReadyForBattleReadyOnOpponentOrRingBow().Execute(context));
    }
}
