using System.Linq;
using L5R.Engine.Abilities;
using L5R.Engine.Cards.Scripts;
using L5R.Engine.State;

namespace L5R.Engine.GameSteps.BotActions;

/// <summary>
/// Phase B adapter for lion's-pride-brawler. The script itself allows any target (own or
/// opponent's, bowed or not) whose effective military skill is at most the brawler's own -
/// this adapter narrows that to a sensible bot heuristic: an unbowed opponent-controlled
/// character, denying it from acting/defending further, rather than the script's full
/// (strategically nonsensical for a bot) legal range.
/// </summary>
public sealed class LionsPrideBrawlerBotAction : IBotScriptAction
{
    public bool IsLegal(GameState game, Card source, Player actingPlayer) =>
        game.CurrentConflict?.Attackers.Contains(source) == true && FindTarget(game, source) is not null;

    public void Invoke(GameState game, Card source, Player actingPlayer)
    {
        var target = FindTarget(game, source)
            ?? throw new InvalidOperationException($"'{source.Id}' has no legal bot target.");

        var context = new AbilityContext { Game = game, Player = actingPlayer, Source = source, Target = target };
        new LionsPrideBrawlerBowLowerMilitarySkill().Execute(context);
    }

    private static Card? FindTarget(GameState game, Card source) =>
        game.AllCards()
            .Where(c => c.Controller != source.Controller && !c.Bowed && game.EffectiveMilitarySkill(c) <= game.EffectiveMilitarySkill(source))
            .FirstOrDefault();
}
