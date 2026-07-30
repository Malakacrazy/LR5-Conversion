using L5R.Engine.GameSteps.BotActions;
using L5R.Engine.State;

namespace L5R.Engine.Tests.GameSteps.BotActions;

public class NitenMasterBotActionTests
{
    private static GameState NewScenario(out Player p1, out Card nitenMaster, out Card weapon)
    {
        p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };

        nitenMaster = new Card { Id = "niten-master", Type = CardType.Character, Controller = p1, Bowed = true };
        weapon = new Card { Id = "a-weapon", Type = CardType.Attachment, Controller = p1, AttachedTo = nitenMaster, Traits = new[] { "weapon" } };
        p1.PlayArea.Add(nitenMaster);
        p1.PlayArea.Add(weapon);

        return game;
    }

    [Test]
    public void IsLegal_WhenBowedWithAnOwnWeaponAttached_True()
    {
        var game = NewScenario(out var p1, out var nitenMaster, out _);

        Assert.That(new NitenMasterBotAction().IsLegal(game, nitenMaster, p1), Is.True);
    }

    [Test]
    public void IsLegal_WhenAlreadyReady_False()
    {
        var game = NewScenario(out var p1, out var nitenMaster, out _);
        nitenMaster.Bowed = false;

        Assert.That(new NitenMasterBotAction().IsLegal(game, nitenMaster, p1), Is.False);
    }

    [Test]
    public void Invoke_ReadiesItself()
    {
        var game = NewScenario(out var p1, out var nitenMaster, out _);

        new NitenMasterBotAction().Invoke(game, nitenMaster, p1);

        Assert.That(nitenMaster.Bowed, Is.False);
    }
}
