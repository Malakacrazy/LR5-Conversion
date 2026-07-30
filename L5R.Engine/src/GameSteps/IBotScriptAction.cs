using L5R.Engine.State;

namespace L5R.Engine.GameSteps;

/// <summary>
/// Phase B: an opt-in bridge from one scriptOverride card's bespoke ICardScript to something
/// a bot can discover and invoke automatically during GameLoop.RunActionWindow - mirroring
/// what CardFactory's CardAction bridge (M1) does for plain JSON abilities.actions[] entries,
/// but per-script rather than generic. There's no safe reflection-based dispatch across the
/// ~95 scriptOverride classes: their Execute(context) signatures agree, but what extra input
/// each one needs (a target, a choice, nothing at all) and when it's even legal to attempt
/// varies card by card. Adopted one card at a time into ScriptedActionRegistry, same
/// incremental rhythm as the scriptOverride backlog itself.
/// </summary>
public interface IBotScriptAction
{
    /// <summary>Is this action currently legal for its own source card, attempted by actingPlayer? Mirrors CardAction.MeetsRequirements' role for plain JSON actions.</summary>
    bool IsLegal(GameState game, Card source, Player actingPlayer);

    /// <summary>
    /// Supplies whatever inputs the underlying script needs (context.Target,
    /// context.PlayAttachTarget, context.ChosenChoice, ...), using the simplest available
    /// legal choice - matching FirstLegalActionBotPolicy's own "first legal option"
    /// philosophy - then invokes it. Only called after IsLegal returned true for the same
    /// (game, source, actingPlayer).
    /// </summary>
    void Invoke(GameState game, Card source, Player actingPlayer);
}
