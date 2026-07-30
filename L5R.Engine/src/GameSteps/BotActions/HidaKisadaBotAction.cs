using System.Linq;
using L5R.Engine.Abilities;
using L5R.Engine.Cards.Scripts;
using L5R.Engine.State;

namespace L5R.Engine.GameSteps.BotActions;

/// <summary>
/// hida-kisada's entire precondition is stable, currently-queryable state (in play, unblanked,
/// opponent hasn't won a conflict this round, not already used this conflict) with no event
/// tracking at all - the cleanest fit of this whole reaction batch. GameState.
/// FirstActionCancelledThisConflict self-resets once per new conflict, so the "once per
/// conflict" limit needs no adapter-added gate. Note: nothing elsewhere in the engine actually
/// checks this flag to block an opponent's action - the script's own doc comment already
/// documents this as a pre-existing, deliberate scope boundary (same as reprieve/stand-your-
/// ground), not something this adapter introduces.
/// </summary>
public sealed class HidaKisadaBotAction : IBotScriptAction
{
    public bool IsLegal(GameState game, Card source, Player actingPlayer) =>
        game.CurrentConflict is not null
        && source.Location == "play area"
        && !game.IsBlanked(source)
        && !game.ConflictRecord.Any(c => c.Winner == game.Opponent(actingPlayer))
        && !game.FirstActionCancelledThisConflict;

    public void Invoke(GameState game, Card source, Player actingPlayer)
    {
        var context = new AbilityContext { Game = game, Player = actingPlayer, Source = source };
        new HidaKisadaCancelOpponentsFirstActionEachConflict().Execute(context);
    }
}
