using L5R.Engine.Abilities;
using L5R.Engine.Cards.Scripts;
using L5R.Engine.State;

namespace L5R.Engine.Tests.Cards.PerCard;

public class AsceticVisionaryTests
{
    private static (GameState Game, Card Visionary) NewGameAttacking()
    {
        var p1 = new Player { Name = "Player1", Fate = 2 };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var visionary = new Card { Id = "ascetic-visionary", Type = CardType.Character, Controller = p1 };
        p1.PlayArea.Add(visionary);

        var conflict = new Conflict { AttackingPlayer = p1, DefendingPlayer = p2 };
        conflict.Attackers.Add(visionary);
        game.CurrentConflict = conflict;

        return (game, visionary);
    }

    [Test]
    public void PayingFateToAnUnclaimedRing_ReadiesAMonkCharacter()
    {
        var (game, visionary) = NewGameAttacking();
        var monk = new Card { Id = "a-monk", Type = CardType.Character, Controller = game.Player1, Traits = new[] { "monk" }, Bowed = true };
        game.Player1.PlayArea.Add(monk);
        var voidRing = game.Rings.Single(r => r.Element == "void");

        var context = new AbilityContext { Game = game, Player = game.Player1, Source = visionary, CostRingTarget = voidRing, Target = monk };

        new AsceticVisionaryReadyMonkOrMonkAttachmentHolder().Execute(context);

        Assert.That(game.Player1.Fate, Is.EqualTo(1));
        Assert.That(voidRing.Fate, Is.EqualTo(1));
        Assert.That(monk.Bowed, Is.False);
    }

    [Test]
    public void ReadiesACharacterWithAMonkAttachment()
    {
        var (game, visionary) = NewGameAttacking();
        var nonMonk = new Card { Id = "non-monk", Type = CardType.Character, Controller = game.Player1, Bowed = true };
        var monkAttachment = new Card { Id = "monk-attachment", Type = CardType.Attachment, Controller = game.Player1, Traits = new[] { "monk" }, AttachedTo = nonMonk };
        game.Player1.PlayArea.Add(nonMonk);
        game.Player1.PlayArea.Add(monkAttachment);
        var voidRing = game.Rings.Single(r => r.Element == "void");

        var context = new AbilityContext { Game = game, Player = game.Player1, Source = visionary, CostRingTarget = voidRing, Target = nonMonk };

        new AsceticVisionaryReadyMonkOrMonkAttachmentHolder().Execute(context);

        Assert.That(nonMonk.Bowed, Is.False);
    }

    [Test]
    public void ACharacterWithNoMonkConnection_Throws()
    {
        var (game, visionary) = NewGameAttacking();
        var nonMonk = new Card { Id = "non-monk", Type = CardType.Character, Controller = game.Player1, Bowed = true };
        game.Player1.PlayArea.Add(nonMonk);
        var voidRing = game.Rings.Single(r => r.Element == "void");

        var context = new AbilityContext { Game = game, Player = game.Player1, Source = visionary, CostRingTarget = voidRing, Target = nonMonk };

        Assert.Throws<InvalidOperationException>(() => new AsceticVisionaryReadyMonkOrMonkAttachmentHolder().Execute(context));
        Assert.That(game.Player1.Fate, Is.EqualTo(2), "the cost was never paid");
    }

    [Test]
    public void WhileNotAttacking_Throws()
    {
        var p1 = new Player { Name = "Player1", Fate = 2 };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var visionary = new Card { Id = "ascetic-visionary", Type = CardType.Character, Controller = p1 };
        var monk = new Card { Id = "a-monk", Type = CardType.Character, Controller = p1, Traits = new[] { "monk" }, Bowed = true };
        p1.PlayArea.Add(visionary);
        p1.PlayArea.Add(monk);

        var context = new AbilityContext { Game = game, Player = p1, Source = visionary, Target = monk };

        Assert.Throws<InvalidOperationException>(() => new AsceticVisionaryReadyMonkOrMonkAttachmentHolder().Execute(context));
    }
}
