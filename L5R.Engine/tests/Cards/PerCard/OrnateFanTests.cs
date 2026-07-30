using L5R.Engine.State;

namespace L5R.Engine.Tests.Cards.PerCard;

/// <summary>
/// Same generic attachment-bonus mechanism as fine-katana/kitsuki-s-method (political only),
/// plus the printed "restricted" keyword - its 2-per-character cap
/// (GameState.ExceedsRestrictedAttachmentLimit) is exercised via AncestralDaishoTests instead
/// of being duplicated here.
/// </summary>
public class OrnateFanTests
{
    [Test]
    public void WhileAttached_AddsItsPrintedPoliticalBonusToItsParent()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        var bearer = new Card { Id = "bearer", Type = CardType.Character, Controller = p1, PrintedPoliticalSkill = 2 };
        var fan = new Card
        {
            Id = "ornate-fan", Type = CardType.Attachment, Controller = p1, AttachedTo = bearer,
            PrintedMilitaryBonus = 0, PrintedPoliticalBonus = 2, PrintedKeywords = new[] { "restricted" }
        };
        p1.PlayArea.Add(bearer);
        p1.PlayArea.Add(fan);

        Assert.That(game.EffectivePoliticalSkill(bearer), Is.EqualTo(4));
        Assert.That(game.HasKeyword(fan, "restricted"), Is.True);
    }
}
