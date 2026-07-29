using System.Text.Json;
using L5R.Engine.Abilities;
using L5R.Engine.Dsl;
using L5R.Engine.Dsl.GameActions;
using L5R.Engine.State;

namespace L5R.Engine.Tests.Cards.PerCard;

/// <summary>
/// Only daimyo-s-favor's persistentEffects block (attachmentMyControlOnly) is exercised
/// here - its own "actions" (bow to reduce an attachment's cost) needs a "reduceCost" effect
/// distinct from the existing reduceNextPlayedCardCost (cardType-scoped, plus an "isSameAs"
/// predicate op and "limit": {"fixed": 1} semantics, none of which any ported card needs
/// yet), same "port only the reachable slice" convention as court-mask/favored-mount/
/// way-of-the-dragon.
/// </summary>
public class DaimyoSFavorTests
{
    private static IReadOnlyList<PersistentEffectDefinition> LoadPersistentEffects()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Cards", "01-Core", "daimyo-s-favor.json");
        using var document = JsonDocument.Parse(File.ReadAllText(path));
        return AbilityDefinitionParser.ParsePersistentEffects(document.RootElement);
    }

    [Test]
    public void PlayingItOntoAnOpponentsCharacter_IsRestricted()
    {
        var p1 = new Player { Name = "Player1", Fate = 2 };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        var opponentCharacter = new Card { Id = "opponent-character", Type = CardType.Character, Controller = p2 };
        var favor = new Card { Id = "daimyo-s-favor", Type = CardType.Attachment, Controller = p1, PrintedCost = 0, Location = "hand", PersistentEffects = LoadPersistentEffects() };
        p1.Hand.Add(favor);
        p2.PlayArea.Add(opponentCharacter);

        var context = new AbilityContext { Game = game, Player = p1, Source = favor, Target = favor, PlayAttachTarget = opponentCharacter };

        Assert.Throws<InvalidOperationException>(() => new PlayCardGameActionHandler().Execute(context, null));
        Assert.That(favor.AttachedTo, Is.Null, "the attach never happened");
    }

    [Test]
    public void PlayingItOntoYourOwnCharacter_IsAllowed()
    {
        var p1 = new Player { Name = "Player1", Fate = 2 };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        var ownCharacter = new Card { Id = "own-character", Type = CardType.Character, Controller = p1 };
        var favor = new Card { Id = "daimyo-s-favor", Type = CardType.Attachment, Controller = p1, PrintedCost = 0, Location = "hand", PersistentEffects = LoadPersistentEffects() };
        p1.Hand.Add(favor);
        p1.PlayArea.Add(ownCharacter);

        var context = new AbilityContext { Game = game, Player = p1, Source = favor, Target = favor, PlayAttachTarget = ownCharacter };

        new PlayCardGameActionHandler().Execute(context, null);

        Assert.That(favor.AttachedTo, Is.EqualTo(ownCharacter));
    }
}
