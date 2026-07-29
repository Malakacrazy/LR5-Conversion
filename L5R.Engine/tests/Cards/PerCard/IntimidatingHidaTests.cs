using L5R.Engine.Abilities;
using L5R.Engine.Cards.Scripts;
using L5R.Engine.State;

namespace L5R.Engine.Tests.Cards.PerCard;

public class IntimidatingHidaTests
{
    [Test]
    public void WhenTheOpponentPassesAsAttacker_TheOpponentLosesOneHonor()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2", Honor = 5 };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var hida = new Card { Id = "intimidating-hida", Type = CardType.Character, Controller = p1 };
        p1.PlayArea.Add(hida);

        game.CurrentConflict = new Conflict { AttackingPlayer = p2, DefendingPlayer = p1 };

        var context = new AbilityContext { Game = game, Player = p1, Source = hida };

        new IntimidatingHidaLoseHonorOnOpponentPass().Execute(context);

        Assert.That(p2.Honor, Is.EqualTo(4));
    }

    [Test]
    public void WhenTheControllerWouldHaveBeenTheAttacker_Throws()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2", Honor = 5 };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var hida = new Card { Id = "intimidating-hida", Type = CardType.Character, Controller = p1 };
        p1.PlayArea.Add(hida);

        game.CurrentConflict = new Conflict { AttackingPlayer = p1, DefendingPlayer = p2 };

        var context = new AbilityContext { Game = game, Player = p1, Source = hida };

        Assert.Throws<InvalidOperationException>(() => new IntimidatingHidaLoseHonorOnOpponentPass().Execute(context));
        Assert.That(p2.Honor, Is.EqualTo(5), "nothing happened");
    }
}
