using L5R.Engine.Cards;
using L5R.Engine.Cards.Scripts;

namespace L5R.Engine.Tests.Cards;

/// <summary>
/// Loads every authored Core Set card (L5R.Engine/cards/01-Core/) through CardLoader,
/// proving the schema/DSL and the registries actually hold up against real cards, not
/// just hand-built fixtures. Each card here was ported from a real ringteki
/// server/game/cards/01-Core/*.js implementation.
/// </summary>
public class CoreSetCardsTests
{
    private static readonly string CardsDir = Path.Combine(AppContext.BaseDirectory, "Cards", "01-Core");

    [TestCase("hiruma-yojimbo")]
    [TestCase("border-rider")]
    [TestCase("cloud-the-mind")]
    [TestCase("assassination")]
    [TestCase("court-mask")]
    [TestCase("ancestral-lands")]
    [TestCase("city-of-the-open-hand")]
    [TestCase("artisan-academy")]
    [TestCase("keeper-of-air")]
    [TestCase("magnificent-kimono")]
    [TestCase("for-shame")]
    [TestCase("steadfast-samurai")]
    [TestCase("shiba-yojimbo")]
    [TestCase("favored-niece")]
    [TestCase("agasha-swordsmith")]
    [TestCase("shiba-tsukune")]
    [TestCase("niten-master")]
    [TestCase("guest-of-honor")]
    [TestCase("hida-guardian")]
    [TestCase("grasp-of-earth")]
    [TestCase("cautious-scout")]
    [TestCase("seppun-guardsman")]
    [TestCase("duelist-training")]
    [TestCase("tattooed-wanderer")]
    [TestCase("kaiu-shuichi")]
    [TestCase("favored-mount")]
    [TestCase("asahina-artisan")]
    [TestCase("otomo-courtier")]
    [TestCase("mirumoto-prodigy")]
    [TestCase("venerable-historian")]
    [TestCase("guidance-of-the-ancestors")]
    [TestCase("togashi-initiate")]
    [TestCase("moto-youth")]
    [TestCase("seeker-of-knowledge")]
    [TestCase("fertile-fields")]
    [TestCase("shinjo-outrider")]
    [TestCase("height-of-fashion")]
    [TestCase("above-question")]
    [TestCase("adept-of-shadows")]
    [TestCase("admit-defeat")]
    [TestCase("against-the-waves")]
    [TestCase("bayushi-yunako")]
    [TestCase("blackmail")]
    [TestCase("borderlands-defender")]
    [TestCase("born-in-war")]
    [TestCase("brash-samurai")]
    [TestCase("breakthrough")]
    [TestCase("calling-in-favors")]
    [TestCase("captive-audience")]
    [TestCase("ambush")]
    [TestCase("cavalry-reserves")]
    [TestCase("charge")]
    [TestCase("city-of-lies")]
    [TestCase("contingency-plan")]
    [TestCase("court-games")]
    [TestCase("daidoji-nerishma")]
    [TestCase("daimyo-s-favor")]
    [TestCase("deathseeker")]
    [TestCase("defend-the-wall")]
    [TestCase("display-of-power")]
    [TestCase("doji-challenger")]
    [TestCase("doji-gift-giver")]
    [TestCase("doomed-shugenja")]
    [TestCase("elemental-fury")]
    [TestCase("endless-plains")]
    [TestCase("enlightened-warrior")]
    public void RealCoreSetCard_LoadsWithoutError(string cardId)
    {
        var loader = new CardLoader(RingtekiCatalog.Effects, RingtekiCatalog.GameActions, RingtekiCatalog.Costs);
        var path = Path.Combine(CardsDir, $"{cardId}.json");

        Assert.That(File.Exists(path), Is.True, $"expected a card file at {path}");

        var card = loader.Load(File.ReadAllText(path));

        Assert.That(card.Id, Is.EqualTo(cardId));
    }

