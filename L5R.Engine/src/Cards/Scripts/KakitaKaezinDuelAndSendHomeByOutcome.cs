namespace L5R.Engine.Cards.Scripts;

/// <summary>
/// scriptOverride for kakita-kaezin: while participating, force the opponent to choose a
/// participating character to duel. If this character wins, send every uninvolved
/// character home; if it loses, send this character home. Needs a duel-outcome-dependent
/// follow-up gameAction the closed vocabulary can't express. Stubbed until the state
/// model has duels and conflicts.
/// </summary>
public sealed class KakitaKaezinDuelAndSendHomeByOutcome : ICardScript
{
}
