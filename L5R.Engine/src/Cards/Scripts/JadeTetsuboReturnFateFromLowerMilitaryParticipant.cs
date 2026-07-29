using L5R.Engine.Abilities;
using L5R.Engine.State;

namespace L5R.Engine.Cards.Scripts;

/// <summary>
/// jade-tetsubo (the attachmentMyControlOnly restriction is generic, handled by the card's
/// own persistentEffects block): while the attached character is participating, bow this
/// attachment and return all fate from a lower-*effective*-military participating character
/// (context.Target, caller-supplied - trust-the-caller, same convention as every other
/// target in this engine) to its own controller. Not a fate steal - ringteki's "recipient:
/// target.owner" is just the target's own controller in this engine's model, so this only
/// does something when the target is a weak participant on either side.
///
/// Written directly against the low-level state model (Conflict.Attackers/Defenders,
/// GameState.EffectiveMilitarySkill, direct Card/Player mutation) rather than round-tripping
/// through PredicateEvaluator/JsonElement - there's no JSON driving this card, so forcing one
/// through the JSON-oriented evaluator would fight the reason it needed a script at all.
/// </summary>
public sealed class JadeTetsuboReturnFateFromLowerMilitaryParticipant : ICardScript
{
    public void Execute(AbilityContext context)
    {
        var jadeTetsubo = context.Source;
        var parent = jadeTetsubo.AttachedTo
            ?? throw new InvalidOperationException($"'{jadeTetsubo.Id}' is not currently attached to anything.");

        if (!IsParticipating(context.Game, parent))
            throw new InvalidOperationException($"'{jadeTetsubo.Id}' can only be used while the attached character is participating.");

        if (jadeTetsubo.Bowed)
            throw new InvalidOperationException($"'{jadeTetsubo.Id}' is already bowed.");

        var target = context.Target
            ?? throw new InvalidOperationException($"'{jadeTetsubo.Id}' requires context.Target to be set.");

        if (!IsParticipating(context.Game, target))
            throw new InvalidOperationException($"'{target.Id}' is not participating.");

        if (context.Game.EffectiveMilitarySkill(target) >= context.Game.EffectiveMilitarySkill(parent))
            throw new InvalidOperationException($"'{target.Id}' does not have lower military skill than '{parent.Id}'.");

        jadeTetsubo.Bowed = true;
        target.Controller.Fate += target.Fate;
        target.Fate = 0;
    }

    private static bool IsParticipating(GameState game, Card card) =>
        game.CurrentConflict is { } conflict && (conflict.Attackers.Contains(card) || conflict.Defenders.Contains(card));
}
