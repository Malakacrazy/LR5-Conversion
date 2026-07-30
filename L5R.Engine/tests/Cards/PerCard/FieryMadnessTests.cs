using L5R.Engine.State;

namespace L5R.Engine.Tests.Cards.PerCard;

/// <summary>
/// A negative-stat "poison" attachment - same generic militaryBonus/politicalBonus mechanism
/// as fine-katana/ornate-fan/kitsuki-s-method/way-of-the-dragon, just with negative printed
/// values, confirming GameState.EffectiveStat's attachment-bonus scan isn't clamped at 0.
/// </summary>
public class FieryMadnessTests
{
    [Test]
    public void WhileAttached_SubtractsFromBothSkills()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        var bearer = new Card { Id = "bearer", Type = CardType.Character, Controller = p1, PrintedMilitarySkill = 3, PrintedPoliticalSkill = 3 };
        var madness = new Card
        {
            Id = "fiery-madness", Type = CardType.Attachment, Controller = p1, AttachedTo = bearer,
            PrintedMilitaryBonus = -2, PrintedPoliticalBonus = -2
        };
        p1.PlayArea.Add(bearer);
        p1.PlayArea.Add(madness);

        Assert.That(game.EffectiveMilitarySkill(bearer), Is.EqualTo(1));
        Assert.That(game.EffectivePoliticalSkill(bearer), Is.EqualTo(1));
    }

    [Test]
    public void CanDriveASkillNegative()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        var bearer = new Card { Id = "bearer", Type = CardType.Character, Controller = p1, PrintedMilitarySkill = 1, PrintedPoliticalSkill = 0 };
        var madness = new Card
        {
            Id = "fiery-madness", Type = CardType.Attachment, Controller = p1, AttachedTo = bearer,
            PrintedMilitaryBonus = -2, PrintedPoliticalBonus = -2
        };
        p1.PlayArea.Add(bearer);
        p1.PlayArea.Add(madness);

        Assert.That(game.EffectiveMilitarySkill(bearer), Is.EqualTo(-1));
        Assert.That(game.EffectivePoliticalSkill(bearer), Is.EqualTo(-2));
    }
}
