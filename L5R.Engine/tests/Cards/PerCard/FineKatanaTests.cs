using L5R.Engine.State;

namespace L5R.Engine.Tests.Cards.PerCard;

/// <summary>
/// A pure stats-and-keywords attachment (no "abilities" block) - its entire game text is the
/// printed +2 military bonus (card-schema.json's "militaryBonus", applied automatically by
/// GameState.EffectiveStat's attachment-bonus scan) plus the printed "restricted" keyword
/// (GameState.HasKeyword, now checking Card.PrintedKeywords too). No scriptOverride needed;
/// nothing here required per-card logic once those two generic mechanics existed.
/// </summary>
public class FineKatanaTests
{
    [Test]
    public void WhileAttached_AddsItsPrintedMilitaryBonusToItsParent()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        var bearer = new Card { Id = "bearer", Type = CardType.Character, Controller = p1, PrintedMilitarySkill = 2, PrintedPoliticalSkill = 1 };
        var katana = new Card
        {
            Id = "fine-katana", Type = CardType.Attachment, Controller = p1, AttachedTo = bearer,
            PrintedMilitaryBonus = 2, PrintedPoliticalBonus = 0, PrintedKeywords = new[] { "restricted" }
        };
        p1.PlayArea.Add(bearer);
        p1.PlayArea.Add(katana);

        Assert.That(game.EffectiveMilitarySkill(bearer), Is.EqualTo(4));
        Assert.That(game.EffectivePoliticalSkill(bearer), Is.EqualTo(1), "politicalBonus is 0 - unaffected");
        Assert.That(game.HasKeyword(katana, "restricted"), Is.True);
    }

    [Test]
    public void Unattached_ContributesNothing()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        var bearer = new Card { Id = "bearer", Type = CardType.Character, Controller = p1, PrintedMilitarySkill = 2 };
        p1.PlayArea.Add(bearer);

        Assert.That(game.EffectiveMilitarySkill(bearer), Is.EqualTo(2));
    }
}
