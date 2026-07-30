using L5R.Engine.State;

namespace L5R.Engine.Tests.Cards.PerCard;

/// <summary>
/// Same generic attachment-bonus mechanism as fine-katana/ornate-fan (political only here),
/// plus two printed keywords - "ancestral" (return-to-hand-on-parent-leaving-play: not yet
/// implemented anywhere in this engine, a separate gap from the stat bonus) and "restricted"
/// (the 2-per-character cap: also not yet implemented - GameState.ExceedsAttachmentLimit only
/// tracks a card's own declared attachmentLimit, e.g. way-of-the-dragon's "no second copy of
/// itself", not the broader "restricted" keyword's cross-card count). Only the printed-keyword
/// presence and the stat bonus are verified below; the other two rules need their own engine
/// support before they can be tested meaningfully.
/// </summary>
public class KitsukiSMethodTests
{
    [Test]
    public void WhileAttached_AddsItsPrintedPoliticalBonusToItsParent()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        var bearer = new Card { Id = "bearer", Type = CardType.Character, Controller = p1, PrintedMilitarySkill = 2, PrintedPoliticalSkill = 1 };
        var method = new Card
        {
            Id = "kitsuki-s-method", Type = CardType.Attachment, Controller = p1, AttachedTo = bearer,
            PrintedMilitaryBonus = 0, PrintedPoliticalBonus = 2, PrintedKeywords = new[] { "ancestral", "restricted" }
        };
        p1.PlayArea.Add(bearer);
        p1.PlayArea.Add(method);

        Assert.That(game.EffectivePoliticalSkill(bearer), Is.EqualTo(3));
        Assert.That(game.EffectiveMilitarySkill(bearer), Is.EqualTo(2), "militaryBonus is 0 - unaffected");
        Assert.That(game.HasKeyword(method, "ancestral"), Is.True);
        Assert.That(game.HasKeyword(method, "restricted"), Is.True);
    }
}
