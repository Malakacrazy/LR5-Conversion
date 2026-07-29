using L5R.Engine.Abilities;
using L5R.Engine.Cards.Scripts;
using L5R.Engine.State;

namespace L5R.Engine.Tests.Cards.PerCard;

public class VengefulOathkeeperTests
{
    private static (GameState Game, Card Oathkeeper) NewGameLostAMilitaryConflictWithOathkeeperInHand(bool defending = false)
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var oathkeeper = new Card { Id = "vengeful-oathkeeper", Type = CardType.Character, Controller = p1, Location = "hand" };
        p1.Hand.Add(oathkeeper);

        var conflict = defending
            ? new Conflict { AttackingPlayer = p2, DefendingPlayer = p1, ConflictType = "military", Loser = p1 }
            : new Conflict { AttackingPlayer = p1, DefendingPlayer = p2, ConflictType = "military", Loser = p1 };
        game.CurrentConflict = conflict;

        return (game, oathkeeper);
    }

    [Test]
    public void AfterLosingAMilitaryConflictAsAttacker_EntersPlayJoiningAsAnAttacker()
    {
        var (game, oathkeeper) = NewGameLostAMilitaryConflictWithOathkeeperInHand();

        var context = new AbilityContext { Game = game, Player = game.Player1, Source = oathkeeper };

        new VengefulOathkeeperPutIntoPlayOnMilitaryLoss().Execute(context);

        Assert.That(game.Player1.PlayArea, Does.Contain(oathkeeper));
        Assert.That(game.Player1.Hand, Does.Not.Contain(oathkeeper));
        Assert.That(game.CurrentConflict!.Attackers, Does.Contain(oathkeeper));
    }

    [Test]
    public void AfterLosingAsDefender_EntersPlayJoiningAsADefender()
    {
        var (game, oathkeeper) = NewGameLostAMilitaryConflictWithOathkeeperInHand(defending: true);

        var context = new AbilityContext { Game = game, Player = game.Player1, Source = oathkeeper };

        new VengefulOathkeeperPutIntoPlayOnMilitaryLoss().Execute(context);

        Assert.That(game.CurrentConflict!.Defenders, Does.Contain(oathkeeper));
    }

    [Test]
    public void AfterLosingAPoliticalConflict_Throws()
    {
        var (game, oathkeeper) = NewGameLostAMilitaryConflictWithOathkeeperInHand();
        game.CurrentConflict!.ConflictType = "political";

        var context = new AbilityContext { Game = game, Player = game.Player1, Source = oathkeeper };

        Assert.Throws<InvalidOperationException>(() => new VengefulOathkeeperPutIntoPlayOnMilitaryLoss().Execute(context));
    }

    [Test]
    public void AfterWinning_Throws()
    {
        var (game, oathkeeper) = NewGameLostAMilitaryConflictWithOathkeeperInHand();
        game.CurrentConflict!.Loser = game.Player2;

        var context = new AbilityContext { Game = game, Player = game.Player1, Source = oathkeeper };

        Assert.Throws<InvalidOperationException>(() => new VengefulOathkeeperPutIntoPlayOnMilitaryLoss().Execute(context));
    }

    [Test]
    public void WhenNotInHand_Throws()
    {
        var (game, oathkeeper) = NewGameLostAMilitaryConflictWithOathkeeperInHand();
        game.Player1.Hand.Remove(oathkeeper);
        game.Player1.PlayArea.Add(oathkeeper);
        oathkeeper.Location = "play area";

        var context = new AbilityContext { Game = game, Player = game.Player1, Source = oathkeeper };

        Assert.Throws<InvalidOperationException>(() => new VengefulOathkeeperPutIntoPlayOnMilitaryLoss().Execute(context));
    }
}
