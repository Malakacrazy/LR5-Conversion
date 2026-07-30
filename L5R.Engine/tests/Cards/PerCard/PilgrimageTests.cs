using L5R.Engine.Abilities;
using L5R.Engine.Cards.Scripts;
using L5R.Engine.Dsl.GameActions;
using L5R.Engine.State;

namespace L5R.Engine.Tests.Cards.PerCard;

public class PilgrimageTests
{
    [Test]
    public void PreventsTheRingFromResolvingForAConflictDeclaredAgainstIt()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var pilgrimage = new Card { Id = "pilgrimage", Type = CardType.Province, Controller = p1 };

        var conflict = new Conflict { AttackingPlayer = p2, DefendingPlayer = p1, DeclaredProvince = pilgrimage, Elements = new List<string> { "air" } };
        game.CurrentConflict = conflict;

        var context = new AbilityContext { Game = game, Player = p1, Source = pilgrimage };
        new PilgrimageCancelRingEffectsAtThisProvince().Execute(context);

        var resolveContext = new AbilityContext { Game = game, Player = p1, Source = pilgrimage, ChosenChoice = "Gain 2 Honor" };
        Assert.Throws<InvalidOperationException>(() => new ResolveConflictRingGameActionHandler().Execute(resolveContext, null));
        Assert.That(p1.Honor, Is.EqualTo(0));
    }

    [Test]
    public void WhenBroken_Throws()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var pilgrimage = new Card { Id = "pilgrimage", Type = CardType.Province, Controller = p1, Broken = true };

        var conflict = new Conflict { AttackingPlayer = p2, DefendingPlayer = p1, DeclaredProvince = pilgrimage };
        game.CurrentConflict = conflict;

        var context = new AbilityContext { Game = game, Player = p1, Source = pilgrimage };

        Assert.Throws<InvalidOperationException>(() => new PilgrimageCancelRingEffectsAtThisProvince().Execute(context));
    }

    [Test]
    public void WhenDeclaredAgainstADifferentProvince_Throws()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var pilgrimage = new Card { Id = "pilgrimage", Type = CardType.Province, Controller = p1 };
        var otherProvince = new Card { Id = "other-province", Type = CardType.Province, Controller = p1 };

        var conflict = new Conflict { AttackingPlayer = p2, DefendingPlayer = p1, DeclaredProvince = otherProvince };
        game.CurrentConflict = conflict;

        var context = new AbilityContext { Game = game, Player = p1, Source = pilgrimage };

        Assert.Throws<InvalidOperationException>(() => new PilgrimageCancelRingEffectsAtThisProvince().Execute(context));
    }
}
