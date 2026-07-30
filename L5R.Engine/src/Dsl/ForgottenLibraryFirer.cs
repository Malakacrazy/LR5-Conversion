using System.Linq;
using L5R.Engine.Abilities;
using L5R.Engine.Cards.Scripts;
using L5R.Engine.State;

namespace L5R.Engine.Dsl;

/// <summary>
/// forgotten-library: after the draw phase begins, draw 1 card. Its own script only checks
/// GameState.CurrentPhase, so this fires as a one-shot hook at the very top of
/// GameLoop.DrawPhaseStep - before the honor bid or the bid-driven draw, matching "after the
/// draw phase begins" as the phase's very first event. Holdings sit in PlayArea once "played"
/// from their revealed province slot (this engine's established convention - see
/// SecludedTempleBotAction/ArtisanAcademyBotAction, both already adopted the same way), so
/// this scans PlayArea, not Provinces.
/// </summary>
public static class ForgottenLibraryFirer
{
    public static void FireIfLegal(GameState game, Player player)
    {
        foreach (var library in player.PlayArea.Where(c => c.Id == "forgotten-library" && !game.IsBlanked(c)).ToList())
        {
            var context = new AbilityContext { Game = game, Player = player, Source = library };
            new ForgottenLibraryDrawOnDrawPhase().Execute(context);
        }
    }
}
