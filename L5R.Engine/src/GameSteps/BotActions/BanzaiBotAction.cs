using System.Linq;
using L5R.Engine.Abilities;
using L5R.Engine.Cards.Scripts;
using L5R.Engine.State;

namespace L5R.Engine.GameSteps.BotActions;

/// <summary>
/// Phase B adapter for the event card banzai - invoked via IBotPolicy.ResolveEventScript
/// (EventResolver's scriptOverride'd-event fallback), same convention as outwit/rout: only
/// makes sense as part of playing the card during an active conflict. Prefers boosting the
/// bot's own participant (the real card's likely intent, though the script itself doesn't
/// restrict controller). Only resolves the ability once - the real card's "may resolve
/// again for 1 honor" chain is the caller's responsibility, same convention already used for
/// shiba-tsukune/giver-of-gifts.
/// </summary>
public sealed class BanzaiBotAction : IBotScriptAction
{
    public bool IsLegal(GameState game, Card source, Player actingPlayer) =>
        FindTarget(game, actingPlayer) is not null;

    public void Invoke(GameState game, Card source, Player actingPlayer)
    {
        var target = FindTarget(game, actingPlayer)
            ?? throw new InvalidOperationException($"'{source.Id}' has no legal bot target.");

        var context = new AbilityContext { Game = game, Player = actingPlayer, Source = source, Target = target };
        new BanzaiGrantMilitarySkillRepeatable().Execute(context);
    }

    private static Card? FindTarget(GameState game, Player actingPlayer)
    {
        var conflict = game.CurrentConflict;
        if (conflict is null) return null;

        return conflict.Attackers.Concat(conflict.Defenders).FirstOrDefault(c => c.Controller == actingPlayer);
    }
}
