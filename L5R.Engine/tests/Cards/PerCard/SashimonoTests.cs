using System.Text.Json;
using L5R.Engine.Abilities;
using L5R.Engine.Dsl;
using L5R.Engine.Dsl.GameActions;
using L5R.Engine.State;

namespace L5R.Engine.Tests.Cards.PerCard;

public class SashimonoTests
{
    private static IReadOnlyList<WhileAttachedDefinition> LoadWhileAttached()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Cards", "01-Core", "sashimono.json");
        using var document = JsonDocument.Parse(File.ReadAllText(path));
        return AbilityDefinitionParser.ParseWhileAttached(document.RootElement);
    }

    [Test]
    public void DuringAMilitaryConflict_TheBearerCannotBeBowed()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var bearer = new Card { Id = "some-bushi", Type = CardType.Character, Controller = p1 };
        var sashimono = new Card
        {
            Id = "sashimono", Type = CardType.Attachment, Controller = p1,
            AttachedTo = bearer, WhileAttachedEffects = LoadWhileAttached()
        };
        p1.PlayArea.Add(bearer);
        p1.PlayArea.Add(sashimono);

        var conflict = new Conflict { AttackingPlayer = p1, DefendingPlayer = p2, ConflictType = "military" };
        game.CurrentConflict = conflict;

        var context = new AbilityContext { Game = game, Player = p2, Source = new Card { Id = "enemy", Type = CardType.Character, Controller = p2 }, Target = bearer };

        Assert.Throws<InvalidOperationException>(() => new BowGameActionHandler().Execute(context, parameters: null));
        Assert.That(bearer.Bowed, Is.False);
    }

    [Test]
    public void DuringAPoliticalConflict_TheBearerCanStillBeBowed()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var bearer = new Card { Id = "some-bushi", Type = CardType.Character, Controller = p1 };
        var sashimono = new Card
        {
            Id = "sashimono", Type = CardType.Attachment, Controller = p1,
            AttachedTo = bearer, WhileAttachedEffects = LoadWhileAttached()
        };
        p1.PlayArea.Add(bearer);
        p1.PlayArea.Add(sashimono);

        var conflict = new Conflict { AttackingPlayer = p1, DefendingPlayer = p2, ConflictType = "political" };
        game.CurrentConflict = conflict;

        var context = new AbilityContext { Game = game, Player = p2, Source = new Card { Id = "enemy", Type = CardType.Character, Controller = p2 }, Target = bearer };
        new BowGameActionHandler().Execute(context, parameters: null);

        Assert.That(bearer.Bowed, Is.True);
    }
}
