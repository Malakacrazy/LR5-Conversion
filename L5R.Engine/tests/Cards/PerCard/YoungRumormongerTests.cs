using L5R.Engine.Abilities;
using L5R.Engine.Cards.Scripts;
using L5R.Engine.Dsl.GameActions;
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

    // The tests below exercise HonorGameActionHandler/DishonorGameActionHandler directly
    // (not the script) - proving young-rumormonger is wired in automatically as a
    // replacement effect, without any caller needing to know it exists.

    [Test]
    public void HonorGameActionHandler_WithARumormongerInPlay_RedirectsAutomatically()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        var rumormonger = new Card { Id = "young-rumormonger", Type = CardType.Character, Controller = p1 };
        var originalTarget = new Card { Id = "original-target", Type = CardType.Character, Controller = p2 };
        var otherCharacter = new Card { Id = "other-character", Type = CardType.Character, Controller = p2 };
        p1.PlayArea.Add(rumormonger);
        p2.PlayArea.Add(originalTarget);
        p2.PlayArea.Add(otherCharacter);

        var context = new AbilityContext { Game = game, Player = p2, Source = originalTarget, Target = originalTarget };
        new HonorGameActionHandler().Execute(context, null);

        Assert.That(originalTarget.IsHonored, Is.False, "redirected away");
        Assert.That(otherCharacter.IsHonored, Is.True);
    }

    [Test]
    public void DishonorGameActionHandler_WithARumormongerInPlay_RedirectsAutomatically()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        var rumormonger = new Card { Id = "young-rumormonger", Type = CardType.Character, Controller = p1 };
        var originalTarget = new Card { Id = "original-target", Type = CardType.Character, Controller = p2 };
        var otherCharacter = new Card { Id = "other-character", Type = CardType.Character, Controller = p2 };
        p1.PlayArea.Add(rumormonger);
        p2.PlayArea.Add(originalTarget);
        p2.PlayArea.Add(otherCharacter);

        var context = new AbilityContext { Game = game, Player = p2, Source = originalTarget, Target = originalTarget };
        new DishonorGameActionHandler().Execute(context, null);

        Assert.That(originalTarget.IsDishonored, Is.False, "redirected away");
        Assert.That(otherCharacter.IsDishonored, Is.True);
    }

    [Test]
    public void HonorGameActionHandler_WithNoOtherCharacterToRedirectTo_HonorsTheOriginalTarget()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        var rumormonger = new Card { Id = "young-rumormonger", Type = CardType.Character, Controller = p1 };
        var originalTarget = new Card { Id = "original-target", Type = CardType.Character, Controller = p2 };
        p1.PlayArea.Add(rumormonger);
        p2.PlayArea.Add(originalTarget);

        var context = new AbilityContext { Game = game, Player = p2, Source = originalTarget, Target = originalTarget };
        new HonorGameActionHandler().Execute(context, null);

        Assert.That(originalTarget.IsHonored, Is.True, "no legal redirect target exists");
    }

    [Test]
    public void HonorGameActionHandler_WithExactlyTwoCharacters_RedirectsOnceWithoutPingPonging()
    {
        // Guards against the recursive redirect calling back into itself: with only two
        // characters for the target's controller, a naive implementation would redirect
        // A -> B, then see the same condition holds for B and redirect B -> A forever.
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        var rumormonger = new Card { Id = "young-rumormonger", Type = CardType.Character, Controller = p1 };
        var characterA = new Card { Id = "character-a", Type = CardType.Character, Controller = p2 };
        var characterB = new Card { Id = "character-b", Type = CardType.Character, Controller = p2 };
        p1.PlayArea.Add(rumormonger);
        p2.PlayArea.Add(characterA);
        p2.PlayArea.Add(characterB);

        var context = new AbilityContext { Game = game, Player = p2, Source = characterA, Target = characterA };

        Assert.DoesNotThrow(() => new HonorGameActionHandler().Execute(context, null));
        Assert.That(characterB.IsHonored, Is.True);
        Assert.That(characterA.IsHonored, Is.False);
    }
}
