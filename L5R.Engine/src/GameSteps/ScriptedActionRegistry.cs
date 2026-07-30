using L5R.Engine.GameSteps.BotActions;

namespace L5R.Engine.GameSteps;

/// <summary>
/// Maps a card id to its Phase B bot adapter (IBotScriptAction). Deliberately small - grows
/// one entry at a time as each scriptOverride card is adopted into the bot's action space,
/// mirroring GameActionRegistry/CostRegistry's own "grows one entry at a time as a real
/// ported card needs it" convention. A card with no entry here simply isn't bot-drivable
/// yet - not an error, just not adopted.
/// </summary>
public sealed class ScriptedActionRegistry
{
    private readonly Dictionary<string, IBotScriptAction> _actions = new()
    {
        ["fearsome-mystic"] = new FearsomeMysticBotAction(),
        ["lion-s-pride-brawler"] = new LionsPrideBrawlerBotAction(),
        ["solemn-scholar"] = new SolemnScholarBotAction(),
        ["borderlands-fortifications"] = new BorderlandsFortificationsBotAction(),
        ["bayushi-shoju"] = new BayushiShojuBotAction(),
        ["outwit"] = new OutwitBotAction(),
        ["rout"] = new RoutBotAction(),
        ["strength-in-numbers"] = new StrengthInNumbersBotAction(),
        ["jade-tetsubo"] = new JadeTetsuboBotAction(),
        ["meddling-mediator"] = new MeddlingMediatorBotAction(),
        ["way-of-the-phoenix"] = new WayOfThePhoenixBotAction(),
        ["ascetic-visionary"] = new AsceticVisionaryBotAction(),
        ["shinjo-tatsuo"] = new ShinjoTatsuoBotAction(),
        ["artisan-academy"] = new ArtisanAcademyBotAction(),
        ["i-am-ready"] = new IAmReadyBotAction(),
        ["giver-of-gifts"] = new GiverOfGiftsBotAction(),
        ["rebuild"] = new RebuildBotAction(),
        ["niten-adept"] = new NitenAdeptBotAction(),
        ["shrewd-yasuki"] = new ShrewdYasukiBotAction(),
        ["spyglass"] = new SpyglassBotAction(),
        ["obstinate-recruit"] = new ObstinateRecruitBotAction(),
        ["radiant-orator"] = new RadiantOratorBotAction(),
        ["enlightened-warrior"] = new EnlightenedWarriorBotAction(),
        ["ide-trader"] = new IdeTraderBotAction(),
        ["secluded-temple"] = new SecludedTempleBotAction(),
        ["kitsu-spiritcaller"] = new KitsuSpiritcallerBotAction(),
        ["togashi-kazue"] = new TogashiKazueStealFateBotAction(),
        ["niten-master"] = new NitenMasterBotAction(),
        ["hida-kisada"] = new HidaKisadaBotAction(),
        ["tattooed-wanderer"] = new TattooedWandererBotAction(),
        ["yogo-hiroue"] = new YogoHiroueBotAction(),
        ["akodo-toturi"] = new AkodoToturiBotAction(),
        ["doji-hotaru"] = new DojiHotaruBotAction(),
        ["asako-diplomat"] = new AsakoDiplomatBotAction(),
        ["blackmail-artist"] = new BlackmailArtistBotAction(),
        ["deathseeker"] = new DeathseekerBotAction(),
        ["hida-tomonatsu"] = new HidaTomonatsuBotAction(),
        ["honored-blade"] = new HonoredBladeBotAction(),
        ["ikoma-eiji"] = new IkomaEijiBotAction(),
        ["vengeful-oathkeeper"] = new VengefulOathkeeperBotAction(),
        ["kakita-asami"] = new KakitaAsamiBotAction(),
        ["kakita-kaezin"] = new KakitaKaezinBotAction(),
        ["mirumoto-raitsugu"] = new MirumotoRaitsuguBotAction(),
        ["duelist-training"] = new DuelistTrainingBotAction(),
        ["banzai"] = new BanzaiBotAction(),
        ["indomitable-will"] = new IndomitableWillBotAction(),
        ["for-greater-glory"] = new ForGreaterGloryBotAction(),
        ["fallen-in-battle"] = new FallenInBattleBotAction(),
        ["spies-at-court"] = new SpiesAtCourtBotAction(),
        ["mantra-of-fire"] = new MantraOfFireBotAction(),
        ["the-perfect-gift"] = new ThePerfectGiftBotAction(),
        ["calling-in-favors"] = new CallingInFavorsBotAction(),
        ["display-of-power"] = new DisplayOfPowerBotAction(),
        ["court-games"] = new CourtGamesBotAction(),
        ["mountain-s-anvil-castle"] = new MountainsAnvilCastleBotAction(),
        ["seeker-initiate"] = new SeekerInitiateBotAction(),
        ["togashi-yokuni"] = new TogashiYokuniBotAction()
    };

    public IBotScriptAction? Resolve(string cardId) =>
        _actions.TryGetValue(cardId, out var action) ? action : null;
}
