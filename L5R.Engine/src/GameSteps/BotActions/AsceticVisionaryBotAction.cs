using System.Linq;
using L5R.Engine.Abilities;
using L5R.Engine.Cards.Scripts;
using L5R.Engine.Dsl.Costs;
using L5R.Engine.State;

namespace L5R.Engine.GameSteps.BotActions;

/// <summary>
/// Phase B adapter for ascetic-visionary. Narrows the script's own permissive target range
/// (any monk-or-monk-attachment-holder, either controller) to a sensible bot heuristic: one
/// of the bot's own bowed characters - readying an opponent's character, or one already
/// ready, would be a pointless move a trivial bot shouldn't make even though the script
/// itself doesn't forbid it.
/// </summary>
public sealed class AsceticVisionaryBotAction : IBotScriptAction
{
    public bool IsLegal(GameState game, Card source, Player actingPlayer) =>
        game.CurrentConflict?.Attackers.Contains(source) == true
        && new PayFateToRingCostHandler().CanPay(new AbilityContext { Game = game, Player = actingPlayer, Source = source }, null)
        && FindTarget(game, actingPlayer) is not null;

    public void Invoke(GameState game, Card source, Player actingPlayer)
    {
        var target = FindTarget(game, actingPlayer)
            ?? throw new InvalidOperationException($"'{source.Id}' has no legal bot target.");
        var ring = game.Rings.First(r => r.IsUnclaimed);

        var context = new AbilityContext { Game = game, Player = actingPlayer, Source = source, Target = target, CostRingTarget = ring };
        new AsceticVisionaryReadyMonkOrMonkAttachmentHolder().Execute(context);
    }

    private static Card? FindTarget(GameState game, Player actingPlayer) =>
        actingPlayer.PlayArea.FirstOrDefault(c =>
            c.Bowed && (c.Traits.Contains("monk") || game.AllCards().Any(a => a.AttachedTo == c && a.Traits.Contains("monk"))));
}
