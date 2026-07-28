namespace L5R.Engine.Cards.Scripts;

/// <summary>
/// scriptOverride for secret-cache: after a conflict is declared against this province,
/// search the top 5 cards of your conflict deck for a card and add it to hand, then
/// shuffle. Needs event.conflict.conflictProvince field inspection, the same gap as
/// EndlessPlainsBreakAndDiscardAttacker. Stubbed until the state model has conflicts and
/// provinces.
/// </summary>
public sealed class SecretCacheSearchTopFiveOnConflictDeclared : ICardScript
{
}
