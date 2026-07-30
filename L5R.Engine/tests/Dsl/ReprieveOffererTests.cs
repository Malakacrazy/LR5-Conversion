using L5R.Engine.Abilities;
using L5R.Engine.Dsl;
using L5R.Engine.Dsl.GameActions;
using L5R.Engine.State;

namespace L5R.Engine.Tests.Dsl;

public class ReprieveOffererTests
{
    [Test]
    public void TryInterrupt_WhenAttached_DiscardsItselfInsteadAndReturnsTrue()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        var parent = new Card { Id = "some-character", Type = CardType.Character, Controller = p1 };
        var reprieve = new Card { Id = "reprieve", Type = CardType.Attachment, Controller = p1, AttachedTo = parent };
        p1.PlayArea.Add(parent);
        p1.PlayArea.Add(reprieve);

        var intercepted = ReprieveOfferer.TryInterrupt(game, parent);

        Assert.That(intercepted, Is.True);
        Assert.That(p1.Discard, Does.Contain(reprieve));
        Assert.That(p1.PlayArea, Does.Contain(parent));
    }

    [Test]
    public void TryInterrupt_WithNoReprieveAttached_ReturnsFalse()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        var parent = new Card { Id = "some-character", Type = CardType.Character, Controller = p1 };
        p1.PlayArea.Add(parent);

        Assert.That(ReprieveOfferer.TryInterrupt(game, parent), Is.False);
    }

    [Test]
    public void DiscardFromPlay_WithReprieveAttached_SavesTheCharacter()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        var parent = new Card { Id = "some-character", Type = CardType.Character, Controller = p1 };
        var reprieve = new Card { Id = "reprieve", Type = CardType.Attachment, Controller = p1, AttachedTo = parent };
        p1.PlayArea.Add(parent);
        p1.PlayArea.Add(reprieve);

        var context = new AbilityContext { Game = game, Player = p1, Source = parent, Target = parent };
        new DiscardFromPlayGameActionHandler().Execute(context, null);

        Assert.That(p1.PlayArea, Does.Contain(parent), "the parent stays in play");
        Assert.That(p1.Discard, Does.Contain(reprieve), "reprieve is discarded instead");
    }
}
