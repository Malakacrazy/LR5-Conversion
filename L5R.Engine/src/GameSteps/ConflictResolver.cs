using System.Linq;
using L5R.Engine.Abilities;
using L5R.Engine.Dsl.GameActions;
using L5R.Engine.State;

namespace L5R.Engine.GameSteps;

/// <summary>
/// The minimal-but-real conflict declaration/resolution sequence ringteki's ConflictFlow
/// (gamesteps/conflict/conflictflow.js) implements in 19 sub-steps: declare ring+province+
/// attackers -> declare defenders -> sum skill -> ties favor the attacker -> break the
/// province if the attacker wins by enough (or unconditionally for the stronghold, once
/// eligible - conquest) -> resolve the ring's element -> claim the ring -> an unopposed
/// loser loses 1 honor -> bow and return participants home (already done at declaration/
/// defense time here, matching this engine's "commit = bow immediately" convention rather
/// than a separate return-home step).
///
/// Deliberately out of scope for v1 (none of it is needed to play a real, complete game
/// with the generic-DSL card set): the covert keyword, ring-switching mid-declaration, the
/// mid-conflict action-ability window (ConflictActionWindow's alternating-actions loop -
/// nothing here has an in-conflict action to trigger), and duels (a separate,
/// card-triggered subsystem, not part of the base conflict flow).
/// </summary>
public static class ConflictResolver
{
    /// <summary>A player's own unbroken provinces, plus their Stronghold once 2+ of the other 4 are already broken (real rule) - Stronghold isn't part of Provinces (see its own doc comment), so it's appended here rather than folded in.</summary>
    public static IReadOnlyList<Card> AttackableProvinces(Player defender)
    {
        var result = defender.Provinces.Where(p => !p.Broken).ToList();
        if (defender.Provinces.Count(p => p.Broken) >= 2 && defender.Stronghold is { Broken: false } stronghold)
            result.Add(stronghold);

        return result;
    }

    public static IReadOnlyList<Card> EligibleAttackers(GameState game, Player attacker) =>
        attacker.PlayArea
            .Where(c => c.Type == CardType.Character && !c.Bowed && !game.IsRestrictedFrom(c, "declareAsAttacker"))
            .ToList();

    public static IReadOnlyList<Card> EligibleDefenders(GameState game, Player defender) =>
        defender.PlayArea
            .Where(c => c.Type == CardType.Character && !c.Bowed && !game.IsRestrictedFrom(c, "declareAsDefender"))
            .ToList();

    public static void Resolve(GameState game, Player attacker, ConflictDeclaration declaration, IBotPolicy defenderPolicy)
    {
        var defender = game.Opponent(attacker);
        var ring = game.Rings.First(r => r.Element == declaration.Ring);

        var conflict = new Conflict
        {
            AttackingPlayer = attacker,
            DefendingPlayer = defender,
            ConflictType = ring.ConflictType,
            DeclaredProvince = declaration.Province
        };
        conflict.Elements.Add(declaration.Ring);
        game.CurrentConflict = conflict;
        ring.Contested = true;

        foreach (var card in declaration.Attackers)
        {
            conflict.Attackers.Add(card);
            card.Bowed = true;
        }

        declaration.Province.Facedown = false;

        var defenders = defenderPolicy.DeclareDefenders(game, conflict, defender);
        foreach (var card in defenders)
        {
            conflict.Defenders.Add(card);
            card.Bowed = true;
        }
        conflict.Unopposed = defenders.Count == 0;

        conflict.AttackerSkill = SumSkill(game, conflict.Attackers, conflict.ConflictType!);
        conflict.DefenderSkill = SumSkill(game, conflict.Defenders, conflict.ConflictType!);
        conflict.SkillDifference = conflict.AttackerSkill - conflict.DefenderSkill;

        if (conflict.AttackerSkill != 0 || conflict.DefenderSkill != 0)
        {
            if (conflict.AttackerSkill >= conflict.DefenderSkill)
            {
                conflict.Winner = attacker;
                conflict.Loser = defender;
            }
            else
            {
                conflict.Winner = defender;
                conflict.Loser = attacker;
            }
        }

        if (conflict.Unopposed && conflict.Loser is not null)
            conflict.Loser.Honor -= 1;

        if (conflict.Winner == attacker)
        {
            TryBreakProvince(game, defender, declaration.Province, conflict);

            ResolveConflictRingGameActionHandler.ResolveElement(declaration.Ring,
                new AbilityContext { Game = game, Player = attacker, Source = attacker.Stronghold! });

            if (!ring.Claimed)
            {
                ring.Claimed = true;
                ring.ClaimedBy = attacker;
            }
        }

        ring.Contested = false;
        game.ConflictRecord.Add(conflict);
        game.EndConflict();
    }

    /// <summary>Stronghold provinces have no printed strength and break unconditionally on an attacker win (real rule - see AttackableProvinces for the "2+ other provinces broken" eligibility gate) - the conquest win itself.</summary>
    private static void TryBreakProvince(GameState game, Player defender, Card province, Conflict conflict)
    {
        var isStronghold = province == defender.Stronghold;
        var breaks = isStronghold || conflict.SkillDifference >= game.EffectiveProvinceStrength(province);
        if (!breaks || province.Broken)
            return;

        province.Broken = true;

        if (isStronghold)
        {
            game.Winner = conflict.AttackingPlayer;
            return;
        }

        defender.Provinces.Remove(province);
        province.Location = "discard";
        province.Facedown = false;
        defender.Discard.Add(province);
    }

    private static int SumSkill(GameState game, IEnumerable<Card> participants, string conflictType) =>
        participants.Sum(card => conflictType == "military" ? game.EffectiveMilitarySkill(card) : game.EffectivePoliticalSkill(card));
}
