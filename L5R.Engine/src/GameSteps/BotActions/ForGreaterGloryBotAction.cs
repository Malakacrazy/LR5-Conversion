using L5R.Engine.Abilities;
using L5R.Engine.Cards.Scripts;
using L5R.Engine.State;

namespace L5R.Engine.GameSteps.BotActions;

/// <summary>
/// for-greater-glory: the script itself only checks ConflictType/AttackingPlayer, trusting
/// the caller to know a province actually broke - the adapter adds a
/// DeclaredProvince.Broken gate the script doesn't check, since otherwise the bot would fire
/// this on every winning military-attacker conflict regardless of whether a province broke.
/// No target selection needed - the script bulk-applies to every own Bushi in the conflict.
/// </summary>
public sealed class ForGreaterGloryBotAction : IBotScriptAction
{
    public bool IsLegal(GameState game, Card source, Player actingPlayer) =>
        game.CurrentConflict is { } conflict
        && conflict.ConflictType == "military"
        && conflict.AttackingPlayer == actingPlayer
        && conflict.DeclaredProvince is { Broken: true };

    public void Invoke(GameState game, Card source, Player actingPlayer)
    {
        var context = new AbilityContext { Game = game, Player = actingPlayer, Source = source };
        new ForGreaterGloryPlaceFateOnBushi().Execute(context);
    }
}
