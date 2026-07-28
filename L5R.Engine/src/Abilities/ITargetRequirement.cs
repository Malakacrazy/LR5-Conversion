namespace L5R.Engine.Abilities;

/// <summary>
/// ringteki AbilityTargetCard.hasLegalTarget: does at least one eligible candidate exist
/// right now for this target slot?
/// </summary>
public interface ITargetRequirement
{
    bool HasLegalTarget(AbilityContext context);
}
