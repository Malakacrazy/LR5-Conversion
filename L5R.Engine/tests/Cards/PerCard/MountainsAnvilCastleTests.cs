using L5R.Engine.State;

namespace L5R.Engine.Tests.Cards.PerCard;

/// <summary>
/// mountain-s-anvil-castle's own action is scriptOverride'd (its cardCondition/effect need
/// counting a candidate's own attachments and a min()-style computed value, neither of which
/// any vocabulary this engine has supports yet) - only its provisioning fields are reachable,
/// same "port only the reachable slice" convention as court-mask/way-of-the-dragon.
/// </summary>
public class MountainsAnvilCastleTests
{
    [Test]
    public void Provisions_StartingHonorAndFateIncomeAndStrengthBonus()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        var stronghold = new Card { Id = "mountain-s-anvil-castle", Type = CardType.Stronghold, Controller = p1, PrintedHonor = 11, PrintedFateIncome = 7, PrintedStrengthBonus = 2 };
        p1.Stronghold = stronghold;

        game.SetHonorFromStronghold(p1);

        Assert.That(p1.Honor, Is.EqualTo(11));
        Assert.That(game.FateIncomeFor(p1), Is.EqualTo(7));
        Assert.That(game.StrongholdStrengthBonusFor(p1), Is.EqualTo(2));
    }

    [Test]
    public void WithoutAStrongholdSet_ThrowsRatherThanSilentlyDoingNothing()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };

        Assert.Throws<InvalidOperationException>(() => game.SetHonorFromStronghold(p1));
        Assert.Throws<InvalidOperationException>(() => game.FateIncomeFor(p1));
        Assert.That(game.StrongholdStrengthBonusFor(p1), Is.EqualTo(0), "strengthBonus alone defaults rather than requiring a stronghold");
    }
}
