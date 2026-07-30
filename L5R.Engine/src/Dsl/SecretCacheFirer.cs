using System.Linq;
using L5R.Engine.Abilities;
using L5R.Engine.Cards.Scripts;
using L5R.Engine.State;

namespace L5R.Engine.Dsl;

/// <summary>
/// secret-cache: after a conflict is declared against this province, search the top 5 cards
/// of the controller's deck. A province card, never scanned by ChooseScriptedAction (only
/// Hand+PlayArea are), so this is fired directly at the exact production moment - the same
/// conflict-declaration hook ConflictResolver already uses for TriggeredReactionFirer's
/// onCardRevealed. Picks the top card of the searched pool as the "first legal candidate",
/// same heuristic as every other adopted card.
/// </summary>
public static class SecretCacheFirer
{
    public static void FireIfLegal(GameState game, Card province)
    {
        if (province.Id != "secret-cache" || game.CurrentConflict is not { } conflict || conflict.DeclaredProvince != province)
            return;

        var context = new AbilityContext { Game = game, Player = province.Controller, Source = province, ChosenDeckSearchCard = province.Controller.Deck.FirstOrDefault() };
        new SecretCacheSearchTopFiveOnConflictDeclared().Execute(context);
    }
}
