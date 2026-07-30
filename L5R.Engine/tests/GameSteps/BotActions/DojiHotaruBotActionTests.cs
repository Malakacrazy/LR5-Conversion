using L5R.Engine.GameSteps.BotActions;
using L5R.Engine.State;

namespace L5R.Engine.Tests.GameSteps.BotActions;

public class DojiHotaruBotActionTests
{
    private static GameState NewScenario(out Player p1, out Card hotaru)
    {
        p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };

        hotaru = new Card { Id = "doji-hotaru", Type = CardType.Character, Controller = p1 };
        p1.PlayArea.Add(hotaru);

        var conflict = new Conflict { AttackingPlayer = p1, DefendingPlayer = p2, ConflictType = "political", RingClaimedThisConflict = true };
        conflict.Attackers.Add(hotaru);
        conflict.Elements.Add("water");
        game.CurrentConflict = conflict;

        var ring = game.Rings.Find(r => r.Element == "water")!;
        ring.Claimed = true;
        ring.ClaimedBy = p1;

        return game;
    }

    [Test]
    public void IsLegal_AfterClaimingTheRingDuringAPoliticalConflict_True()
    {
        var game = NewScenario(out var p1, out var hotaru);

        Assert.That(new DojiHotaruBotAction().IsLegal(game, hotaru, p1), Is.True);
    }

    [Test]
    public void IsLegal_DuringAMilitaryConflict_False()
    {
        var game = NewScenario(out var p1, out var hotaru);
        game.CurrentConflict!.ConflictType = "military";

        Assert.That(new DojiHotaruBotAction().IsLegal(game, hotaru, p1), Is.False);
    }

    [Test]
    public void Invoke_ResolvesWithoutThrowing()
    {
        var game = NewScenario(out var p1, out var hotaru);

        Assert.DoesNotThrow(() => new DojiHotaruBotAction().Invoke(game, hotaru, p1));
    }
}
