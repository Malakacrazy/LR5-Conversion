using L5R.Engine.Abilities;
using L5R.Engine.Cards.Scripts;
using L5R.Engine.State;

namespace L5R.Engine.Tests.Cards.PerCard;

public class FallenInBattleTests
{
    private static (GameState Game, Card Source, Card Target) NewGameWonDecisivelyByMilitary()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var source = new Card { Id = "fallen-in-battle", Type = CardType.Event, Controller = p1 };
        var target = new Card { Id = "participant", Type = CardType.Character, Controller = p2 };
        p2.PlayArea.Add(target);

        var conflict = new Conflict { AttackingPlayer = p1, DefendingPlayer = p2, ConflictType = "military", Winner = p1, SkillDifference = 5 };
        conflict.Defenders.Add(target);
        game.CurrentConflict = conflict;

        return (game, source, target);
    }

    [Test]
    public void AfterADecisiveMilitaryWin_DiscardsAParticipatingCharacter()
    {
        var (game, source, target) = NewGameWonDecisivelyByMilitary();

        var context = new AbilityContext { Game = game, Player = game.Player1, Source = source, Target = target };

        new FallenInBattleDiscardOnDecisiveMilitaryWin().Execute(context);

        Assert.That(game.Player2.Discard, Does.Contain(target));
    }

    [Test]
    public void WithLessThanFiveSkillDifference_Throws()
    {
        var (game, source, target) = NewGameWonDecisivelyByMilitary();
        game.CurrentConflict!.SkillDifference = 4;

        var context = new AbilityContext { Game = game, Player = game.Player1, Source = source, Target = target };

        Assert.Throws<InvalidOperationException>(() => new FallenInBattleDiscardOnDecisiveMilitaryWin().Execute(context));
    }

    [Test]
    public void AfterAPoliticalWin_Throws()
    {
        var (game, source, target) = NewGameWonDecisivelyByMilitary();
        game.CurrentConflict!.ConflictType = "political";

        var context = new AbilityContext { Game = game, Player = game.Player1, Source = source, Target = target };

        Assert.Throws<InvalidOperationException>(() => new FallenInBattleDiscardOnDecisiveMilitaryWin().Execute(context));
    }

    [Test]
    public void ANonParticipatingCharacter_Throws()
    {
        var (game, source, _) = NewGameWonDecisivelyByMilitary();
        var homeCharacter = new Card { Id = "home-character", Type = CardType.Character, Controller = game.Player2 };
        game.Player2.PlayArea.Add(homeCharacter);

        var context = new AbilityContext { Game = game, Player = game.Player1, Source = source, Target = homeCharacter };

        Assert.Throws<InvalidOperationException>(() => new FallenInBattleDiscardOnDecisiveMilitaryWin().Execute(context));
    }
}
