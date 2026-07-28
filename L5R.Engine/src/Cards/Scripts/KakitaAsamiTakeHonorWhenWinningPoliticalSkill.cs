namespace L5R.Engine.Cards.Scripts;

/// <summary>
/// scriptOverride for kakita-asami: while participating in a political conflict, if your
/// side has more political skill than your opponent's, take 1 honor from them. Needs a
/// skill-difference comparison whose direction depends on attacker/defender perspective,
/// beyond compareValues's fixed-direction comparison. Stubbed until the state model has
/// conflicts.
/// </summary>
public sealed class KakitaAsamiTakeHonorWhenWinningPoliticalSkill : ICardScript
{
}
