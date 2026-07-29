using L5R.Engine.Abilities;
using L5R.Engine.Cards.Scripts;
using L5R.Engine.Dsl.GameActions;
using L5R.Engine.State;

namespace L5R.Engine.Tests.Cards.PerCard;

public class IndomitableWillTests
{
    [Test]
    public void WhenWinningAlone_TheSoleParticipantCannotBeBowed()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var source = new Card { Id = "indomitable-will", Type = CardType.Event, Controller = p1 };
        var soleParticipant = new Card { Id = "sole-participant", Type = CardType.Character, Controller = p1 };
        p1.PlayArea.Add(soleParticipant);

        var conflict = new Conflict { AttackingPlayer = p1, DefendingPlayer = p2, Winner = p1 };
        conflict.Attackers.Add(soleParticipant);
        game.CurrentConflict = conflict;

        var context = new AbilityContext { Game = game, Player = p1, Source = source };

        new IndomitableWillPreventBowOnSoloWin().Execute(context);

        Assert.Throws<InvalidOperationException>(
            () => new BowGameActionHandler().Execute(new AbilityContext { Game = game, Player = p1, Source = source, Target = soleParticipant }, null));
        Assert.That(soleParticipant.Bowed, Is.False);
    }

    [Test]
    public void WithMoreThanOneParticipant_Throws()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var source = new Card { Id = "indomitable-will", Type = CardType.Event, Controller = p1 };
        var participant1 = new Card { Id = "participant-1", Type = CardType.Character, Controller = p1 };
        var participant2 = new Card { Id = "participant-2", Type = CardType.Character, Controller = p1 };
        p1.PlayArea.Add(participant1);
        p1.PlayArea.Add(participant2);

        var conflict = new Conflict { AttackingPlayer = p1, DefendingPlayer = p2, Winner = p1 };
        conflict.Attackers.Add(participant1);
        conflict.Attackers.Add(participant2);
        game.CurrentConflict = conflict;

        var context = new AbilityContext { Game = game, Player = p1, Source = source };

        Assert.Throws<InvalidOperationException>(() => new IndomitableWillPreventBowOnSoloWin().Execute(context));
    }

    [Test]
    public void WhenTheControllerDoesNotWin_Throws()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var source = new Card { Id = "indomitable-will", Type = CardType.Event, Controller = p1 };
        var soleParticipant = new Card { Id = "sole-participant", Type = CardType.Character, Controller = p1 };
        p1.PlayArea.Add(soleParticipant);

        var conflict = new Conflict { AttackingPlayer = p1, DefendingPlayer = p2, Winner = p2 };
        conflict.Attackers.Add(soleParticipant);
        game.CurrentConflict = conflict;

        var context = new AbilityContext { Game = game, Player = p1, Source = source };

        Assert.Throws<InvalidOperationException>(() => new IndomitableWillPreventBowOnSoloWin().Execute(context));
    }
}
