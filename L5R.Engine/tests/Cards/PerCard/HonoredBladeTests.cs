using L5R.Engine.Abilities;
using L5R.Engine.Cards.Scripts;
using L5R.Engine.State;

namespace L5R.Engine.Tests.Cards.PerCard;

public class HonoredBladeTests
{
    private static (GameState Game, Card Blade, Card Parent) NewGameWithAttachedParticipant()
    {
        var p1 = new Player { Name = "Player1", Honor = 5 };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var parent = new Card { Id = "parent-character", Type = CardType.Character, Controller = p1 };
        var blade = new Card { Id = "honored-blade", Type = CardType.Attachment, Controller = p1, AttachedTo = parent };
        p1.PlayArea.Add(parent);
        p1.PlayArea.Add(blade);

        var conflict = new Conflict { AttackingPlayer = p1, DefendingPlayer = p2 };
        conflict.Attackers.Add(parent);
        game.CurrentConflict = conflict;

        return (game, blade, parent);
    }

    [Test]
    public void WhenTheAttachedCharactersControllerWins_GainsOneHonor()
    {
        var (game, blade, _) = NewGameWithAttachedParticipant();
        game.CurrentConflict!.Winner = game.Player1;

        var context = new AbilityContext { Game = game, Player = game.Player1, Source = blade };

        new HonoredBladeGainHonorWhenParentWins().Execute(context);

        Assert.That(game.Player1.Honor, Is.EqualTo(6));
    }

    [Test]
    public void WhenTheAttachedCharactersControllerLoses_Throws()
    {
        var (game, blade, _) = NewGameWithAttachedParticipant();
        game.CurrentConflict!.Winner = game.Player2;

        var context = new AbilityContext { Game = game, Player = game.Player1, Source = blade };

        Assert.Throws<InvalidOperationException>(() => new HonoredBladeGainHonorWhenParentWins().Execute(context));
        Assert.That(game.Player1.Honor, Is.EqualTo(5), "nothing happened");
    }

    [Test]
    public void WhenNoWinnerIsSet_Throws()
    {
        var (game, blade, _) = NewGameWithAttachedParticipant();

        var context = new AbilityContext { Game = game, Player = game.Player1, Source = blade };

        Assert.Throws<InvalidOperationException>(() => new HonoredBladeGainHonorWhenParentWins().Execute(context));
    }

    [Test]
    public void WhileNotParticipating_Throws()
    {
        var p1 = new Player { Name = "Player1", Honor = 5 };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var parent = new Card { Id = "parent-character", Type = CardType.Character, Controller = p1 };
        var blade = new Card { Id = "honored-blade", Type = CardType.Attachment, Controller = p1, AttachedTo = parent };
        p1.PlayArea.Add(parent);
        p1.PlayArea.Add(blade);

        var conflict = new Conflict { AttackingPlayer = p1, DefendingPlayer = p2, Winner = p1 };
        game.CurrentConflict = conflict;

        var context = new AbilityContext { Game = game, Player = p1, Source = blade };

        Assert.Throws<InvalidOperationException>(() => new HonoredBladeGainHonorWhenParentWins().Execute(context));
    }
}
