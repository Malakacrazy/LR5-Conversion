using L5R.Engine.Abilities;

namespace L5R.Engine.Cards.Scripts;

/// <summary>voice-of-honor: same reasoning as ForgedEdictCannotPlayNormally - its effect is entirely a JSON-driven "wouldInterrupt" triggeredAbility resolved exclusively by WouldInterruptOfferer.</summary>
public sealed class VoiceOfHonorCannotPlayNormally : ICardScript
{
    public bool CanPlay(AbilityContext context) => false;
}
