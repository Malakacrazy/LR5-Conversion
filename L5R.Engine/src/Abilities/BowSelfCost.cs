namespace L5R.Engine.Abilities;

/// <summary>ringteki costs.js bowSelf.</summary>
public sealed class BowSelfCost : ICost
{
    public bool CanPay(AbilityContext context) => !context.Source.Bowed;
}
