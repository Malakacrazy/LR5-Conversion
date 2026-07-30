using L5R.Engine.Abilities;
using L5R.Engine.Cards.Scripts;
using L5R.Engine.State;

namespace L5R.Engine.GameSteps.BotActions;

/// <summary>
/// kakita-asami: Conflict.AttackerSkill/DefenderSkill are only meaningfully populated once
/// skill has been summed, i.e. by the post-resolution window - during the earlier mid-
/// conflict window both default to 0, making the comparison always false there. The script
/// itself never requires kakita-asami to be in play or participating (a real character
/// ability should only function while in play) - the adapter adds a Location == "play area"
/// gate the script doesn't check, since ChooseScriptedAction would otherwise also offer this
/// while she's sitting unplayed in hand.
/// </summary>
public sealed class KakitaAsamiBotAction : IBotScriptAction
{
    public bool IsLegal(GameState game, Card source, Player actingPlayer) =>
        source.Location == "play area"
        && game.CurrentConflict is { ConflictType: "political" } conflict
        && IsWinningForController(conflict, actingPlayer);

    public void Invoke(GameState game, Card source, Player actingPlayer)
    {
        var context = new AbilityContext { Game = game, Player = actingPlayer, Source = source };
        new KakitaAsamiTakeHonorWhenWinningPoliticalSkill().Execute(context);
    }

    private static bool IsWinningForController(Conflict conflict, Player actingPlayer)
    {
        var difference = conflict.AttackerSkill - conflict.DefenderSkill;
        return actingPlayer == conflict.AttackingPlayer ? difference > 0 : difference < 0;
    }
}
