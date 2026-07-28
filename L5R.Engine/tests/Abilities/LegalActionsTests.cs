using L5R.Engine.Abilities;
using L5R.Engine.State;

namespace L5R.Engine.Tests.Abilities;

/// <summary>
/// Exercises GetLegalActions against the same requirement-checking chain ringteki uses
/// (CardAction.meetsRequirements / BaseAbility.meetsRequirements): location, phase,
/// player ownership, ability limit, condition, cost affordability, and target legality
/// each independently gate whether an action is currently legal.
/// </summary>
public class LegalActionsTests
{
    private static GameState NewGame(out Player p1, out Player p2)
    {
        p1 = new Player { Name = "Player1" };
        p2 = new Player { Name = "Player2" };
        return new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
    }

    [Test]
    public void ActionIsLegal_WhenAllRequirementsAreMet()
    {
        var game = NewGame(out var p1, out _);
        var card = new Card { Id = "c1", Type = CardType.Character, Controller = p1 };
        p1.PlayArea.Add(card);
        card.Actions.Add(new CardAction { Title = "Ready this character", Card = card });

        var legal = LegalActions.GetLegalActions(game, p1);

        Assert.That(legal.Select(a => a.Title), Does.Contain("Ready this character"));
    }

    [Test]
    public void ActionIsIllegal_InTheWrongPhase()
    {
        var game = NewGame(out var p1, out _);
        game.CurrentPhase = Phase.Dynasty;
        var card = new Card { Id = "c1", Type = CardType.Character, Controller = p1 };
        p1.PlayArea.Add(card);
        card.Actions.Add(new CardAction { Title = "Conflict-only action", Card = card, Phase = Phase.Conflict });

        var legal = LegalActions.GetLegalActions(game, p1);

        Assert.That(legal, Is.Empty);
    }

    [Test]
    public void ActionIsIllegal_WhenBowSelfCostCannotBePaid()
    {
        var game = NewGame(out var p1, out _);
        var card = new Card { Id = "c1", Type = CardType.Character, Controller = p1, Bowed = true };
        p1.PlayArea.Add(card);
        card.Actions.Add(new CardAction { Title = "Bow for value", Card = card, Costs = new ICost[] { new BowSelfCost() } });

        var legal = LegalActions.GetLegalActions(game, p1);

        Assert.That(legal, Is.Empty, "already-bowed card can't pay a bowSelf cost");
    }

    [Test]
    public void ActionIsIllegal_WithoutOwnershipUnlessAnyPlayer()
    {
        var game = NewGame(out var p1, out var p2);
        var card = new Card { Id = "c1", Type = CardType.Character, Controller = p1 };
        p1.PlayArea.Add(card);
        card.Actions.Add(new CardAction { Title = "Owner-only action", Card = card });
        card.Actions.Add(new CardAction { Title = "Anyone can trigger", Card = card, AnyPlayer = true });

        var legalForOpponent = LegalActions.GetLegalActions(game, p2);

        Assert.That(legalForOpponent.Select(a => a.Title), Does.Not.Contain("Owner-only action"));
        Assert.That(legalForOpponent.Select(a => a.Title), Does.Contain("Anyone can trigger"));
    }

    [Test]
    public void ActionIsIllegal_WithoutALegalTarget_AndBecomesLegalOnceOneExists()
    {
        var game = NewGame(out var p1, out _);
        var source = new Card { Id = "source", Type = CardType.Character, Controller = p1 };
        p1.PlayArea.Add(source);
        source.Actions.Add(new CardAction
        {
            Title = "Ready a bowed character",
            Card = source,
            Targets = new ITargetRequirement[]
            {
                new CardTargetRequirement((card, _) => card.Type == CardType.Character && card.Bowed)
            }
        });

        Assert.That(LegalActions.GetLegalActions(game, p1), Is.Empty, "no bowed character exists yet");

        var bowedCharacter = new Card { Id = "other", Type = CardType.Character, Controller = p1, Bowed = true };
        p1.PlayArea.Add(bowedCharacter);

        var legal = LegalActions.GetLegalActions(game, p1);
        Assert.That(legal.Select(a => a.Title), Does.Contain("Ready a bowed character"));
    }

    [Test]
    public void ActionIsIllegal_OnceItsAbilityLimitIsExhausted()
    {
        var game = NewGame(out var p1, out _);
        var card = new Card { Id = "c1", Type = CardType.Character, Controller = p1 };
        p1.PlayArea.Add(card);
        var limit = new FixedAbilityLimit(1);
        var action = new CardAction { Title = "Once per round", Card = card, Limit = limit };
        card.Actions.Add(action);

        Assert.That(LegalActions.GetLegalActions(game, p1).Select(a => a.Title), Does.Contain("Once per round"));

        limit.Increment(p1);

        Assert.That(LegalActions.GetLegalActions(game, p1), Is.Empty, "limit exhausted after use");
    }
}
