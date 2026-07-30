using L5R.Engine.GameSteps;
using L5R.Engine.State;

namespace L5R.Engine.Tests.GameSteps;

public class DuelResolverTests
{
    private static (Player p1, Player p2, Card challenger, Card target) NewDuelists(int challengerSkill, int challengerBid, int targetSkill, int targetBid)
    {
        var p1 = new Player { Name = "Player1", ShowBid = challengerBid };
        var p2 = new Player { Name = "Player2", ShowBid = targetBid };
        var challenger = new Card { Id = "challenger", Type = CardType.Character, Controller = p1, PrintedMilitarySkill = challengerSkill };
        var target = new Card { Id = "target", Type = CardType.Character, Controller = p2, PrintedMilitarySkill = targetSkill };
        return (p1, p2, challenger, target);
    }

    [Test]
    public void Resolve_HigherSkillPlusBidWins()
    {
        var (p1, p2, challenger, target) = NewDuelists(challengerSkill: 5, challengerBid: 1, targetSkill: 3, targetBid: 1);
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };

        Assert.That(DuelResolver.Resolve(game, challenger, target), Is.EqualTo(challenger));
    }

    [Test]
    public void Resolve_HonorBidCanFlipASkillDeficit()
    {
        var (p1, p2, challenger, target) = NewDuelists(challengerSkill: 2, challengerBid: 5, targetSkill: 3, targetBid: 0);
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };

        Assert.That(DuelResolver.Resolve(game, challenger, target), Is.EqualTo(challenger));
    }

    [Test]
    public void Resolve_TiesFavorTheChallenger()
    {
        var (p1, p2, challenger, target) = NewDuelists(challengerSkill: 3, challengerBid: 0, targetSkill: 3, targetBid: 0);
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };

        Assert.That(DuelResolver.Resolve(game, challenger, target), Is.EqualTo(challenger));
    }
}
