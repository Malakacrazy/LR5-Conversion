namespace L5R.Engine.Cards.Scripts;

/// <summary>
/// scriptOverride for ikoma-eiji: after losing a political conflict, put a cheap bushi
/// character into play from your discard pile or provinces. Needs event.conflict field
/// inspection plus a two-level select (choose which character). Stubbed until the state
/// model has conflicts and nested selection.
/// </summary>
public sealed class IkomaEijiPutBushiIntoPlayOnPoliticalLoss : ICardScript
{
}
