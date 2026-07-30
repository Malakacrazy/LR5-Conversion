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
        ["rout"] = new RoutBotAction()
    };

    public IBotScriptAction? Resolve(string cardId) =>
        _actions.TryGetValue(cardId, out var action) ? action : null;
}
