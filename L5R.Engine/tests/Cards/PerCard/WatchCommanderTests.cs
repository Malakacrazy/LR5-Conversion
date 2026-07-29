using L5R.Engine.Abilities;
using L5R.Engine.Cards.Scripts;
using L5R.Engine.State;

namespace L5R.Engine.Tests.Cards.PerCard;

/// <summary>
/// watch-commander's reaction is scriptOverride'd (WatchCommanderLoseHonorOnOpponentCardPlayed) -
/// its own persistentEffects block (attachmentLimit/attachmentMyControlOnly) is generic DSL
/// territory, covered by GameState.ExceedsAttachmentLimit/IsAttachRestricted's own tests
/// elsewhere; this file only exercises the scripted reaction.
/// </summary>
public class WatchCommanderTests
{
    [Test]
    public void WhileTheAttachedCharacterParticipates_TheOpponentLosesOneHonor()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2", Honor = 5 };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var parent = new Card { Id = "parent-character", Type = CardType.Character, Controller = p1 };
        var watchCommander = new Card { Id = "watch-commander", Type = CardType.Attachment, Controller = p1, AttachedTo = parent };
        p1.PlayArea.Add(parent);
        p1.PlayArea.Add(watchCommander);

        var conflict = new Conflict { AttackingPlayer = p1, DefendingPlayer = p2 };
        conflict.Attackers.Add(parent);
        game.CurrentConflict = conflict;

        var context = new AbilityContext { Game = game, Player = p1, Source = watchCommander };

        new WatchCommanderLoseHonorOnOpponentCardPlayed().Execute(context);

        Assert.That(p2.Honor, Is.EqualTo(4));
    }

    [Test]
    public void WhileNotParticipating_Throws()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2", Honor = 5 };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var parent = new Card { Id = "parent-character", Type = CardType.Character, Controller = p1 };
        var watchCommander = new Card { Id = "watch-commander", Type = CardType.Attachment, Controller = p1, AttachedTo = parent };
        p1.PlayArea.Add(parent);
        p1.PlayArea.Add(watchCommander);

        var context = new AbilityContext { Game = game, Player = p1, Source = watchCommander };

        Assert.Throws<InvalidOperationException>(() => new WatchCommanderLoseHonorOnOpponentCardPlayed().Execute(context));
        Assert.That(p2.Honor, Is.EqualTo(5), "nothing happened");
    }

    [Test]
    public void WhenNotAttached_Throws()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2", Honor = 5 };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var watchCommander = new Card { Id = "watch-commander", Type = CardType.Attachment, Controller = p1 };
        p1.PlayArea.Add(watchCommander);

        var context = new AbilityContext { Game = game, Player = p1, Source = watchCommander };

        Assert.Throws<InvalidOperationException>(() => new WatchCommanderLoseHonorOnOpponentCardPlayed().Execute(context));
    }
}
