using System.Linq;
using L5R.Engine.Abilities;
using L5R.Engine.Cards.Scripts;
using L5R.Engine.State;

namespace L5R.Engine.GameSteps.BotActions;

/// <summary>
/// Phase B adapter for radiant-orator - same "send home an outclassed opponent" family as
/// OutwitBotAction/RoutBotAction/StrengthInNumbersBotAction. The script itself only requires
/// the target be opponent-controlled (not necessarily participating), but sending home a
/// non-participant is a meaningless no-op via SendHomeGameActionHandler, so the adapter
/// narrows to an opponent participant specifically.
/// </summary>
public sealed class RadiantOratorBotAction : IBotScriptAction
{
    public bool IsLegal(GameState game, Card source, Player actingPlayer) =>
        game.CurrentConflict is { } conflict && (conflict.Attackers.Contains(source) || conflict.Defenders.Contains(source))
        && ReadyParticipatingGlory(game, actingPlayer) > ReadyParticipatingGlory(game, game.Opponent(actingPlayer))
        && FindTarget(game, actingPlayer) is not null;

    public void Invoke(GameState game, Card source, Player actingPlayer)
    {
        var target = FindTarget(game, actingPlayer)
            ?? throw new InvalidOperationException($"'{source.Id}' has no legal bot target.");

        var context = new AbilityContext { Game = game, Player = actingPlayer, Source = source, Target = target };
        new RadiantOratorSendHomeWhenAheadOnGlory().Execute(context);
    }

    private static Card? FindTarget(GameState game, Player actingPlayer)
    {
        var conflict = game.CurrentConflict;
        if (conflict is null)
            return null;

        var opponent = game.Opponent(actingPlayer);
        return conflict.Attackers.Concat(conflict.Defenders).FirstOrDefault(c => c.Controller == opponent);
    }

    private static int ReadyParticipatingGlory(GameState game, Player player) =>
        game.CurrentConflict is { } conflict
            ? conflict.Attackers.Concat(conflict.Defenders)
                .Where(c => c.Controller == player && c.Type == CardType.Character && !c.Bowed)
                .Sum(game.EffectiveGlory)
            : 0;
}
