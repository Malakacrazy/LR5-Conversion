using L5R.Engine.Abilities;
using L5R.Engine.Dsl.GameActions;

namespace L5R.Engine.Cards.Scripts;

/// <summary>
/// ready-for-battle: after an opponent's card effect or a ring effect bows a character
/// you control, ready that character. Targets the bowed character directly
/// (context.Target, trust-the-caller) rather than a player-chosen selection - ringteki's
/// own gameAction target is "context.event.card", not a prompted choice. Whether the bow
/// was caused by the reacting player themself (which should NOT trigger this - e.g. bowing
/// your own character as a cost) is a caller-set fact: see
/// AbilityContext.BowCausedBySelf's own doc comment.
/// </summary>
public sealed class ReadyForBattleReadyOnOpponentOrRingBow : ICardScript
{
    public void Execute(AbilityContext context)
    {
        var target = context.Target
            ?? throw new InvalidOperationException("ready-for-battle requires context.Target (the character that was bowed) to be set.");

        if (target.Controller != context.Player)
            throw new InvalidOperationException($"'{target.Id}' must be controlled by ready-for-battle's controller.");

        if (context.BowCausedBySelf)
            throw new InvalidOperationException("ready-for-battle can only trigger when the bow wasn't caused by its controller's own ability.");

        new ReadyGameActionHandler().Execute(context, null);
    }
}
