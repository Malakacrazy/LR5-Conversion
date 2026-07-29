using L5R.Engine.Abilities;
using L5R.Engine.Cards.Scripts;
using L5R.Engine.State;

namespace L5R.Engine.Tests.Cards.PerCard;

public class OutwitTests
{
    [Test]
    public void SendsHomeAnOpponentOutclassedByAParticipatingCourtier()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var source = new Card { Id = "outwit", Type = CardType.Event, Controller = p1 };
        var courtier = new Card { Id = "my-courtier", Type = CardType.Character, Controller = p1, Traits = new[] { "courtier" }, PrintedPoliticalSkill = 4 };
        var target = new Card { Id = "opponent-character", Type = CardType.Character, Controller = p2, PrintedPoliticalSkill = 2 };
        p1.PlayArea.Add(courtier);
        p2.PlayArea.Add(target);

        var conflict = new Conflict { AttackingPlayer = p1, DefendingPlayer = p2 };
        conflict.Attackers.Add(courtier);
        conflict.Defenders.Add(target);
        game.CurrentConflict = conflict;

        var context = new AbilityContext { Game = game, Player = p1, Source = source, Target = target };

        new OutwitSendHomeOutclassedByCourtier().Execute(context);

        Assert.That(conflict.Defenders, Does.Not.Contain(target));
    }

    [Test]
    public void WithoutAnOutclassingCourtier_Throws()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var source = new Card { Id = "outwit", Type = CardType.Event, Controller = p1 };
        var weakCourtier = new Card { Id = "my-courtier", Type = CardType.Character, Controller = p1, Traits = new[] { "courtier" }, PrintedPoliticalSkill = 1 };
        var target = new Card { Id = "opponent-character", Type = CardType.Character, Controller = p2, PrintedPoliticalSkill = 2 };
        p1.PlayArea.Add(weakCourtier);
        p2.PlayArea.Add(target);

        var conflict = new Conflict { AttackingPlayer = p1, DefendingPlayer = p2 };
        conflict.Attackers.Add(weakCourtier);
        conflict.Defenders.Add(target);
        game.CurrentConflict = conflict;

        var context = new AbilityContext { Game = game, Player = p1, Source = source, Target = target };

        Assert.Throws<InvalidOperationException>(() => new OutwitSendHomeOutclassedByCourtier().Execute(context));
    }
}
