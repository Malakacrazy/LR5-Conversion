using System.Linq;
using L5R.Engine.Abilities;
using L5R.Engine.Cards.Scripts;
using L5R.Engine.State;

namespace L5R.Engine.GameSteps.BotActions;

/// <summary>Phase B adapter for jade-tetsubo. Target pool is any participant (either side) with lower effective military skill than the attached parent.</summary>
public sealed class JadeTetsuboBotAction : IBotScriptAction
{
    public bool IsLegal(GameState game, Card source, Player actingPlayer) =>
        !source.Bowed && source.AttachedTo is { } parent && IsParticipating(game, parent) && FindTarget(game, parent) is not null;

    public void Invoke(GameState game, Card source, Player actingPlayer)
    {
        var parent = source.AttachedTo
            ?? throw new InvalidOperationException($"'{source.Id}' is not currently attached to anything.");
        var target = FindTarget(game, parent)
            ?? throw new InvalidOperationException($"'{source.Id}' has no legal bot target.");

        var context = new AbilityContext { Game = game, Player = actingPlayer, Source = source, Target = target };
        new JadeTetsuboReturnFateFromLowerMilitaryParticipant().Execute(context);
    }

    private static Card? FindTarget(GameState game, Card parent)
    {
        var conflict = game.CurrentConflict;
        if (conflict is null)
            return null;

        return conflict.Attackers.Concat(conflict.Defenders)
            .FirstOrDefault(c => game.EffectiveMilitarySkill(c) < game.EffectiveMilitarySkill(parent));
    }

    private static bool IsParticipating(GameState game, Card card) =>
        game.CurrentConflict is { } conflict && (conflict.Attackers.Contains(card) || conflict.Defenders.Contains(card));
}
