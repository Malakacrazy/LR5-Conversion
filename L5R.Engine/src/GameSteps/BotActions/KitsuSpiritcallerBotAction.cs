using System.Linq;
using L5R.Engine.Abilities;
using L5R.Engine.Cards.Scripts;
using L5R.Engine.State;

namespace L5R.Engine.GameSteps.BotActions;

/// <summary>
/// Phase B adapter for kitsu-spiritcaller. Adds an adapter-only "not already bowed" gate
/// beyond the script's own checks - it pays a bow-self cost internally but never checks its
/// own Bowed status first, so without this the same instance could be "re-legal" and asked
/// to bow itself again this window even though it can only pay that cost once.
/// </summary>
public sealed class KitsuSpiritcallerBotAction : IBotScriptAction
{
    public bool IsLegal(GameState game, Card source, Player actingPlayer) =>
        !source.Bowed && game.CurrentConflict is not null && FindTarget(actingPlayer) is not null;

    public void Invoke(GameState game, Card source, Player actingPlayer)
    {
        var target = FindTarget(actingPlayer)
            ?? throw new InvalidOperationException($"'{source.Id}' has no legal bot target.");

        var context = new AbilityContext { Game = game, Player = actingPlayer, Source = source, Target = target };
        new KitsuSpiritcallerResurrectUntilConflictEnd().Execute(context);
    }

    private static Card? FindTarget(Player actingPlayer) =>
        actingPlayer.Discard.FirstOrDefault(c => c.Type == CardType.Character);
}
