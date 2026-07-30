using L5R.Engine.Dsl;
using L5R.Engine.State;

namespace L5R.Engine.Tests.Dsl;

public class SecretCacheFirerTests
{
    [Test]
    public void FireIfLegal_WhenDeclaredAgainstIt_TakesTheTopCard()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p2 };
        var province = new Card { Id = "secret-cache", Type = CardType.Province, Controller = p1 };
        var topCard = new Card { Id = "top-card", Type = CardType.Character, Controller = p1 };
        p1.Deck.Add(topCard);
        var conflict = new Conflict { AttackingPlayer = p2, DefendingPlayer = p1, DeclaredProvince = province };
        game.CurrentConflict = conflict;

        SecretCacheFirer.FireIfLegal(game, province);

        Assert.That(p1.Hand, Contains.Item(topCard));
        Assert.That(p1.Deck, Does.Not.Contain(topCard));
    }

    [Test]
    public void FireIfLegal_WhenDeclaredAgainstADifferentProvince_DoesNotFire()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p2 };
        var province = new Card { Id = "secret-cache", Type = CardType.Province, Controller = p1 };
        var otherProvince = new Card { Id = "other-province", Type = CardType.Province, Controller = p1 };
        var topCard = new Card { Id = "top-card", Type = CardType.Character, Controller = p1 };
        p1.Deck.Add(topCard);
        var conflict = new Conflict { AttackingPlayer = p2, DefendingPlayer = p1, DeclaredProvince = otherProvince };
        game.CurrentConflict = conflict;

        SecretCacheFirer.FireIfLegal(game, province);

        Assert.That(p1.Hand, Is.Empty);
    }
}
