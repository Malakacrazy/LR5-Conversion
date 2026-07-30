using System.Linq;
using L5R.Engine.Abilities;
using L5R.Engine.Cards.Scripts;
using L5R.Engine.State;

namespace L5R.Engine.Dsl;

/// <summary>
/// endless-plains: after a conflict is declared against this province, break itself as a
/// cost to discard an attacking character. Same conflict-declaration hook site as
/// SecretCacheFirer - see its own doc comment for why a province needs a direct hook rather
/// than the general bot scan.
/// </summary>
public static class EndlessPlainsFirer
{
    public static void FireIfLegal(GameState game, Card province)
    {
        if (province.Id != "endless-plains" || province.Broken || game.CurrentConflict is not { } conflict || conflict.DeclaredProvince != province)
            return;

        var target = conflict.Attackers.FirstOrDefault();
        if (target is null)
            return;

        var context = new AbilityContext { Game = game, Player = province.Controller, Source = province, Target = target };
        new EndlessPlainsBreakAndDiscardAttacker().Execute(context);
    }
}
