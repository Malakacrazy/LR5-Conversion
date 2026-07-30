using L5R.Engine.Abilities;
using L5R.Engine.Cards.Scripts;
using L5R.Engine.Dsl;
using L5R.Engine.State;

namespace L5R.Engine.Tests.Cards.PerCard;

public class TogashiYokuniTests
{
    private static ActionDefinition SyntheticGainFateAction() =>
        new("Some character's ability", Array.Empty<CostDefinition>(), null, new[] { new GameActionDefinition("gainFate", null) }, null, null);

    [Test]
    public void CopyingAnotherCharactersAbility_RecordsTheGrant()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        var yokuni = new Card { Id = "togashi-yokuni", Type = CardType.Character, Controller = p1 };
        var otherCharacter = new Card { Id = "other-character", Type = CardType.Character, Controller = p2 };
        p1.PlayArea.Add(yokuni);
        p2.PlayArea.Add(otherCharacter);

        var ability = SyntheticGainFateAction();
        var context = new AbilityContext { Game = game, Player = p1, Source = yokuni, Target = otherCharacter, ChosenAbility = ability };

        new TogashiYokuniCopyAnotherCharactersAbility().Execute(context);

        Assert.That(game.GainedAbilities, Does.Contain((yokuni, ability)));
    }

    [Test]
    public void TheCopiedAbilityCanActuallyBeUsedFromYokuniAsSource()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        var yokuni = new Card { Id = "togashi-yokuni", Type = CardType.Character, Controller = p1 };
        var otherCharacter = new Card { Id = "other-character", Type = CardType.Character, Controller = p2 };
        p1.PlayArea.Add(yokuni);
        p2.PlayArea.Add(otherCharacter);

        var ability = SyntheticGainFateAction();
        var context = new AbilityContext { Game = game, Player = p1, Source = yokuni, Target = otherCharacter, ChosenAbility = ability };
        new TogashiYokuniCopyAnotherCharactersAbility().Execute(context);

        var (_, grantedAbility) = game.GainedAbilities.Single();
        var executor = new AbilityExecutor(new CostRegistry(), new GameActionRegistry());
        executor.Execute(grantedAbility, new AbilityContext { Game = game, Player = p1, Source = yokuni });

        Assert.That(p1.Fate, Is.EqualTo(1));
    }

    [Test]
    public void WhenThePhaseAdvances_TheGrantExpires()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var yokuni = new Card { Id = "togashi-yokuni", Type = CardType.Character, Controller = p1 };
        var otherCharacter = new Card { Id = "other-character", Type = CardType.Character, Controller = p2 };
        p1.PlayArea.Add(yokuni);
        p2.PlayArea.Add(otherCharacter);

        var context = new AbilityContext { Game = game, Player = p1, Source = yokuni, Target = otherCharacter, ChosenAbility = SyntheticGainFateAction() };
        new TogashiYokuniCopyAnotherCharactersAbility().Execute(context);

        game.AdvancePhase();

        Assert.That(game.GainedAbilities, Is.Empty);
    }

    [Test]
    public void CopyingItsOwnAbility_Throws()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        var yokuni = new Card { Id = "togashi-yokuni", Type = CardType.Character, Controller = p1 };
        p1.PlayArea.Add(yokuni);

        var context = new AbilityContext { Game = game, Player = p1, Source = yokuni, Target = yokuni, ChosenAbility = SyntheticGainFateAction() };

        Assert.Throws<InvalidOperationException>(() => new TogashiYokuniCopyAnotherCharactersAbility().Execute(context));
    }
}
