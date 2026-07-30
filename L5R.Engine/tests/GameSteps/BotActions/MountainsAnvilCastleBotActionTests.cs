using L5R.Engine.GameSteps.BotActions;
using L5R.Engine.State;

namespace L5R.Engine.Tests.GameSteps.BotActions;

public class MountainsAnvilCastleBotActionTests
{
    private static (GameState game, Card stronghold, Card participant) NewScenario(int printedMilitary = 3, int printedPolitical = 2)
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        var stronghold = new Card { Id = "mountain-s-anvil-castle", Type = CardType.Stronghold, Controller = p1 };
        p1.Stronghold = stronghold;

        var participant = new Card { Id = "participant", Type = CardType.Character, Controller = p1, PrintedMilitarySkill = printedMilitary, PrintedPoliticalSkill = printedPolitical };
        var attachment = new Card { Id = "attachment", Type = CardType.Attachment, Controller = p1, AttachedTo = participant };
        p1.PlayArea.Add(participant);
        p1.PlayArea.Add(attachment);

        var conflict = new Conflict { AttackingPlayer = p1, DefendingPlayer = p2 };
        conflict.Attackers.Add(participant);
        game.CurrentConflict = conflict;

        return (game, stronghold, participant);
    }

    [Test]
    public void IsLegal_WithAParticipantHavingAnAttachment_True()
    {
        var (game, stronghold, _) = NewScenario();
        Assert.That(new MountainsAnvilCastleBotAction().IsLegal(game, stronghold, game.Player1), Is.True);
    }

    [Test]
    public void IsLegal_WhenAlreadyBowed_False()
    {
        var (game, stronghold, _) = NewScenario();
        stronghold.Bowed = true;
        Assert.That(new MountainsAnvilCastleBotAction().IsLegal(game, stronghold, game.Player1), Is.False);
    }

    [Test]
    public void Invoke_BowsTheStrongholdAndGrantsPlusOneToBothSkills()
    {
        var (game, stronghold, participant) = NewScenario();

        new MountainsAnvilCastleBotAction().Invoke(game, stronghold, game.Player1);

        Assert.That(stronghold.Bowed, Is.True);
        Assert.That(game.EffectiveMilitarySkill(participant), Is.EqualTo(4));
        Assert.That(game.EffectivePoliticalSkill(participant), Is.EqualTo(3));
    }
}
