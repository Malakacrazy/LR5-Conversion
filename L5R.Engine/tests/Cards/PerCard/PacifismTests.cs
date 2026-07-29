using System.Text.Json;
using L5R.Engine.Abilities;
using L5R.Engine.Cards.Scripts;
using L5R.Engine.Dsl;
using L5R.Engine.Dsl.GameActions;
using L5R.Engine.State;

namespace L5R.Engine.Tests.Cards.PerCard;

public class PacifismTests
{
    private static IReadOnlyList<WhileAttachedDefinition> LoadWhileAttached()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Cards", "01-Core", "pacifism.json");
        using var document = JsonDocument.Parse(File.ReadAllText(path));
        return AbilityDefinitionParser.ParseWhileAttached(document.RootElement);
    }

    [Test]
    public void DuringAMilitaryConflict_TheBearerCannotBeDeclaredAsAnAttackerOrDefender()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var bearer = new Card { Id = "pacified-character", Type = CardType.Character, Controller = p1 };
        var pacifism = new Card
        {
            Id = "pacifism", Type = CardType.Attachment, Controller = p1,
            AttachedTo = bearer, WhileAttachedEffects = LoadWhileAttached()
        };
        p1.PlayArea.Add(bearer);
        p1.PlayArea.Add(pacifism);

        var conflict = new Conflict { AttackingPlayer = p1, DefendingPlayer = p2, ConflictType = "military" };
        game.CurrentConflict = conflict;

        var context = new AbilityContext { Game = game, Player = p1, Source = bearer, Target = bearer };

        Assert.Throws<InvalidOperationException>(() => new MoveToConflictGameActionHandler().Execute(context, parameters: null));
        Assert.That(conflict.Attackers, Does.Not.Contain(bearer));
    }

    [Test]
    public void DuringAPoliticalConflict_TheBearerCanStillParticipate()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var bearer = new Card { Id = "pacified-character", Type = CardType.Character, Controller = p1 };
        var pacifism = new Card
        {
            Id = "pacifism", Type = CardType.Attachment, Controller = p1,
            AttachedTo = bearer, WhileAttachedEffects = LoadWhileAttached()
        };
        p1.PlayArea.Add(bearer);
        p1.PlayArea.Add(pacifism);

        var conflict = new Conflict { AttackingPlayer = p1, DefendingPlayer = p2, ConflictType = "political" };
        game.CurrentConflict = conflict;

        var context = new AbilityContext { Game = game, Player = p1, Source = bearer, Target = bearer };
        new MoveToConflictGameActionHandler().Execute(context, parameters: null);

        Assert.That(conflict.Attackers, Does.Contain(bearer), "pacifism only restricts military conflicts");
    }

    [Test]
    public void WithNoActiveConflict_CanBePlayed()
    {
        var p1 = new Player { Name = "Player1", Fate = 5 };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Dynasty };
        var character = new Card { Id = "some-character", Type = CardType.Character, Controller = p2 };
        var pacifism = new Card
        {
            Id = "pacifism", Type = CardType.Attachment, Controller = p1,
            PrintedCost = 2, PlayScript = new PacifismCannotPlayDuringConflict()
        };
        p1.Hand.Add(pacifism);
        p2.PlayArea.Add(character);

        var context = new AbilityContext { Game = game, Player = p1, Source = pacifism, Target = pacifism, PlayAttachTarget = character };

        new PlayCardGameActionHandler().Execute(context, null);

        Assert.That(p1.PlayArea, Does.Contain(pacifism));
    }

    [Test]
    public void DuringAConflict_CannotBePlayed()
    {
        var p1 = new Player { Name = "Player1", Fate = 5 };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var character = new Card { Id = "some-character", Type = CardType.Character, Controller = p2 };
        var pacifism = new Card
        {
            Id = "pacifism", Type = CardType.Attachment, Controller = p1,
            PrintedCost = 2, PlayScript = new PacifismCannotPlayDuringConflict()
        };
        p1.Hand.Add(pacifism);
        p2.PlayArea.Add(character);
        game.CurrentConflict = new Conflict { AttackingPlayer = p1, DefendingPlayer = p2 };

        var context = new AbilityContext { Game = game, Player = p1, Source = pacifism, Target = pacifism, PlayAttachTarget = character };

        Assert.Throws<InvalidOperationException>(() => new PlayCardGameActionHandler().Execute(context, null));
    }
}
