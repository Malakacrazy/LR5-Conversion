namespace L5R.Engine.Abilities;

/// <summary>ringteki costs.js payFate.</summary>
public sealed class PayFateCost : ICost
{
    private readonly int _amount;

    public PayFateCost(int amount = 1) => _amount = amount;

    public bool CanPay(AbilityContext context) => context.Player.Fate >= _amount;
}
