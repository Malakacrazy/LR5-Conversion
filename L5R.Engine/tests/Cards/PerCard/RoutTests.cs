using L5R.Engine.Abilities;
using L5R.Engine.Cards.Scripts;
using L5R.Engine.State;

namespace L5R.Engine.Tests.Cards.PerCard;

public class RoutTests
{
    [Test]
    public void SendsHomeAnOpponentOutclassedByAParticipatingBushi()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var source = new Card { Id = "rout", Type = CardType.Event, Controller = p1 };
        var bushi = new Card { Id = "my-bushi", Type = CardType.Character, Controller = p1, Traits = new[] { "bushi" }, PrintedMilitarySkill = 4 };
        var target = new Card { Id = "opponent-character", Type = CardType.Character, Controller = p2, PrintedMilitarySkill = 2 };
        p1.PlayArea.Add(bushi);
        p2.PlayArea.Add(target);

        var conflict = new Conflict { AttackingPlayer = p1, DefendingPlayer = p2 };
        conflict.Attackers.Add(bushi);
        conflict.Defenders.Add(target);
        game.CurrentConflict = conflict;

        var context = new AbilityContext { Game = game, Player = p1, Source = source, Target = target };

        new RoutSendHomeOutclassedByBushi().Execute(context);

        Assert.That(conflict.Defenders, Does.Not.Contain(target));
    }

    [Test]
    public void WithoutAnOutclassingBushi_Throws()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var source = new Card { Id = "rout", Type = CardType.Event, Controller = p1 };
        var weakBushi = new Card { Id = "my-bushi", Type = CardType.Character, Controller = p1, Traits = new[] { "bushi" }, PrintedMilitarySkill = 1 };
        var target = new Card { Id = "opponent-character", Type = CardType.Character, Controller = p2, PrintedMilitarySkill = 2 };
        p1.PlayArea.Add(weakBushi);
        p2.PlayArea.Add(target);

        var conflict = new Conflict { AttackingPlayer = p1, DefendingPlayer = p2 };
        conflict.Attackers.Add(weakBushi);
        conflict.Defenders.Add(target);
        game.CurrentConflict = conflict;

        var context = new AbilityContext { Game = game, Player = p1, Source = source, Target = target };

        Assert.Throws<InvalidOperationException>(() => new RoutSendHomeOutclassedByBushi().Execute(context));
    }

    [Test]
    public void ANonParticipatingBushi_DoesNotCount()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var source = new Card { Id = "rout", Type = CardType.Event, Controller = p1 };
        var homeBushi = new Card { Id = "my-bushi", Type = CardType.Character, Controller = p1, Traits = new[] { "bushi" }, PrintedMilitarySkill = 4 };
        var target = new Card { Id = "opponent-character", Type = CardType.Character, Controller = p2, PrintedMilitarySkill = 2 };
        p1.PlayArea.Add(homeBushi);
        p2.PlayArea.Add(target);

        var conflict = new Conflict { AttackingPlayer = p1, DefendingPlayer = p2 };
        conflict.Defenders.Add(target);
        game.CurrentConflict = conflict;

        var context = new AbilityContext { Game = game, Player = p1, Source = source, Target = target };

        Assert.Throws<InvalidOperationException>(() => new RoutSendHomeOutclassedByBushi().Execute(context));
    }
}
