using L5R.Engine.State;

namespace L5R.Engine.Tests.Cards.PerCard;

/// <summary>
/// A vanilla attachment - no "abilities" block and no printed stat bonus (unlike fine-katana/
/// ornate-fan/kitsuki-s-method); its entire game text is the "ancestral" and "restricted"
/// keywords. Neither rule is implemented anywhere yet - "ancestral" (return this card to hand
/// when the attached character leaves play) needs an on-leaves-play reaction hook nothing
/// currently drives, and "restricted" (max 2 restricted attachments per character) needs a
/// cross-card keyword count GameState.ExceedsAttachmentLimit doesn't do (see
/// KitsukiSMethodTests' own doc comment). Only printed-keyword presence is verified below.
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
}
