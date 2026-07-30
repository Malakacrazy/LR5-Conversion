using L5R.Engine.Abilities;
using L5R.Engine.Cards.Scripts;
using L5R.Engine.State;

namespace L5R.Engine.Dsl;

/// <summary>
/// pilgrimage: during a conflict declared against this province, cancel the ring's own
/// effect. Fired at the same conflict-declaration hook site as SecretCacheFirer -
/// guaranteed to run before ConflictResolver ever resolves the ring's effect later in the
/// same Resolve call.
/// </summary>
public static class PilgrimageFirer
{
    public static void FireIfLegal(GameState game, Card province)
    {
        if (province.Id != "pilgrimage" || province.Broken || game.IsBlanked(province))
            return;

        if (game.CurrentConflict is not { } conflict || conflict.DeclaredProvince != province)
            return;

        var context = new AbilityContext { Game = game, Player = province.Controller, Source = province };
        new PilgrimageCancelRingEffectsAtThisProvince().Execute(context);
    }
}
