using L5R.Engine.Abilities;
using L5R.Engine.Cards.Scripts;
using L5R.Engine.State;

namespace L5R.Engine.Tests.Cards.PerCard;

public class YoungRumormongerTests
{
    [Test]
    public void RedirectsHonorToADifferentCharacterWithTheSameController()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        var rumormonger = new Card { Id = "young-rumormonger", Type = CardType.Character, Controller = p1 };
        var originalTarget = new Card { Id = "original-target", Type = CardType.Character, Controller = p2 };
        var newTarget = new Card { Id = "new-target", Type = CardType.Character, Controller = p2 };
        p1.PlayArea.Add(rumormonger);
        p2.PlayArea.Add(originalTarget);
        p2.PlayArea.Add(newTarget);

        var context = new AbilityContext
        {
            Game = game, Player = p1, Source = rumormonger, Target = originalTarget,
            SecondTarget = newTarget, ChosenChoice = "Honor"
        };

        new YoungRumormongerRedirectHonorOrDishonor().Execute(context);

        Assert.That(newTarget.IsHonored, Is.True);
        Assert.That(originalTarget.IsHonored, Is.False, "the original target is untouched");
    }

    [Test]
    public void RedirectsDishonorToADifferentCharacterWithTheSameController()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        var rumormonger = new Card { Id = "young-rumormonger", Type = CardType.Character, Controller = p1 };
        var originalTarget = new Card { Id = "original-target", Type = CardType.Character, Controller = p2 };
        var newTarget = new Card { Id = "new-target", Type = CardType.Character, Controller = p2 };
        p1.PlayArea.Add(rumormonger);
        p2.PlayArea.Add(originalTarget);
        p2.PlayArea.Add(newTarget);

        var context = new AbilityContext
        {
            Game = game, Player = p1, Source = rumormonger, Target = originalTarget,
            SecondTarget = newTarget, ChosenChoice = "Dishonor"
        };

        new YoungRumormongerRedirectHonorOrDishonor().Execute(context);

        Assert.That(newTarget.IsDishonored, Is.True);
    }

    [Test]
    public void RedirectingToACharacterWithADifferentController_Throws()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        var rumormonger = new Card { Id = "young-rumormonger", Type = CardType.Character, Controller = p1 };
        var originalTarget = new Card { Id = "original-target", Type = CardType.Character, Controller = p2 };
        var wrongController = new Card { Id = "wrong-controller", Type = CardType.Character, Controller = p1 };
        p1.PlayArea.Add(rumormonger);
        p1.PlayArea.Add(wrongController);
        p2.PlayArea.Add(originalTarget);

        var context = new AbilityContext
        {
            Game = game, Player = p1, Source = rumormonger, Target = originalTarget,
            SecondTarget = wrongController, ChosenChoice = "Honor"
        };

        Assert.Throws<InvalidOperationException>(() => new YoungRumormongerRedirectHonorOrDishonor().Execute(context));
    }

    [Test]
    public void RedirectingToTheSameCharacter_Throws()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        var rumormonger = new Card { Id = "young-rumormonger", Type = CardType.Character, Controller = p1 };
        var originalTarget = new Card { Id = "original-target", Type = CardType.Character, Controller = p2 };
        p1.PlayArea.Add(rumormonger);
        p2.PlayArea.Add(originalTarget);

        var context = new AbilityContext
        {
            Game = game, Player = p1, Source = rumormonger, Target = originalTarget,
            SecondTarget = originalTarget, ChosenChoice = "Honor"
        };

        Assert.Throws<InvalidOperationException>(() => new YoungRumormongerRedirectHonorOrDishonor().Execute(context));
    }
}
