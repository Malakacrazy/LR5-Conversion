using L5R.Engine.Abilities;
using L5R.Engine.Cards.Scripts;
using L5R.Engine.State;

namespace L5R.Engine.Tests.Cards.PerCard;

public class SpyglassTests
{
    [Test]
    public void AfterTheAttachedCharacterJoinsTheConflict_DrawsACard()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var parent = new Card { Id = "parent-character", Type = CardType.Character, Controller = p1 };
        var spyglass = new Card { Id = "spyglass", Type = CardType.Attachment, Controller = p1, AttachedTo = parent };
        var topOfDeck = new Card { Id = "top-of-deck", Type = CardType.Character, Controller = p1 };
        p1.PlayArea.Add(parent);
        p1.PlayArea.Add(spyglass);
        p1.Deck.Add(topOfDeck);

        var conflict = new Conflict { AttackingPlayer = p1, DefendingPlayer = p2 };
        conflict.Attackers.Add(parent);
        game.CurrentConflict = conflict;

        var context = new AbilityContext { Game = game, Player = p1, Source = spyglass };

        new SpyglassDrawOnParentJoiningConflict().Execute(context);

        Assert.That(p1.Hand, Does.Contain(topOfDeck));
    }

    [Test]
    public void WhileTheAttachedCharacterIsNotParticipating_Throws()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var parent = new Card { Id = "parent-character", Type = CardType.Character, Controller = p1 };
        var spyglass = new Card { Id = "spyglass", Type = CardType.Attachment, Controller = p1, AttachedTo = parent };
        p1.PlayArea.Add(parent);
        p1.PlayArea.Add(spyglass);

        var context = new AbilityContext { Game = game, Player = p1, Source = spyglass };

        Assert.Throws<InvalidOperationException>(() => new SpyglassDrawOnParentJoiningConflict().Execute(context));
    }
}
