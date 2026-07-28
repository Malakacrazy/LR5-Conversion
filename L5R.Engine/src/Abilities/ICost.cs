namespace L5R.Engine.Abilities;

/// <summary>One entry from ringteki's costs.js catalog: can this cost currently be paid?</summary>
public interface ICost
{
    bool CanPay(AbilityContext context);
}
