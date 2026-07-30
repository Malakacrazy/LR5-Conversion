using L5R.Engine.Abilities;
using L5R.Engine.Dsl.GameActions;
using L5R.Engine.State;

namespace L5R.Engine.Tests.Cards.PerCard;

/// <summary>
/// A vanilla attachment - no "abilities" block and no printed stat bonus (unlike fine-katana/
/// ornate-fan/kitsuki-s-method); its entire game text is the "ancestral" and "restricted"
/// keywords. "Ancestral" (DiscardFromPlayGameActionHandler's cascade) and "restricted"
/// (GameState.ExceedsRestrictedAttachmentLimit) are both exercised below - this is their
/// natural home since ancestral-daisho is the one ported card carrying both.
/// </summary>
public class AncestralDaishoTests
{
    [Test]
    public void HasItsPrintedTraitsAndKeywords()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        var bearer = new Card { Id = "bearer", Type = CardType.Character, Controller = p1, PrintedMilitarySkill = 2 };
        var daisho = new Card
        {
            Id = "ancestral-daisho", Type = CardType.Attachment, Controller = p1, AttachedTo = bearer,
            Traits = new[] { "weapon" }, PrintedKeywords = new[] { "ancestral", "restricted" }, PrintedCost = 1
        };
        p1.PlayArea.Add(bearer);
        p1.PlayArea.Add(daisho);

        Assert.That(daisho.Traits, Is.EquivalentTo(new[] { "weapon" }));
        Assert.That(game.HasKeyword(daisho, "ancestral"), Is.True);
        Assert.That(game.HasKeyword(daisho, "restricted"), Is.True);
        Assert.That(game.EffectiveMilitarySkill(bearer), Is.EqualTo(2), "no printed stat bonus - unaffected");
    }

    [Test]
    public void WhenItsParentLeavesPlay_ItReturnsToHandInsteadOfTheDiscardPile()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        var bearer = new Card { Id = "bearer", Type = CardType.Character, Controller = p1 };
        var daisho = new Card { Id = "ancestral-daisho", Type = CardType.Attachment, Controller = p1, AttachedTo = bearer, PrintedKeywords = new[] { "ancestral", "restricted" } };
        p1.PlayArea.Add(bearer);
        p1.PlayArea.Add(daisho);

        var context = new AbilityContext { Game = game, Player = p1, Source = bearer, Target = bearer };
        new DiscardFromPlayGameActionHandler().Execute(context, null);

        Assert.That(p1.Hand, Contains.Item(daisho));
        Assert.That(p1.Discard, Does.Not.Contain(daisho));
        Assert.That(daisho.AttachedTo, Is.Null);
    }

    [Test]
    public void DiscardedDirectly_ItGoesToTheDiscardPileLikeAnyOtherAttachment()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        var bearer = new Card { Id = "bearer", Type = CardType.Character, Controller = p1 };
        var daisho = new Card { Id = "ancestral-daisho", Type = CardType.Attachment, Controller = p1, AttachedTo = bearer, PrintedKeywords = new[] { "ancestral", "restricted" } };
        p1.PlayArea.Add(bearer);
        p1.PlayArea.Add(daisho);

        var context = new AbilityContext { Game = game, Player = p1, Source = daisho, Target = daisho };
        new DiscardFromPlayGameActionHandler().Execute(context, null);

        Assert.That(p1.Discard, Contains.Item(daisho));
        Assert.That(p1.Hand, Does.Not.Contain(daisho), "ancestral only saves it when its parent leaves play, not when it's discarded on its own");
    }

    [Test]
    public void AThirdRestrictedAttachment_ExceedsTheLimit()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        var bearer = new Card { Id = "bearer", Type = CardType.Character, Controller = p1 };
        var first = new Card { Id = "fine-katana", Type = CardType.Attachment, Controller = p1, AttachedTo = bearer, PrintedKeywords = new[] { "restricted" } };
        var second = new Card { Id = "ornate-fan", Type = CardType.Attachment, Controller = p1, AttachedTo = bearer, PrintedKeywords = new[] { "restricted" } };
        var third = new Card { Id = "ancestral-daisho", Type = CardType.Attachment, Controller = p1, AttachedTo = bearer, PrintedKeywords = new[] { "ancestral", "restricted" } };
        p1.PlayArea.Add(bearer);
        p1.PlayArea.Add(first);
        p1.PlayArea.Add(second);
        p1.PlayArea.Add(third);

        Assert.That(game.ExceedsRestrictedAttachmentLimit(third), Is.True);
    }

    [Test]
    public void TwoRestrictedAttachments_DoesNotExceedTheLimit()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        var bearer = new Card { Id = "bearer", Type = CardType.Character, Controller = p1 };
        var first = new Card { Id = "fine-katana", Type = CardType.Attachment, Controller = p1, AttachedTo = bearer, PrintedKeywords = new[] { "restricted" } };
        var second = new Card { Id = "ancestral-daisho", Type = CardType.Attachment, Controller = p1, AttachedTo = bearer, PrintedKeywords = new[] { "ancestral", "restricted" } };
        p1.PlayArea.Add(bearer);
        p1.PlayArea.Add(first);
        p1.PlayArea.Add(second);

        Assert.That(game.ExceedsRestrictedAttachmentLimit(second), Is.False);
    }

    [Test]
    public void NonRestrictedAttachmentsDoNotCountTowardTheLimit()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        var bearer = new Card { Id = "bearer", Type = CardType.Character, Controller = p1 };
        var restrictedOne = new Card { Id = "fine-katana", Type = CardType.Attachment, Controller = p1, AttachedTo = bearer, PrintedKeywords = new[] { "restricted" } };
        var restrictedTwo = new Card { Id = "ornate-fan", Type = CardType.Attachment, Controller = p1, AttachedTo = bearer, PrintedKeywords = new[] { "restricted" } };
        var unrestricted = new Card { Id = "way-of-the-dragon", Type = CardType.Attachment, Controller = p1, AttachedTo = bearer };
        p1.PlayArea.Add(bearer);
        p1.PlayArea.Add(restrictedOne);
        p1.PlayArea.Add(restrictedTwo);
        p1.PlayArea.Add(unrestricted);

        Assert.That(game.ExceedsRestrictedAttachmentLimit(restrictedTwo), Is.False);
    }
}
