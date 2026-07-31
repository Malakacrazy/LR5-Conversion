using L5R.Engine.Abilities;
using L5R.Engine.GameSteps;
using L5R.Engine.Logging;
using L5R.Engine.Scheduling;
using L5R.Engine.State;

namespace L5R.Engine.Tests.GameSteps;

/// <summary>Only exercises DeclareDefenders - ConflictResolver.Resolve never calls anything else on the defender's policy.</summary>
public sealed class FixedDefendersBotPolicy : IBotPolicy
{
    private readonly IReadOnlyList<Card> _defenders;

    public FixedDefendersBotPolicy(params Card[] defenders) => _defenders = defenders;

    public Task<CardAction?> ChooseAction(GameState game, Player player) => throw new NotSupportedException();
    public Task<Card?> ChoosePlay(GameState game, Player player, string location) => throw new NotSupportedException();
    public Task<int> ChooseHonorBid(GameState game, Player player) => throw new NotSupportedException();
    public Task<ConflictDeclaration?> DeclareConflict(GameState game, Player player) => throw new NotSupportedException();
    public Task<IReadOnlyList<Card>> DeclareDefenders(GameState game, Conflict conflict, Player defender) => Task.FromResult(_defenders);
    public Task<(Card Source, IBotScriptAction Action)?> ChooseScriptedAction(GameState game, Player player) => throw new NotSupportedException();
    public Task<IBotScriptAction?> ResolveEventScript(string cardId) => throw new NotSupportedException();
}

public class ConflictResolverTests
{
    /// <summary>
    /// ConflictResolver.Resolve is an IEnumerator (see its own doc comment - it can pause on
    /// a human-backed policy's DeclareDefenders/mid-conflict window), so calling it directly
    /// does nothing until something drives it. Every bot policy here resolves synchronously
    /// (Task.FromResult), so a single Scheduler.Pump() always drains it in one shot.
    /// </summary>
    private static void Resolve(GameState game, Player attacker, ConflictDeclaration declaration, IBotPolicy defenderPolicy, IBotPolicy? attackerPolicy = null, EventLog? eventLog = null)
    {
        var scheduler = new Scheduler();
        scheduler.QueueStep(ConflictResolver.Resolve(game, attacker, declaration, defenderPolicy, attackerPolicy, eventLog));
        scheduler.Pump();
    }

