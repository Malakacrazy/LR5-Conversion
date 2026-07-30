using L5R.Engine.Abilities;
using L5R.Engine.Cards.Scripts;
using L5R.Engine.State;

namespace L5R.Engine.GameSteps.BotActions;

/// <summary>Phase B adapter for artisan-academy. No target at all - the script is a verified no-op precondition check (see its own doc comment).</summary>
public sealed class ArtisanAcademyBotAction : IBotScriptAction
{
    public bool IsLegal(GameState game, Card source, Player actingPlayer) =>
        game.CurrentPhase == Phase.Conflict && actingPlayer.Deck.Count > 0;

    public void Invoke(GameState game, Card source, Player actingPlayer)
    {
        var context = new AbilityContext { Game = game, Player = actingPlayer, Source = source };
        new ArtisanAcademyRevealTopCard().Execute(context);
    }
}
