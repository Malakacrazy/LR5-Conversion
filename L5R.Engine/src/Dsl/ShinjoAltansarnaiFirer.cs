using System.Linq;
using L5R.Engine.Abilities;
using L5R.Engine.Cards.Scripts;
using L5R.Engine.State;

namespace L5R.Engine.Dsl;

/// <summary>
/// shinjo-altansarnai: after breaking a province during a military conflict while attacking,
/// the opponent discards one of their own characters. Fires directly inside
/// ConflictResolver.TryBreakProvince right after province.Broken is set - the exact moment
/// a province break actually happens, matching TriggeredReactionFirer's own "onBreakProvince"
/// call one line above. Picks the opponent's first PlayArea character deterministically
/// (same "no target-selection policy" convention as SecludedTempleBotAction's own FindTarget)
/// rather than through IBotPolicy.
/// </summary>
public static class ShinjoAltansarnaiFirer
{
    public static void FireIfLegal(GameState game, Conflict conflict)
    {
        if (conflict.ConflictType != "military")
            return;

        foreach (var altansarnai in conflict.Attackers.Where(c => c.Id == "shinjo-altansarnai" && !game.IsBlanked(c)).ToList())
        {
            var opponent = game.Opponent(altansarnai.Controller);
            var target = opponent.PlayArea.FirstOrDefault(c => c.Type == CardType.Character);
            if (target is null)
                continue;

            var context = new AbilityContext { Game = game, Player = altansarnai.Controller, Source = altansarnai, Target = target };
            new ShinjoAltansarnaiDiscardOnMilitaryProvinceBreak().Execute(context);
        }
    }
}
