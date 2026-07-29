using L5R.Engine.Abilities;
using L5R.Engine.Cards.Scripts;
using L5R.Engine.State;

namespace L5R.Engine.Tests.Cards.PerCard;

public class SecretCacheTests
{
    [Test]
    public void WhenTheConflictIsDeclaredAgainstIt_TakesAChosenCardToHand()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var secretCache = new Card { Id = "secret-cache", Type = CardType.Province, Controller = p1 };
        var topCard = new Card { Id = "top-card", Type = CardType.Character, Controller = p1 };
        p1.Deck.Add(topCard);

        var conflict = new Conflict { AttackingPlayer = p2, DefendingPlayer = p1, DeclaredProvince = secretCache };
        game.CurrentConflict = conflict;

        var context = new AbilityContext { Game = game, Player = p1, Source = secretCache, ChosenDeckSearchCard = topCard };

        new SecretCacheSearchTopFiveOnConflictDeclared().Execute(context);

        Assert.That(p1.Hand, Does.Contain(topCard));
    }

    [Test]
    public void WhenTheConflictIsDeclaredAgainstADifferentProvince_Throws()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var secretCache = new Card { Id = "secret-cache", Type = CardType.Province, Controller = p1 };
        var otherProvince = new Card { Id = "other-province", Type = CardType.Province, Controller = p1 };

        var conflict = new Conflict { AttackingPlayer = p2, DefendingPlayer = p1, DeclaredProvince = otherProvince };
        game.CurrentConflict = conflict;

        var context = new AbilityContext { Game = game, Player = p1, Source = secretCache };

        Assert.Throws<InvalidOperationException>(() => new SecretCacheSearchTopFiveOnConflictDeclared().Execute(context));
    }
}
