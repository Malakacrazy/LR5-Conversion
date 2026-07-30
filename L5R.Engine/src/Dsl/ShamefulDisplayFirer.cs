using System.Linq;
using L5R.Engine.Abilities;
using L5R.Engine.Cards.Scripts;
using L5R.Engine.State;

namespace L5R.Engine.Dsl;

/// <summary>
/// shameful-display: choose 2 participating characters, honor one and dishonor the other.
/// The script itself never checks Conflict.DeclaredProvince (it only needs both targets to
/// be participants) - this adapter adds that gate, matching the real card's "after a
/// conflict is declared against this province" trigger, since the script trusts the caller
/// to know when that's appropriate. Same conflict-declaration hook site as SecretCacheFirer.
/// Prefers dishonoring an opponent's participant and honoring its own, the same "aggressive
/// first choice" heuristic as court-games/asako-diplomat.
/// </summary>
public static class ShamefulDisplayFirer
{
    public static void FireIfLegal(GameState game, Card province)
    {
        if (province.Id != "shameful-display" || game.CurrentConflict is not { } conflict || conflict.DeclaredProvince != province)
            return;

        var controller = province.Controller;
        var opponent = game.Opponent(controller);
        var honorTarget = conflict.Attackers.Concat(conflict.Defenders).FirstOrDefault(c => c.Controller == controller);
        var dishonorTarget = conflict.Attackers.Concat(conflict.Defenders).FirstOrDefault(c => c.Controller == opponent);
        if (honorTarget is null || dishonorTarget is null)
            return;

        var context = new AbilityContext { Game = game, Player = controller, Source = province, Target = honorTarget, SecondTarget = dishonorTarget };
        new ShamefulDisplayHonorOneDishonorOther().Execute(context);
    }
}
