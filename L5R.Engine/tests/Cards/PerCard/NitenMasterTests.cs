using L5R.Engine.Abilities;
using L5R.Engine.Cards.Scripts;
using L5R.Engine.State;

namespace L5R.Engine.Tests.Cards.PerCard;

public class NitenMasterTests
{
    [Test]
    public void WhenAFriendlyWeaponIsAttached_ReadiesItself()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        var nitenMaster = new Card { Id = "niten-master", Type = CardType.Character, Controller = p1, Bowed = true };
        var weapon = new Card { Id = "a-weapon", Type = CardType.Attachment, Controller = p1, Traits = new[] { "weapon" }, AttachedTo = nitenMaster };
        p1.PlayArea.Add(nitenMaster);
        p1.PlayArea.Add(weapon);

        var context = new AbilityContext { Game = game, Player = p1, Source = nitenMaster, Target = weapon };

        new NitenMasterReadyOnWeaponAttached().Execute(context);

        Assert.That(nitenMaster.Bowed, Is.False);
    }

    [Test]
    public void ANonWeaponAttachment_Throws()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        var nitenMaster = new Card { Id = "niten-master", Type = CardType.Character, Controller = p1, Bowed = true };
        var nonWeapon = new Card { Id = "not-a-weapon", Type = CardType.Attachment, Controller = p1, AttachedTo = nitenMaster };
        p1.PlayArea.Add(nitenMaster);
        p1.PlayArea.Add(nonWeapon);

        var context = new AbilityContext { Game = game, Player = p1, Source = nitenMaster, Target = nonWeapon };

        Assert.Throws<InvalidOperationException>(() => new NitenMasterReadyOnWeaponAttached().Execute(context));
        Assert.That(nitenMaster.Bowed, Is.True);
    }

    [Test]
    public void AWeaponAttachedToAnotherCharacter_Throws()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        var nitenMaster = new Card { Id = "niten-master", Type = CardType.Character, Controller = p1, Bowed = true };
        var otherCharacter = new Card { Id = "other-character", Type = CardType.Character, Controller = p1 };
        var weapon = new Card { Id = "a-weapon", Type = CardType.Attachment, Controller = p1, Traits = new[] { "weapon" }, AttachedTo = otherCharacter };
        p1.PlayArea.Add(nitenMaster);
        p1.PlayArea.Add(otherCharacter);
        p1.PlayArea.Add(weapon);

        var context = new AbilityContext { Game = game, Player = p1, Source = nitenMaster, Target = weapon };

        Assert.Throws<InvalidOperationException>(() => new NitenMasterReadyOnWeaponAttached().Execute(context));
    }

    [Test]
    public void AnOpponentControlledWeapon_Throws()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        var nitenMaster = new Card { Id = "niten-master", Type = CardType.Character, Controller = p1, Bowed = true };
        var weapon = new Card { Id = "a-weapon", Type = CardType.Attachment, Controller = p2, Traits = new[] { "weapon" }, AttachedTo = nitenMaster };
        p1.PlayArea.Add(nitenMaster);
        p2.PlayArea.Add(weapon);

        var context = new AbilityContext { Game = game, Player = p1, Source = nitenMaster, Target = weapon };

        Assert.Throws<InvalidOperationException>(() => new NitenMasterReadyOnWeaponAttached().Execute(context));
    }
}
