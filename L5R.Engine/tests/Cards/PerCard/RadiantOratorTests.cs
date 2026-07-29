using L5R.Engine.Abilities;
using L5R.Engine.Cards.Scripts;
using L5R.Engine.State;

namespace L5R.Engine.Tests.Cards.PerCard;

public class RadiantOratorTests
{
    [Test]
    public void WhenAheadOnGlory_SendsAnOpponentCharacterHome()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var orator = new Card { Id = "radiant-orator", Type = CardType.Character, Controller = p1, PrintedGlory = 2 };
        var opponentCharacter = new Card { Id = "opponent-character", Type = CardType.Character, Controller = p2, PrintedGlory = 1 };
        p1.PlayArea.Add(orator);
        p2.PlayArea.Add(opponentCharacter);

        var conflict = new Conflict { AttackingPlayer = p1, DefendingPlayer = p2 };
        conflict.Attackers.Add(orator);
        conflict.Defenders.Add(opponentCharacter);
        game.CurrentConflict = conflict;

        var context = new AbilityContext { Game = game, Player = p1, Source = orator, Target = opponentCharacter };

        new RadiantOratorSendHomeWhenAheadOnGlory().Execute(context);

        Assert.That(conflict.Defenders, Does.Not.Contain(opponentCharacter));
    }

    [Test]
    public void BowedCharactersDoNotCountTowardGlory()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var orator = new Card { Id = "radiant-orator", Type = CardType.Character, Controller = p1, PrintedGlory = 2, Bowed = true };
        var opponentCharacter = new Card { Id = "opponent-character", Type = CardType.Character, Controller = p2, PrintedGlory = 1 };
        p1.PlayArea.Add(orator);
        p2.PlayArea.Add(opponentCharacter);

        var conflict = new Conflict { AttackingPlayer = p1, DefendingPlayer = p2 };
        conflict.Attackers.Add(orator);
        conflict.Defenders.Add(opponentCharacter);
        game.CurrentConflict = conflict;

        var context = new AbilityContext { Game = game, Player = p1, Source = orator, Target = opponentCharacter };

        Assert.Throws<InvalidOperationException>(() => new RadiantOratorSendHomeWhenAheadOnGlory().Execute(context));
    }

    [Test]
    public void WhenNotAheadOnGlory_Throws()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var orator = new Card { Id = "radiant-orator", Type = CardType.Character, Controller = p1, PrintedGlory = 1 };
        var opponentCharacter = new Card { Id = "opponent-character", Type = CardType.Character, Controller = p2, PrintedGlory = 2 };
        p1.PlayArea.Add(orator);
        p2.PlayArea.Add(opponentCharacter);

        var conflict = new Conflict { AttackingPlayer = p1, DefendingPlayer = p2 };
        conflict.Attackers.Add(orator);
        conflict.Defenders.Add(opponentCharacter);
        game.CurrentConflict = conflict;

        var context = new AbilityContext { Game = game, Player = p1, Source = orator, Target = opponentCharacter };

        Assert.Throws<InvalidOperationException>(() => new RadiantOratorSendHomeWhenAheadOnGlory().Execute(context));
    }
}
