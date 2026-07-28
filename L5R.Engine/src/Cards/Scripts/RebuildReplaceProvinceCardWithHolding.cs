namespace L5R.Engine.Cards.Scripts;

/// <summary>
/// scriptOverride for rebuild: shuffle a card in one of your unbroken provinces back
/// into your dynasty deck, then put a holding from your discard pile into play in that
/// province. Needs a candidate-relative province-status lookup and a cost-to-target
/// cross-reference, neither modeled by the closed vocabulary. Stubbed until the state
/// model has provinces.
/// </summary>
public sealed class RebuildReplaceProvinceCardWithHolding : ICardScript
{
}
