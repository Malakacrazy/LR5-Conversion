using L5R.Engine.State;

namespace L5R.Engine.Abilities;

/// <summary>ringteki AbilityTargetCard: a target slot that must resolve to a card matching a condition.</summary>
public sealed class CardTargetRequirement : ITargetRequirement
{
    private readonly Func<Card, AbilityContext, bool> _cardCondition;

    public CardTargetRequirement(Func<Card, AbilityContext, bool> cardCondition) => _cardCondition = cardCondition;

    public bool HasLegalTarget(AbilityContext context) =>
        context.Game.AllCards().Any(card => _cardCondition(card, context));
}
