using System.Text.Json;
using L5R.Engine.Abilities;
using L5R.Engine.Dsl;
using L5R.Engine.Dsl.GameActions;
using L5R.Engine.State;

namespace L5R.Engine.Tests.Cards.PerCard;

/// <summary>
/// Only way-of-the-dragon's persistentEffects block (attachmentLimit/attachmentMyControlOnly)
/// is exercised here - its whileAttached "increaseLimitOnAbilities" needs an ability-use-limit
/// tracking subsystem this engine doesn't have yet (no ported card's "limit" field has ever
/// been enforced), same convention as court-mask/favored-mount only testing the inverse slice
/// of their own shared attachmentMyControlOnly declaration.
/// </summary>
public class WayOfTheDragonTests
{
    private static IReadOnlyList<PersistentEffectDefinition> LoadPersistentEffects()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Cards", "01-Core", "way-of-the-dragon.json");
        using var document = JsonDocument.Parse(File.ReadAllText(path));
        return AbilityDefinitionParser.ParsePersistentEffects(document.RootElement);
    }

    [Test]
    public void ASecondCopyOnTheSameCharacter_ExceedsTheAttachmentLimit()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        var bearer = new Card { Id = "bearer", Type = CardType.Character, Controller = p1 };
        var first = new Card { Id = "way-of-the-dragon", Type = CardType.Attachment, Controller = p1, AttachedTo = bearer, PersistentEffects = LoadPersistentEffects() };
        var second = new Card { Id = "way-of-the-dragon", Type = CardType.Attachment, Controller = p1, AttachedTo = bearer, PersistentEffects = LoadPersistentEffects() };
        p1.PlayArea.Add(bearer);
        p1.PlayArea.Add(first);
        p1.PlayArea.Add(second);

        Assert.That(game.ExceedsAttachmentLimit(second), Is.True);
    }

    [Test]
    public void ASingleCopy_DoesNotExceedTheAttachmentLimit()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        var bearer = new Card { Id = "bearer", Type = CardType.Character, Controller = p1 };
        var dragon = new Card { Id = "way-of-the-dragon", Type = CardType.Attachment, Controller = p1, AttachedTo = bearer, PersistentEffects = LoadPersistentEffects() };
        p1.PlayArea.Add(bearer);
        p1.PlayArea.Add(dragon);

        Assert.That(game.ExceedsAttachmentLimit(dragon), Is.False);
    }

    [Test]
    public void PlayingItOntoAnOpponentsCharacter_IsRestricted()
    {
        var p1 = new Player { Name = "Player1", Fate = 2 };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        var opponentCharacter = new Card { Id = "opponent-character", Type = CardType.Character, Controller = p2 };
        var dragon = new Card { Id = "way-of-the-dragon", Type = CardType.Attachment, Controller = p1, PrintedCost = 2, Location = "hand", PersistentEffects = LoadPersistentEffects() };
        p1.Hand.Add(dragon);
        p2.PlayArea.Add(opponentCharacter);

        var context = new AbilityContext { Game = game, Player = p1, Source = dragon, Target = dragon, PlayAttachTarget = opponentCharacter };

        Assert.Throws<InvalidOperationException>(() => new PlayCardGameActionHandler().Execute(context, null));
        Assert.That(dragon.AttachedTo, Is.Null, "the attach never happened");
    }

    [Test]
    public void PlayingItOntoYourOwnCharacter_IsAllowed()
    {
        var p1 = new Player { Name = "Player1", Fate = 2 };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        var ownCharacter = new Card { Id = "own-character", Type = CardType.Character, Controller = p1 };
        var dragon = new Card { Id = "way-of-the-dragon", Type = CardType.Attachment, Controller = p1, PrintedCost = 2, Location = "hand", PersistentEffects = LoadPersistentEffects() };
        p1.Hand.Add(dragon);
        p1.PlayArea.Add(ownCharacter);

        var context = new AbilityContext { Game = game, Player = p1, Source = dragon, Target = dragon, PlayAttachTarget = ownCharacter };

        new PlayCardGameActionHandler().Execute(context, null);

        Assert.That(dragon.AttachedTo, Is.EqualTo(ownCharacter));
    }
}
