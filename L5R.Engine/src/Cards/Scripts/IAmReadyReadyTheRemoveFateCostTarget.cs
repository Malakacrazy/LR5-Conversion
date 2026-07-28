namespace L5R.Engine.Cards.Scripts;

/// <summary>
/// scriptOverride for i-am-ready: remove 1 fate from a friendly Unicorn character to
/// ready that same character. Needs a contextPath reference to the removeFate cost's
/// chosen target, which the closed valueRef vocabulary doesn't support. Stubbed until the
/// state model has cost-target cross-referencing.
/// </summary>
public sealed class IAmReadyReadyTheRemoveFateCostTarget : ICardScript
{
}
