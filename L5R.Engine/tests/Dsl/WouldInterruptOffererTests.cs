using System.Text.Json;
using L5R.Engine.Cards;
using L5R.Engine.GameSteps;
using L5R.Engine.State;

namespace L5R.Engine.Tests.Dsl;

public class WouldInterruptOffererTests
{
    private static Card LoadCard(string cardId, Player controller)
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Cards", "01-Core", $"{cardId}.json");
        using var document = JsonDocument.Parse(File.ReadAllText(path));
        return CardFactory.BuildCard(document.RootElement, controller);
    }

    private static (Player p1, Player p2, GameState game) NewGameDuringAConflict()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2", Honor = 5 };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p2, CurrentPhase = Phase.Conflict };
        game.CurrentConflict = new Conflict { AttackingPlayer = p2, DefendingPlayer = p1 };
        return (p1, p2, game);
    }

    [Test]
    public void ForgedEdict_DishonoringACourtier_CancelsTheOpponentsEvent()
    {
        var (p1, p2, game) = NewGameDuringAConflict();
        var target = new Card { Id = "cheap-character", Type = CardType.Character, Controller = p1, PrintedCost = 2 };
        var courtier = new Card { Id = "courtier-ally", Type = CardType.Character, Controller = p1, Traits = new[] { "courtier" }, PrintedCost = 3 };
        p1.PlayArea.Add(target);
        p1.PlayArea.Add(courtier);

        var assassination = LoadCard("assassination", p2);
        var forgedEdict = LoadCard("forged-edict", p1);
        p1.Hand.Add(forgedEdict);

        EventResolver.ResolveAndDiscard(game, assassination, p2);

        Assert.That(p1.Discard, Does.Not.Contain(target), "assassination's effect never ran - it was cancelled before Resolve");
        Assert.That(courtier.IsDishonored, Is.True, "forged-edict's own dishonor cost was paid");
        Assert.That(p1.Hand, Does.Not.Contain(forgedEdict));
        Assert.That(p1.Discard, Contains.Item(forgedEdict), "forged-edict itself is discarded after being played as an interrupt");
        Assert.That(p2.Discard, Contains.Item(assassination), "the original event is still discarded even though it was cancelled");
    }

    [Test]
    public void ForgedEdict_WithNoCourtierInPlay_TheEventResolvesNormally()
    {
        var (p1, p2, game) = NewGameDuringAConflict();
        var target = new Card { Id = "cheap-character", Type = CardType.Character, Controller = p1, PrintedCost = 2 };
        p1.PlayArea.Add(target);

        var assassination = LoadCard("assassination", p2);
        var forgedEdict = LoadCard("forged-edict", p1);
        p1.Hand.Add(forgedEdict);

        EventResolver.ResolveAndDiscard(game, assassination, p2);

        Assert.That(p1.Discard, Contains.Item(target), "no courtier to pay the cost with, so forged-edict couldn't be played");
        Assert.That(p1.Hand, Contains.Item(forgedEdict), "forged-edict was never played, so it stays in hand");
    }

    [Test]
    public void VoiceOfHonor_WhenAheadOnHonoredCharacters_CancelsTheOpponentsEvent()
    {
        var (p1, p2, game) = NewGameDuringAConflict();
        var target = new Card { Id = "cheap-character", Type = CardType.Character, Controller = p1, PrintedCost = 2 };
        var honoredAlly = new Card { Id = "honored-ally", Type = CardType.Character, Controller = p1, IsHonored = true, PrintedCost = 3 };
        p1.PlayArea.Add(target);
        p1.PlayArea.Add(honoredAlly);

        var assassination = LoadCard("assassination", p2);
        var voiceOfHonor = LoadCard("voice-of-honor", p1);
        p1.Hand.Add(voiceOfHonor);

        EventResolver.ResolveAndDiscard(game, assassination, p2);

        Assert.That(p1.Discard, Does.Not.Contain(target), "assassination was cancelled");
        Assert.That(p1.Hand, Does.Not.Contain(voiceOfHonor));
        Assert.That(p1.Discard, Contains.Item(voiceOfHonor));
    }

    [Test]
    public void VoiceOfHonor_WhenNotAheadOnHonoredCharacters_TheEventResolvesNormally()
    {
        var (p1, p2, game) = NewGameDuringAConflict();
        var target = new Card { Id = "cheap-character", Type = CardType.Character, Controller = p1, PrintedCost = 2 };
        p1.PlayArea.Add(target);

        var assassination = LoadCard("assassination", p2);
        var voiceOfHonor = LoadCard("voice-of-honor", p1);
        p1.Hand.Add(voiceOfHonor);

        EventResolver.ResolveAndDiscard(game, assassination, p2);

        Assert.That(p1.Discard, Contains.Item(target), "0 honored characters on each side isn't 'ahead', so voice-of-honor couldn't be played");
        Assert.That(p1.Hand, Contains.Item(voiceOfHonor));
    }
}
