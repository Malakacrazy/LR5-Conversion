using L5R.Engine.Abilities;
using L5R.Engine.Cards.Scripts;
using L5R.Engine.State;

namespace L5R.Engine.Tests.Cards.PerCard;

public class KakitaAsamiTests
{
    [Test]
    public void AsAttacker_TakesHonorWhenAttackerSkillIsHigher()
    {
        var p1 = new Player { Name = "Player1", Honor = 3 };
        var p2 = new Player { Name = "Player2", Honor = 5 };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var asami = new Card { Id = "kakita-asami", Type = CardType.Character, Controller = p1 };
        p1.PlayArea.Add(asami);

        var conflict = new Conflict { AttackingPlayer = p1, DefendingPlayer = p2, ConflictType = "political", AttackerSkill = 5, DefenderSkill = 3 };
        game.CurrentConflict = conflict;

        var context = new AbilityContext { Game = game, Player = p1, Source = asami };

        new KakitaAsamiTakeHonorWhenWinningPoliticalSkill().Execute(context);

        Assert.That(p1.Honor, Is.EqualTo(4));
        Assert.That(p2.Honor, Is.EqualTo(4));
    }

    [Test]
    public void AsDefender_TakesHonorWhenAttackerSkillIsLower()
    {
        var p1 = new Player { Name = "Player1", Honor = 3 };
        var p2 = new Player { Name = "Player2", Honor = 5 };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var asami = new Card { Id = "kakita-asami", Type = CardType.Character, Controller = p1 };
        p1.PlayArea.Add(asami);

        var conflict = new Conflict { AttackingPlayer = p2, DefendingPlayer = p1, ConflictType = "political", AttackerSkill = 2, DefenderSkill = 5 };
        game.CurrentConflict = conflict;

        var context = new AbilityContext { Game = game, Player = p1, Source = asami };

        new KakitaAsamiTakeHonorWhenWinningPoliticalSkill().Execute(context);

        Assert.That(p1.Honor, Is.EqualTo(4));
    }

    [Test]
    public void AsAttackerWithLowerSkill_Throws()
    {
        var p1 = new Player { Name = "Player1", Honor = 3 };
        var p2 = new Player { Name = "Player2", Honor = 5 };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var asami = new Card { Id = "kakita-asami", Type = CardType.Character, Controller = p1 };
        p1.PlayArea.Add(asami);

        var conflict = new Conflict { AttackingPlayer = p1, DefendingPlayer = p2, ConflictType = "political", AttackerSkill = 2, DefenderSkill = 5 };
        game.CurrentConflict = conflict;

        var context = new AbilityContext { Game = game, Player = p1, Source = asami };

        Assert.Throws<InvalidOperationException>(() => new KakitaAsamiTakeHonorWhenWinningPoliticalSkill().Execute(context));
    }

    [Test]
    public void DuringAMilitaryConflict_Throws()
    {
        var p1 = new Player { Name = "Player1", Honor = 3 };
        var p2 = new Player { Name = "Player2", Honor = 5 };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var asami = new Card { Id = "kakita-asami", Type = CardType.Character, Controller = p1 };
        p1.PlayArea.Add(asami);

        var conflict = new Conflict { AttackingPlayer = p1, DefendingPlayer = p2, ConflictType = "military", AttackerSkill = 5, DefenderSkill = 3 };
        game.CurrentConflict = conflict;

        var context = new AbilityContext { Game = game, Player = p1, Source = asami };

        Assert.Throws<InvalidOperationException>(() => new KakitaAsamiTakeHonorWhenWinningPoliticalSkill().Execute(context));
    }
}
