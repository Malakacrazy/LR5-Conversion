using System.Text.Json;
using L5R.Engine.Dsl;
using L5R.Engine.State;

namespace L5R.Engine.Tests.Cards.PerCard;

public class BornInWarTests
{
    private static IReadOnlyList<WhileAttachedDefinition> LoadWhileAttached()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Cards", "01-Core", "born-in-war.json");
        using var document = JsonDocument.Parse(File.ReadAllText(path));
        return AbilityDefinitionParser.ParseWhileAttached(document.RootElement);
    }

    [Test]
    public void WhileAttached_GivesMilitarySkillEqualToTheNumberOfUnclaimedRings()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        var rider = new Card { Id = "some-cavalry", Type = CardType.Character, Controller = p1, PrintedMilitarySkill = 1 };
        var attachment = new Card
        {
            Id = "born-in-war", Type = CardType.Attachment, Controller = p1,
            AttachedTo = rider, WhileAttachedEffects = LoadWhileAttached()
        };
        p1.PlayArea.Add(rider);
        p1.PlayArea.Add(attachment);

        // All 5 rings start unclaimed by default.
        Assert.That(game.EffectiveMilitarySkill(rider), Is.EqualTo(6));
    }

    [Test]
    public void AsRingsAreClaimed_TheBonusShrinks()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        game.Rings[0].Claimed = true;
        game.Rings[1].Contested = true;
        var rider = new Card { Id = "some-cavalry", Type = CardType.Character, Controller = p1, PrintedMilitarySkill = 1 };
        var attachment = new Card
        {
            Id = "born-in-war", Type = CardType.Attachment, Controller = p1,
            AttachedTo = rider, WhileAttachedEffects = LoadWhileAttached()
        };
        p1.PlayArea.Add(rider);
        p1.PlayArea.Add(attachment);

        Assert.That(game.EffectiveMilitarySkill(rider), Is.EqualTo(4), "1 printed + 3 remaining unclaimed rings");
    }

    [Test]
    public void WhenNotAttached_NoBonus()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        var rider = new Card { Id = "some-cavalry", Type = CardType.Character, Controller = p1, PrintedMilitarySkill = 1 };
        var attachment = new Card
        {
            Id = "born-in-war", Type = CardType.Attachment, Controller = p1,
            WhileAttachedEffects = LoadWhileAttached()
        };
        p1.PlayArea.Add(rider);
        p1.PlayArea.Add(attachment);

        Assert.That(game.EffectiveMilitarySkill(rider), Is.EqualTo(1));
    }
}
