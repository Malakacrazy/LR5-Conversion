using System.Linq;
using L5R.Engine.Abilities;
using L5R.Engine.Cards.Scripts;
using L5R.Engine.State;

namespace L5R.Engine.GameSteps.BotActions;

/// <summary>
/// tattooed-wanderer may be played as an attachment instead of a character - a stable state
/// fact ("still sitting in hand, unattached") that self-resets the moment it's played, same
/// shape as the already-adopted togashi-kazue in-play action. Since ChooseScriptedAction is
/// checked before the generic hand-play fallback (see ActionWindowRunner), registering this
/// always prefers the attachment mode over playing it as a plain character - a heuristic
/// choice, not a rule, but a reasonable one for a trivial bot.
/// </summary>
public sealed class TattooedWandererBotAction : IBotScriptAction
{
    public bool IsLegal(GameState game, Card source, Player actingPlayer) =>
        actingPlayer.Hand.Contains(source)
        && game.EffectiveCost(source, actingPlayer) <= actingPlayer.Fate
        && FindAttachTarget(game, source, actingPlayer) is not null;

    public void Invoke(GameState game, Card source, Player actingPlayer)
    {
        var target = FindAttachTarget(game, source, actingPlayer)
            ?? throw new InvalidOperationException($"'{source.Id}' has no legal bot attach target.");

        var context = new AbilityContext { Game = game, Player = actingPlayer, Source = source, PlayAttachTarget = target };
        new TattooedWandererPlayAsAttachment().Execute(context);
    }

    private static Card? FindAttachTarget(GameState game, Card source, Player actingPlayer) =>
        actingPlayer.PlayArea.FirstOrDefault(c => c.Type == CardType.Character && !game.IsAttachRestricted(source, c, actingPlayer));
}
