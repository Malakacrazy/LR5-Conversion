using System.Text.Json;
using L5R.Engine.Abilities;
using L5R.Engine.Dsl;
using L5R.Engine.Dsl.GameActions;
using L5R.Engine.GameSteps;
using L5R.Engine.Scheduling;
using L5R.Engine.State;
using L5R.Engine.Tests.GameSteps;

namespace L5R.Engine.Tests.Dsl;

public class TriggeredReactionFirerTests
{
    private static TriggeredAbilityDefinition LoadAbility(string cardId)
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Cards", "01-Core", $"{cardId}.json");
        using var document = JsonDocument.Parse(File.ReadAllText(path));
        return AbilityDefinitionParser.ParseTriggeredAbilities(document.RootElement).Single();
    }

    private static (Player p1, Player p2, GameState game) NewGame()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        return (p1, p2, game);
    }

    [Test]
    public void HonoredGeneral_OnCharacterEntersPlay_HonorsItself()
    {
        var (p1, _, game) = NewGame();
        var honoredGeneral = new Card { Id = "honored-general", Type = CardType.Character, Controller = p1, TriggeredAbilities = new[] { LoadAbility("honored-general") } };
        p1.PlayArea.Add(honoredGeneral);

        TriggeredReactionFirer.FireIfLegal(game, honoredGeneral, "onCharacterEntersPlay");

        Assert.That(honoredGeneral.IsHonored, Is.True);
    }

    [Test]
    public void MatsuBeiona_WithThreeOtherBushiAllies_PlacesTwoFate()
    {
        var (p1, _, game) = NewGame();
        var beiona = new Card { Id = "matsu-beiona", Type = CardType.Character, Controller = p1, TriggeredAbilities = new[] { LoadAbility("matsu-beiona") } };
        p1.PlayArea.Add(beiona);
        for (var i = 0; i < 3; i++)
            p1.PlayArea.Add(new Card { Id = $"bushi-{i}", Type = CardType.Character, Controller = p1, Traits = new[] { "bushi" } });

        TriggeredReactionFirer.FireIfLegal(game, beiona, "onCharacterEntersPlay");

        Assert.That(beiona.Fate, Is.EqualTo(2));
    }

    [Test]
    public void MatsuBeiona_WithFewerThanThreeOtherBushiAllies_DoesNotFire()
    {
        var (p1, _, game) = NewGame();
        var beiona = new Card { Id = "matsu-beiona", Type = CardType.Character, Controller = p1, TriggeredAbilities = new[] { LoadAbility("matsu-beiona") } };
        p1.PlayArea.Add(beiona);
        p1.PlayArea.Add(new Card { Id = "bushi-0", Type = CardType.Character, Controller = p1, Traits = new[] { "bushi" } });

        TriggeredReactionFirer.FireIfLegal(game, beiona, "onCharacterEntersPlay");

        Assert.That(beiona.Fate, Is.EqualTo(0));
    }

    [Test]
    public void IuchiWayfinder_WithAFacedownOpponentProvince_DoesNotThrow()
    {
        var (p1, p2, game) = NewGame();
        var wayfinder = new Card { Id = "iuchi-wayfinder", Type = CardType.Character, Controller = p1, TriggeredAbilities = new[] { LoadAbility("iuchi-wayfinder") } };
        p1.PlayArea.Add(wayfinder);
        p2.Provinces.Add(new Card { Id = "facedown-province", Type = CardType.Province, Controller = p2, Facedown = true });

        Assert.DoesNotThrow(() => TriggeredReactionFirer.FireIfLegal(game, wayfinder, "onCharacterEntersPlay"));
    }

    [Test]
    public void IuchiWayfinder_WithNoFacedownOpponentProvince_DoesNotThrow()
    {
        // Matches real bot-driven games: GameLoop's DynastyPhaseStep flips every province
        // face-up unconditionally, so this reaction's own target never has a legal candidate
        // in practice - a pre-existing gap independent of this firing mechanism, not
        // something to work around here.
        var (p1, p2, game) = NewGame();
        var wayfinder = new Card { Id = "iuchi-wayfinder", Type = CardType.Character, Controller = p1, TriggeredAbilities = new[] { LoadAbility("iuchi-wayfinder") } };
        p1.PlayArea.Add(wayfinder);
        p2.Provinces.Add(new Card { Id = "faceup-province", Type = CardType.Province, Controller = p2, Facedown = false });

        Assert.DoesNotThrow(() => TriggeredReactionFirer.FireIfLegal(game, wayfinder, "onCharacterEntersPlay"));
    }

    [Test]
    public void HirumaAmbusher_WhileDefending_DisablesACharactersTriggeredAbilities()
    {
        var (p1, p2, game) = NewGame();
        var ambusher = new Card { Id = "hiruma-ambusher", Type = CardType.Character, Controller = p1, TriggeredAbilities = new[] { LoadAbility("hiruma-ambusher") } };
        p1.PlayArea.Add(ambusher);
        var attacker = new Card { Id = "attacker", Type = CardType.Character, Controller = p2 };
        p2.PlayArea.Add(attacker);

        var conflict = new Conflict { AttackingPlayer = p2, DefendingPlayer = p1 };
        conflict.Defenders.Add(ambusher);
        conflict.Attackers.Add(attacker);
        game.CurrentConflict = conflict;

        TriggeredReactionFirer.FireIfLegal(game, ambusher, "onCharacterEntersPlay");

        // The JSON target has no controller filter, and GameState.AllCards() enumerates
        // Player1 before Player2 - so the "first legal candidate" heuristic lands on
        // ambusher itself here (P1's own PlayArea comes first). Not the real card's likely
        // flavor intent, but a pre-existing, already-accepted heuristic limitation (same
        // shape as GiverOfGiftsBotAction's own documented "targets itself first" quirk).
        Assert.That(game.IsRestrictedFrom(ambusher, "triggerAbilities"), Is.True);
    }

    [Test]
    public void HirumaAmbusher_WhileNotDefending_DoesNotFire()
    {
        var (p1, p2, game) = NewGame();
        var ambusher = new Card { Id = "hiruma-ambusher", Type = CardType.Character, Controller = p1, TriggeredAbilities = new[] { LoadAbility("hiruma-ambusher") } };
        p1.PlayArea.Add(ambusher);
        var attacker = new Card { Id = "attacker", Type = CardType.Character, Controller = p2 };
        p2.PlayArea.Add(attacker);

        var conflict = new Conflict { AttackingPlayer = p1, DefendingPlayer = p2 };
        conflict.Attackers.Add(ambusher);
        game.CurrentConflict = conflict;

        TriggeredReactionFirer.FireIfLegal(game, ambusher, "onCharacterEntersPlay");

        Assert.That(game.IsRestrictedFrom(ambusher, "triggerAbilities"), Is.False);
    }

    [Test]
    public void HirumaAmbusher_TargetingAFriendlyShugenjaWithShibaYojimboInPlay_GetsCancelled()
    {
        // Proves the Prepare -> offer shiba-yojimbo -> Resolve split: without shiba-yojimbo in
        // play, this exact setup would restrict the shugenja (see the sibling test above) -
        // here it must not, because shiba-yojimbo cancels the pending ability first.
        var (p1, p2, game) = NewGame();
        var shugenja = new Card { Id = "my-shugenja", Type = CardType.Character, Controller = p1, Traits = new[] { "shugenja" } };
        var ambusher = new Card { Id = "hiruma-ambusher", Type = CardType.Character, Controller = p1, TriggeredAbilities = new[] { LoadAbility("hiruma-ambusher") } };
        var yojimbo = new Card { Id = "shiba-yojimbo", Type = CardType.Character, Controller = p1 };
        // Insertion order matters: TargetResolver.ResolveLegalTargets picks the first
        // character-type candidate in GameState.AllCards() order, so shugenja must be added
        // before ambusher/yojimbo for it to be the one hiruma-ambusher's reaction targets.
        p1.PlayArea.Add(shugenja);
        p1.PlayArea.Add(ambusher);
        p1.PlayArea.Add(yojimbo);
        var attacker = new Card { Id = "attacker", Type = CardType.Character, Controller = p2 };
        p2.PlayArea.Add(attacker);

        var conflict = new Conflict { AttackingPlayer = p2, DefendingPlayer = p1 };
        conflict.Defenders.Add(ambusher);
        conflict.Attackers.Add(attacker);
        game.CurrentConflict = conflict;

        TriggeredReactionFirer.FireIfLegal(game, ambusher, "onCharacterEntersPlay");

        Assert.That(game.IsRestrictedFrom(shugenja, "triggerAbilities"), Is.False);
    }

    [Test]
    public void ElementalFury_DuringAConflict_SwitchesTheContestedRing()
    {
        var (p1, p2, game) = NewGame();
        var province = new Card { Id = "elemental-fury", Type = CardType.Province, Controller = p1, TriggeredAbilities = new[] { LoadAbility("elemental-fury") } };
        p1.Provinces.Add(province);

        var conflict = new Conflict { AttackingPlayer = p2, DefendingPlayer = p1 };
        conflict.Elements.Add("water");
        game.CurrentConflict = conflict;

        TriggeredReactionFirer.FireIfLegal(game, province, "onCardRevealed");

        Assert.That(conflict.Elements, Is.EqualTo(new[] { "air" }), "the first ring in GameState.Rings that isn't the current element");
    }

    [Test]
    public void NightRaid_OnReveal_ForcesTheAttackerToDiscardOneCardPerAttacker()
    {
        var (p1, p2, game) = NewGame();
        var province = new Card { Id = "night-raid", Type = CardType.Province, Controller = p1, TriggeredAbilities = new[] { LoadAbility("night-raid") } };
        p1.Provinces.Add(province);
        p2.Hand.AddRange(new[]
        {
            new Card { Id = "hand-1", Type = CardType.Character, Controller = p2, Location = "hand" },
            new Card { Id = "hand-2", Type = CardType.Character, Controller = p2, Location = "hand" },
            new Card { Id = "hand-3", Type = CardType.Character, Controller = p2, Location = "hand" },
        });

        var conflict = new Conflict { AttackingPlayer = p2, DefendingPlayer = p1 };
        conflict.Attackers.Add(new Card { Id = "attacker-1", Type = CardType.Character, Controller = p2 });
        conflict.Attackers.Add(new Card { Id = "attacker-2", Type = CardType.Character, Controller = p2 });
        game.CurrentConflict = conflict;

        TriggeredReactionFirer.FireIfLegal(game, province, "onCardRevealed");

        Assert.That(p2.Hand, Has.Count.EqualTo(1));
        Assert.That(p2.Discard, Has.Count.EqualTo(2));
    }

    [Test]
    public void RallyToTheCause_DuringAConflict_SwitchesTheConflictType()
    {
        var (p1, p2, game) = NewGame();
        var province = new Card { Id = "rally-to-the-cause", Type = CardType.Province, Controller = p1, TriggeredAbilities = new[] { LoadAbility("rally-to-the-cause") } };
        p1.Provinces.Add(province);

        var conflict = new Conflict { AttackingPlayer = p2, DefendingPlayer = p1, ConflictType = "military" };
        game.CurrentConflict = conflict;

        TriggeredReactionFirer.FireIfLegal(game, province, "onCardRevealed");

        Assert.That(conflict.ConflictType, Is.EqualTo("political"));
    }

    [Test]
    public void RestorationOfBalance_OnBreak_ForcesOpponentToDiscardDownToFour()
    {
        var (p1, p2, game) = NewGame();
        var province = new Card { Id = "restoration-of-balance", Type = CardType.Province, Controller = p1, TriggeredAbilities = new[] { LoadAbility("restoration-of-balance") } };
        p1.Provinces.Add(province);
        for (var i = 0; i < 6; i++)
            p2.Hand.Add(new Card { Id = $"hand-{i}", Type = CardType.Character, Controller = p2, Location = "hand" });

        var conflict = new Conflict { AttackingPlayer = p2, DefendingPlayer = p1 };
        game.CurrentConflict = conflict;

        TriggeredReactionFirer.FireIfLegal(game, province, "onBreakProvince");

        Assert.That(p2.Hand, Has.Count.EqualTo(4));
        Assert.That(p2.Discard, Has.Count.EqualTo(2));
    }

    [Test]
    public void TheArtOfPeace_OnBreak_HonorsDefendersAndDishonorsAttackers()
    {
        var (p1, p2, game) = NewGame();
        var province = new Card { Id = "the-art-of-peace", Type = CardType.Province, Controller = p1, TriggeredAbilities = new[] { LoadAbility("the-art-of-peace") } };
        p1.Provinces.Add(province);
        var defender = new Card { Id = "defender", Type = CardType.Character, Controller = p1 };
        var attacker = new Card { Id = "attacker", Type = CardType.Character, Controller = p2 };
        p1.PlayArea.Add(defender);
        p2.PlayArea.Add(attacker);

        var conflict = new Conflict { AttackingPlayer = p2, DefendingPlayer = p1 };
        conflict.Attackers.Add(attacker);
        conflict.Defenders.Add(defender);
        game.CurrentConflict = conflict;

        TriggeredReactionFirer.FireIfLegal(game, province, "onBreakProvince");

        Assert.That(defender.IsHonored, Is.True);
        Assert.That(attacker.IsDishonored, Is.True);
    }

    // Integration tests proving the hooks are actually wired into production code, not just
    // callable in isolation.

    [Test]
    public void PlayCardGameActionHandler_PlayingACharacterFromHand_FiresItsOwnCharacterEntersPlayReaction()
    {
        var (p1, _, game) = NewGame();
        var honoredGeneral = new Card
        {
            Id = "honored-general", Type = CardType.Character, Controller = p1, Location = "hand",
            PrintedCost = 0, TriggeredAbilities = new[] { LoadAbility("honored-general") }
        };
        p1.Hand.Add(honoredGeneral);

        var context = new AbilityContext { Game = game, Player = p1, Source = honoredGeneral };
        new PlayCardGameActionHandler().Execute(context, null);

        Assert.That(honoredGeneral.IsHonored, Is.True);
    }

    [Test]
    public void ConflictResolver_BreakingAProvince_FiresItsOwnBreakProvinceReaction()
    {
        var (p1, p2, game) = NewGame();
        p1.Stronghold = new Card { Id = "sh1", Type = CardType.Stronghold, Controller = p1 };
        p2.Stronghold = new Card { Id = "sh2", Type = CardType.Stronghold, Controller = p2 };
        var province = new Card
        {
            Id = "the-art-of-war", Type = CardType.Province, Controller = p2, PrintedProvinceStrength = 3,
            TriggeredAbilities = new[] { LoadAbility("the-art-of-war") }
        };
        p2.Provinces.Add(province);
        p2.Deck.AddRange(new[]
        {
            new Card { Id = "deck-1", Type = CardType.Character, Controller = p2 },
            new Card { Id = "deck-2", Type = CardType.Character, Controller = p2 },
            new Card { Id = "deck-3", Type = CardType.Character, Controller = p2 },
        });
        var attacker = new Card { Id = "attacker", Type = CardType.Character, Controller = p1, PrintedMilitarySkill = 5 };
        p1.PlayArea.Add(attacker);

        var scheduler = new Scheduler();
        scheduler.QueueStep(ConflictResolver.Resolve(game, p1, new ConflictDeclaration("fire", province, new[] { attacker }), new FixedDefendersBotPolicy()));
        scheduler.Pump();

        Assert.That(province.Broken, Is.True);
        Assert.That(p2.Hand, Has.Count.EqualTo(3), "the-art-of-war's own reaction drew 3 cards for its controller (p2), not the winning attacker");
    }
}
