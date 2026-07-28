namespace L5R.Engine.Cards.Scripts;

/// <summary>
/// scriptOverride for calling-in-favors: dishonor a friendly character, then attach the
/// chosen opponent's attachment to it and take control (or discard it if attaching isn't
/// possible). Needs a contextPath reference to the dishonor cost's chosen target, which
/// the closed valueRef vocabulary doesn't support. Stubbed until the state model has
/// attachments and cost-target cross-referencing.
/// </summary>
public sealed class CallingInFavorsAttachOrDiscard : ICardScript
{
}
