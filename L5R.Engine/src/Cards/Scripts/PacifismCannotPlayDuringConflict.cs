using L5R.Engine.Abilities;

namespace L5R.Engine.Cards.Scripts;

/// <summary>
/// pacifism: cannot be played while a conflict is currently in progress (ringteki
/// DrawCard.canPlay override). Same gate as HeightOfFashionCannotPlayDuringConflict; the
/// card's whileAttached cannotParticipate effects are expressed generically in its JSON.
/// </summary>
public sealed class PacifismCannotPlayDuringConflict : ICardScript
{
    public bool CanPlay(AbilityContext context) => context.Game.CurrentConflict is null;
}
