namespace L5R.Engine.Cards.Scripts;

/// <summary>
/// scriptOverride for vengeful-berserker: after another character you control leaves
/// play during a conflict, double this character's military skill until the end of the
/// conflict. Needs event.cardStateWhenLeftPlay field inspection, a snapshot field beyond
/// the closed predicate vocabulary. Stubbed until the state model has conflicts.
/// </summary>
public sealed class VengefulBerserkerDoubleMilitaryOnAllyLeavingPlay : ICardScript
{
}