    [Test]
    public void CloudTheMind_ResolvesItsScriptOverrideHandler()
    {
        var loader = new CardLoader(RingtekiCatalog.Effects, RingtekiCatalog.GameActions, RingtekiCatalog.Costs);
        var path = Path.Combine(CardsDir, "cloud-the-mind.json");

        var card = loader.Load(File.ReadAllText(path));

        Assert.That(card.ScriptOverride, Is.Not.Null);
        Assert.That(card.ScriptOverride!.HandlerType, Is.EqualTo(typeof(CloudTheMindPlayRestriction)));
        Assert.That(card.ScriptOverride!.Reason, Is.Not.Empty, "scriptOverride must document why it was needed");
    }

    [TestCase("artisan-academy", typeof(ArtisanAcademyRevealTopCard))]
    [TestCase("keeper-of-air", typeof(KeeperOfAirGainFateOnDefendedWin))]
    [TestCase("steadfast-samurai", typeof(SteadfastSamuraiHonorThresholdProtection))]
    [TestCase("shiba-yojimbo", typeof(ShibaYojimboCancelShugenjaTargetedAbility))]
    [TestCase("shiba-tsukune", typeof(ShibaTsukuneResolveUpToTwoRings))]
    [TestCase("niten-master", typeof(NitenMasterReadyOnWeaponAttached))]
    [TestCase("cautious-scout", typeof(CautiousScoutBlankLoneAttackersProvince))]
    [TestCase("duelist-training", typeof(DuelistTrainingGrantMilitaryDuelAction))]
    [TestCase("tattooed-wanderer", typeof(TattooedWandererPlayAsAttachment))]
    [TestCase("blackmail", typeof(BlackmailCannotPlayUnlessLessHonorable))]
    [TestCase("breakthrough", typeof(BreakthroughDeclareSecondConflict))]
    [TestCase("calling-in-favors", typeof(CallingInFavorsAttachOrDiscard))]
    [TestCase("court-games", typeof(CourtGamesHonorOrDishonorParticipant))]
    [TestCase("deathseeker", typeof(DeathseekerRemoveFateOrDiscardOnLoss))]
    [TestCase("defend-the-wall", typeof(DefendTheWallResolveRingAsAttacker))]
    [TestCase("display-of-power", typeof(DisplayOfPowerCancelAndClaimRing))]
    [TestCase("endless-plains", typeof(EndlessPlainsBreakAndDiscardAttacker))]
    [TestCase("enlightened-warrior", typeof(EnlightenedWarriorGainFateOnOpponentRingSelect))]
    public void Card_ResolvesItsScriptOverrideHandler(string cardId, Type expectedHandlerType)
    {
        var loader = new CardLoader(RingtekiCatalog.Effects, RingtekiCatalog.GameActions, RingtekiCatalog.Costs);
        var path = Path.Combine(CardsDir, $"{cardId}.json");

        var card = loader.Load(File.ReadAllText(path));

        Assert.That(card.ScriptOverride, Is.Not.Null);
        Assert.That(card.ScriptOverride!.HandlerType, Is.EqualTo(expectedHandlerType));
        Assert.That(card.ScriptOverride!.Reason, Is.Not.Empty, "scriptOverride must document why it was needed");
    }

    [TestCase("kaiu-shuichi")]
    [TestCase("grasp-of-earth")]
    [TestCase("mirumoto-prodigy")]
    [TestCase("admit-defeat")]
    public void Card_NoLongerNeedsScriptOverride_AfterTheVocabularyGrewToCoverIt(string cardId)
    {
        // kaiu-shuichi needed a per-player dynamic count (valueRef "for"); grasp-of-earth
        // needed a bulk-collection gameAction target ("allCardsMatching"); mirumoto-prodigy
        // and admit-defeat needed a per-role conflict participant count (valueRef "role").
        // All were originally scriptOverride; this pins down that they now load generically.
        var loader = new CardLoader(RingtekiCatalog.Effects, RingtekiCatalog.GameActions, RingtekiCatalog.Costs);
        var path = Path.Combine(CardsDir, $"{cardId}.json");

        var card = loader.Load(File.ReadAllText(path));

        Assert.That(card.ScriptOverride, Is.Null);
    }
}
