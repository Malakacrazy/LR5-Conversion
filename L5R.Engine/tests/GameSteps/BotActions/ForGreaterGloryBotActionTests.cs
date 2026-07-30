using L5R.Engine.GameSteps.BotActions;
using L5R.Engine.State;

namespace L5R.Engine.Tests.GameSteps.BotActions;

public class ForGreaterGloryBotActionTests
{
    private static (GameState game, Card fgg, Card bushi) NewScenario(bool provinceBroken = true)
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        var fgg = new Card { Id = "for-greater-glory", Type = CardType.Event, Controller = p1 };
        var bushi = new Card { Id = "bushi-ally", Type = CardType.Character, Controller = p1, Traits = new[] { "bushi" } };
        p1.PlayArea.Add(bushi);
        var province = new Card { Id = "province", Type = CardType.Province, Controller = p2, Broken = provinceBroken };
        var conflict = new Conflict { AttackingPlayer = p1, DefendingPlayer = p2, ConflictType = "military", DeclaredProvince = province };
        conflict.Attackers.Add(bushi);
        game.CurrentConflict = conflict;
        return (game, fgg, bushi);
    }

    [Test]
    public void IsLegal_AsAttackerAfterBreakingAProvince_True()
    {
        var (game, fgg, _) = NewScenario();
        Assert.That(new ForGreaterGloryBotAction().IsLegal(game, fgg, game.Player1), Is.True);
    }

    [Test]
    public void IsLegal_WhenTheProvinceDidNotBreak_False()
    {
        var (game, fgg, _) = NewScenario(provinceBroken: false);
        Assert.That(new ForGreaterGloryBotAction().IsLegal(game, fgg, game.Player1), Is.False);
    }

    [Test]
    public void Invoke_PlacesOneFateOnEachOwnBushiParticipant()
    {
        var (game, fgg, bushi) = NewScenario();
        new ForGreaterGloryBotAction().Invoke(game, fgg, game.Player1);
        Assert.That(bushi.Fate, Is.EqualTo(1));
    }
}
