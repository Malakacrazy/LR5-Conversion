using L5R.Engine.Abilities;
using L5R.Engine.Cards.Scripts;
using L5R.Engine.State;

namespace L5R.Engine.Tests.Cards.PerCard;

public class LionsPrideBrawlerTests
{
    private static (GameState Game, Card Brawler) NewGameAttacking(int brawlerMilitarySkill)
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var brawler = new Card { Id = "lion-s-pride-brawler", Type = CardType.Character, Controller = p1, PrintedMilitarySkill = brawlerMilitarySkill };
        p1.PlayArea.Add(brawler);

        var conflict = new Conflict { AttackingPlayer = p1, DefendingPlayer = p2 };
        conflict.Attackers.Add(brawler);
        game.CurrentConflict = conflict;

        return (game, brawler);
    }

    [Test]
    public void WhileAttacking_BowsACharacterWithEqualOrLowerMilitarySkill()
    {
        var (game, brawler) = NewGameAttacking(brawlerMilitarySkill: 3);
        var target = new Card { Id = "target", Type = CardType.Character, Controller = game.Player2, PrintedMilitarySkill = 3 };
        game.Player2.PlayArea.Add(target);

        var context = new AbilityContext { Game = game, Player = game.Player1, Source = brawler, Target = target };

        new LionsPrideBrawlerBowLowerMilitarySkill().Execute(context);

        Assert.That(target.Bowed, Is.True);
    }

    [Test]
    public void ATargetWithHigherMilitarySkill_Throws()
    {
        var (game, brawler) = NewGameAttacking(brawlerMilitarySkill: 3);
        var target = new Card { Id = "target", Type = CardType.Character, Controller = game.Player2, PrintedMilitarySkill = 4 };
        game.Player2.PlayArea.Add(target);

        var context = new AbilityContext { Game = game, Player = game.Player1, Source = brawler, Target = target };

        Assert.Throws<InvalidOperationException>(() => new LionsPrideBrawlerBowLowerMilitarySkill().Execute(context));
    }

    [Test]
    public void WhileNotAttacking_Throws()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var brawler = new Card { Id = "lion-s-pride-brawler", Type = CardType.Character, Controller = p1, PrintedMilitarySkill = 3 };
        var target = new Card { Id = "target", Type = CardType.Character, Controller = p2, PrintedMilitarySkill = 1 };
        p1.PlayArea.Add(brawler);
        p2.PlayArea.Add(target);

        var context = new AbilityContext { Game = game, Player = p1, Source = brawler, Target = target };

        Assert.Throws<InvalidOperationException>(() => new LionsPrideBrawlerBowLowerMilitarySkill().Execute(context));
    }
}