    private static (GameState game, Player p1, Player p2) NewGame()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        p1.Stronghold = new Card { Id = "sh1", Type = CardType.Stronghold, Controller = p1 };
        p2.Stronghold = new Card { Id = "sh2", Type = CardType.Stronghold, Controller = p2 };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        return (game, p1, p2);
    }

    [Test]
    public void AttackerWinsByEnoughSkill_BreaksTheProvinceAndClaimsTheRing()
    {
        var (game, p1, p2) = NewGame();
        var attacker = new Card { Id = "attacker", Type = CardType.Character, Controller = p1, PrintedMilitarySkill = 5 };
        var province = new Card { Id = "province", Type = CardType.Province, Controller = p2, PrintedProvinceStrength = 3 };
        p1.PlayArea.Add(attacker);
        p2.Provinces.Add(province);

        Resolve(game, p1, new ConflictDeclaration("fire", province, new[] { attacker }), new FixedDefendersBotPolicy());

        var conflict = game.ConflictRecord.Single();
        Assert.That(conflict.Winner, Is.EqualTo(p1));
        Assert.That(province.Broken, Is.True);
        Assert.That(p2.Provinces, Does.Not.Contain(province));
        Assert.That(p2.Discard, Contains.Item(province));
        Assert.That(game.Rings.Single(r => r.Element == "fire").ClaimedBy, Is.EqualTo(p1));
        Assert.That(game.CurrentConflict, Is.Null, "EndConflict cleared it");
    }

    [Test]
    public void AttackerWinsButNotByEnoughSkill_DoesNotBreakTheProvince_ButStillClaimsTheRing()
    {
        var (game, p1, p2) = NewGame();
        var attacker = new Card { Id = "attacker", Type = CardType.Character, Controller = p1, PrintedMilitarySkill = 2 };
        var province = new Card { Id = "province", Type = CardType.Province, Controller = p2, PrintedProvinceStrength = 5 };
        p1.PlayArea.Add(attacker);
        p2.Provinces.Add(province);

        Resolve(game, p1, new ConflictDeclaration("air", province, new[] { attacker }), new FixedDefendersBotPolicy());

        Assert.That(province.Broken, Is.False);
        Assert.That(p2.Provinces, Contains.Item(province));
        Assert.That(game.Rings.Single(r => r.Element == "air").ClaimedBy, Is.EqualTo(p1));
    }

    [Test]
    public void DefenderWinsWithHigherSkill_NoBreakAndNoClaim()
    {
        var (game, p1, p2) = NewGame();
        var attacker = new Card { Id = "attacker", Type = CardType.Character, Controller = p1, PrintedMilitarySkill = 1 };
        var defenderChar = new Card { Id = "defender", Type = CardType.Character, Controller = p2, PrintedMilitarySkill = 4 };
        var province = new Card { Id = "province", Type = CardType.Province, Controller = p2, PrintedProvinceStrength = 1 };
        p1.PlayArea.Add(attacker);
        p2.PlayArea.Add(defenderChar);
        p2.Provinces.Add(province);

        Resolve(game, p1, new ConflictDeclaration("water", province, new[] { attacker }), new FixedDefendersBotPolicy(defenderChar));

        var conflict = game.ConflictRecord.Single();
        Assert.That(conflict.Winner, Is.EqualTo(p2));
        Assert.That(province.Broken, Is.False);
        Assert.That(game.Rings.Single(r => r.Element == "water").Claimed, Is.False);
        Assert.That(defenderChar.Bowed, Is.True, "defenders bow when they commit, regardless of outcome");
    }

    [Test]
    public void TiedSkill_FavorsTheAttacker()
    {
        var (game, p1, p2) = NewGame();
        var attacker = new Card { Id = "attacker", Type = CardType.Character, Controller = p1, PrintedMilitarySkill = 3 };
        var defenderChar = new Card { Id = "defender", Type = CardType.Character, Controller = p2, PrintedMilitarySkill = 3 };
        var province = new Card { Id = "province", Type = CardType.Province, Controller = p2, PrintedProvinceStrength = 0 };
        p1.PlayArea.Add(attacker);
        p2.PlayArea.Add(defenderChar);
        p2.Provinces.Add(province);

        Resolve(game, p1, new ConflictDeclaration("fire", province, new[] { attacker }), new FixedDefendersBotPolicy(defenderChar));

        Assert.That(game.ConflictRecord.Single().Winner, Is.EqualTo(p1));
    }

    [Test]
    public void BothZeroSkill_NoWinner()
    {
        var (game, p1, p2) = NewGame();
        var attacker = new Card { Id = "attacker", Type = CardType.Character, Controller = p1, PrintedMilitarySkill = 0 };
        var province = new Card { Id = "province", Type = CardType.Province, Controller = p2, PrintedProvinceStrength = 0 };
        p1.PlayArea.Add(attacker);
        p2.Provinces.Add(province);

        Resolve(game, p1, new ConflictDeclaration("fire", province, new[] { attacker }), new FixedDefendersBotPolicy());

        Assert.That(game.ConflictRecord.Single().Winner, Is.Null);
        Assert.That(province.Broken, Is.False, "no winner means the attacker didn't win, so nothing breaks");
    }

    [Test]
    public void UnopposedConflict_TheLoserLoses1Honor()
    {
        var (game, p1, p2) = NewGame();
        p2.Honor = 5;
        var attacker = new Card { Id = "attacker", Type = CardType.Character, Controller = p1, PrintedMilitarySkill = 1 };
        var province = new Card { Id = "province", Type = CardType.Province, Controller = p2, PrintedProvinceStrength = 10 };
        p1.PlayArea.Add(attacker);
        p2.Provinces.Add(province);

        Resolve(game, p1, new ConflictDeclaration("fire", province, new[] { attacker }), new FixedDefendersBotPolicy());

        Assert.That(game.ConflictRecord.Single().Unopposed, Is.True);
        Assert.That(p2.Honor, Is.EqualTo(4));
    }

    [Test]
    public void WinningAgainstTheStrongholdOnceEligible_BreaksItUnconditionallyAndWinsTheGame()
    {
        var (game, p1, p2) = NewGame();
        p2.Provinces.Add(new Card { Id = "broken-1", Type = CardType.Province, Controller = p2, Broken = true });
        p2.Provinces.Add(new Card { Id = "broken-2", Type = CardType.Province, Controller = p2, Broken = true });
        var attacker = new Card { Id = "attacker", Type = CardType.Character, Controller = p1, PrintedMilitarySkill = 1 };
        p1.PlayArea.Add(attacker);

        Assert.That(ConflictResolver.AttackableProvinces(p2), Contains.Item(p2.Stronghold));

        Resolve(game, p1, new ConflictDeclaration("fire", p2.Stronghold!, new[] { attacker }), new FixedDefendersBotPolicy());

        Assert.That(p2.Stronghold!.Broken, Is.True);
        Assert.That(game.Winner, Is.EqualTo(p1));
    }

    [Test]
    public void StrongholdIsNotAttackableWithFewerThanTwoBrokenProvinces()
    {
        var (_, _, p2) = NewGame();
        p2.Provinces.Add(new Card { Id = "broken-1", Type = CardType.Province, Controller = p2, Broken = true });

        Assert.That(ConflictResolver.AttackableProvinces(p2), Does.Not.Contain(p2.Stronghold));
    }

    [Test]
    public void WithAnAttackerPolicySupplied_RunsAMidConflictActionWindow_LettingTheAttackerPlayOutwit()
    {
        // Proves the mid-conflict window itself, not just OutwitBotAction in isolation: the
        // event is played from hand *during conflict resolution*, sending home the defender
        // it outclasses, before skill is ever summed.
        var p1 = new Player { Name = "Player1", Fate = 5 };
        var p2 = new Player { Name = "Player2" };
        p1.Stronghold = new Card { Id = "sh1", Type = CardType.Stronghold, Controller = p1 };
        p2.Stronghold = new Card { Id = "sh2", Type = CardType.Stronghold, Controller = p2 };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };

        var myCourtier = new Card { Id = "my-courtier", Type = CardType.Character, Controller = p1, Traits = new[] { "courtier" }, PrintedPoliticalSkill = 5 };
        p1.PlayArea.Add(myCourtier);

        var outwit = new Card { Id = "outwit", Type = CardType.Event, Controller = p1, PrintedCost = 0 };
        p1.Hand.Add(outwit);

        var weakDefender = new Card { Id = "weak-defender", Type = CardType.Character, Controller = p2, PrintedPoliticalSkill = 2 };
        p2.PlayArea.Add(weakDefender);

        var province = new Card { Id = "province", Type = CardType.Province, Controller = p2, PrintedProvinceStrength = 10 };
        p2.Provinces.Add(province);

        var registry = new ScriptedActionRegistry();
        var attackerPolicy = new FirstLegalActionBotPolicy(registry);
        var defenderPolicy = new FirstLegalActionBotPolicy(registry);

        Resolve(game, p1, new ConflictDeclaration("fire", province, new[] { myCourtier }), defenderPolicy, attackerPolicy);

        var conflict = game.ConflictRecord.Single();
        Assert.That(conflict.Defenders, Does.Not.Contain(weakDefender), "outwit sent it home before skill was summed");
        Assert.That(conflict.Unopposed, Is.True, "the only defender was sent home, recomputed after the window");
        Assert.That(p1.Discard, Contains.Item(outwit));
    }

    [Test]
    public void WithAnAttackerPolicySupplied_DiscoversAndActivatesAStrongholdsOwnScriptedAction()
    {
        // Proves FirstLegalActionBotPolicy.ChooseScriptedAction's own stronghold check, not
        // just MountainsAnvilCastleBotAction in isolation - GameState.AllCards() never
        // includes Player.Stronghold, so without that explicit check this would never be
        // discovered at all.
        var p1 = new Player { Name = "Player1", Fate = 5 };
        var p2 = new Player { Name = "Player2" };
        p1.Stronghold = new Card { Id = "mountain-s-anvil-castle", Type = CardType.Stronghold, Controller = p1 };
        p2.Stronghold = new Card { Id = "sh2", Type = CardType.Stronghold, Controller = p2 };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };

        var myCourtier = new Card { Id = "my-courtier", Type = CardType.Character, Controller = p1, PrintedMilitarySkill = 2 };
        var myAttachment = new Card { Id = "my-attachment", Type = CardType.Attachment, Controller = p1, AttachedTo = myCourtier };
        p1.PlayArea.Add(myCourtier);
        p1.PlayArea.Add(myAttachment);

        var province = new Card { Id = "province", Type = CardType.Province, Controller = p2, PrintedProvinceStrength = 10 };
        p2.Provinces.Add(province);

        var registry = new ScriptedActionRegistry();
        var attackerPolicy = new FirstLegalActionBotPolicy(registry);
        var defenderPolicy = new FirstLegalActionBotPolicy(registry);

        Resolve(game, p1, new ConflictDeclaration("fire", province, new[] { myCourtier }), defenderPolicy, attackerPolicy);

        // The skill bonus itself is an untilEndOfConflict LastingEffect, already wiped by
        // Resolve's own EndConflict() cleanup by the time this test can observe it - bowing
        // the stronghold is the one effect that persists past the conflict's own end.
        Assert.That(p1.Stronghold!.Bowed, Is.True);
    }

    [Test]
    public void WithAnAttackerPolicySupplied_RunsAPostResolutionActionWindow_LettingBlackmailArtistTakeHonor()
    {
        // Proves the post-resolution window itself (not just BlackmailArtistBotAction in
        // isolation): Conflict.Winner/ConflictType are only settled *after* the mid-conflict
        // window runs, so this can only fire in the second, later window.
        var p1 = new Player { Name = "Player1", Honor = 5 };
        var p2 = new Player { Name = "Player2", Honor = 5 };
        p1.Stronghold = new Card { Id = "sh1", Type = CardType.Stronghold, Controller = p1 };
        p2.Stronghold = new Card { Id = "sh2", Type = CardType.Stronghold, Controller = p2 };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };

        var blackmailArtist = new Card { Id = "blackmail-artist", Type = CardType.Character, Controller = p1, PrintedPoliticalSkill = 3 };
        p1.PlayArea.Add(blackmailArtist);

        var province = new Card { Id = "province", Type = CardType.Province, Controller = p2, PrintedProvinceStrength = 10 };
        p2.Provinces.Add(province);

        var registry = new ScriptedActionRegistry();
        var attackerPolicy = new FirstLegalActionBotPolicy(registry);
        var defenderPolicy = new FirstLegalActionBotPolicy(registry);

        Resolve(game, p1, new ConflictDeclaration("earth", province, new[] { blackmailArtist }), defenderPolicy, attackerPolicy);

        var conflict = game.ConflictRecord.Single();
        Assert.That(conflict.Winner, Is.EqualTo(p1));
        Assert.That(p1.Honor, Is.EqualTo(6));
        Assert.That(p2.Honor, Is.EqualTo(3), "unopposed loses 1 honor, then blackmail-artist takes 1 more");
    }
}
