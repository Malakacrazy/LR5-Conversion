using L5R.Engine.Abilities;
using L5R.Engine.Cards.Scripts;
using L5R.Engine.State;

namespace L5R.Engine.Tests.Cards.PerCard;

public class TogashiKazueTests
{
    [Test]
    public void PlaysFromHandAsAnAttachment_LeavingItsPrintedTypeAsCharacter()
    {
        var p1 = new Player { Name = "Player1", Fate = 3 };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        var kazue = new Card { Id = "togashi-kazue", Type = CardType.Character, Controller = p1, PrintedCost = 3 };
        var host = new Card { Id = "host", Type = CardType.Character, Controller = p1 };
        p1.Hand.Add(kazue);
        p1.PlayArea.Add(host);

        var context = new AbilityContext { Game = game, Player = p1, Source = kazue, PlayAttachTarget = host };

        new TogashiKazuePlayAsAttachmentOrCharacter().Execute(context);

        Assert.That(kazue.AttachedTo, Is.EqualTo(host));
        Assert.That(kazue.Type, Is.EqualTo(CardType.Character));
    }

    [Test]
    public void WhileAttachedAndParentParticipating_StealsAFateFromAnotherParticipant()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        var host = new Card { Id = "host", Type = CardType.Character, Controller = p1, Fate = 0 };
        var kazue = new Card { Id = "togashi-kazue", Type = CardType.Character, Controller = p1, AttachedTo = host };
        var otherAttacker = new Card { Id = "other-attacker", Type = CardType.Character, Controller = p1, Fate = 2 };
        p1.PlayArea.Add(host);
        p1.PlayArea.Add(kazue);
        p1.PlayArea.Add(otherAttacker);

        var conflict = new Conflict { AttackingPlayer = p1, DefendingPlayer = p2 };
        conflict.Attackers.Add(host);
        conflict.Attackers.Add(otherAttacker);
        game.CurrentConflict = conflict;

        var context = new AbilityContext { Game = game, Player = p1, Source = kazue, Target = otherAttacker };

        new TogashiKazuePlayAsAttachmentOrCharacter().StealFate(context);

        Assert.That(otherAttacker.Fate, Is.EqualTo(1));
        Assert.That(host.Fate, Is.EqualTo(1));
    }

    [Test]
    public void WhenTargetHasNoFate_TakesNothingAndGivesTheParentNothing()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        var host = new Card { Id = "host", Type = CardType.Character, Controller = p1, Fate = 0 };
        var kazue = new Card { Id = "togashi-kazue", Type = CardType.Character, Controller = p1, AttachedTo = host };
        var otherAttacker = new Card { Id = "other-attacker", Type = CardType.Character, Controller = p1, Fate = 0 };
        p1.PlayArea.Add(host);
        p1.PlayArea.Add(kazue);
        p1.PlayArea.Add(otherAttacker);

        var conflict = new Conflict { AttackingPlayer = p1, DefendingPlayer = p2 };
        conflict.Attackers.Add(host);
        conflict.Attackers.Add(otherAttacker);
        game.CurrentConflict = conflict;

        var context = new AbilityContext { Game = game, Player = p1, Source = kazue, Target = otherAttacker };

        new TogashiKazuePlayAsAttachmentOrCharacter().StealFate(context);

        Assert.That(otherAttacker.Fate, Is.EqualTo(0));
        Assert.That(host.Fate, Is.EqualTo(0));
    }

    [Test]
    public void TargetingItsOwnParent_Throws()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        var host = new Card { Id = "host", Type = CardType.Character, Controller = p1, Fate = 2 };
        var kazue = new Card { Id = "togashi-kazue", Type = CardType.Character, Controller = p1, AttachedTo = host };
        p1.PlayArea.Add(host);
        p1.PlayArea.Add(kazue);

        var conflict = new Conflict { AttackingPlayer = p1, DefendingPlayer = p2 };
        conflict.Attackers.Add(host);
        game.CurrentConflict = conflict;

        var context = new AbilityContext { Game = game, Player = p1, Source = kazue, Target = host };

        Assert.Throws<InvalidOperationException>(() => new TogashiKazuePlayAsAttachmentOrCharacter().StealFate(context));
    }

    [Test]
    public void WhenParentIsNotParticipating_Throws()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        var host = new Card { Id = "host", Type = CardType.Character, Controller = p1, Fate = 0 };
        var kazue = new Card { Id = "togashi-kazue", Type = CardType.Character, Controller = p1, AttachedTo = host };
        var otherAttacker = new Card { Id = "other-attacker", Type = CardType.Character, Controller = p1, Fate = 2 };
        p1.PlayArea.Add(host);
        p1.PlayArea.Add(kazue);
        p1.PlayArea.Add(otherAttacker);

        var conflict = new Conflict { AttackingPlayer = p1, DefendingPlayer = p2 };
        conflict.Attackers.Add(otherAttacker);
        game.CurrentConflict = conflict;

        var context = new AbilityContext { Game = game, Player = p1, Source = kazue, Target = otherAttacker };

        Assert.Throws<InvalidOperationException>(() => new TogashiKazuePlayAsAttachmentOrCharacter().StealFate(context));
    }

    [Test]
    public void WhenNotAttached_Throws()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        var kazue = new Card { Id = "togashi-kazue", Type = CardType.Character, Controller = p1 };
        var otherAttacker = new Card { Id = "other-attacker", Type = CardType.Character, Controller = p1, Fate = 2 };
        p1.PlayArea.Add(kazue);
        p1.PlayArea.Add(otherAttacker);

        var context = new AbilityContext { Game = game, Player = p1, Source = kazue, Target = otherAttacker };

        Assert.Throws<InvalidOperationException>(() => new TogashiKazuePlayAsAttachmentOrCharacter().StealFate(context));
    }
}
