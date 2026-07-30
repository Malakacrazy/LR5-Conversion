using System.Linq;
using L5R.Engine.Abilities;
using L5R.Engine.Cards.Scripts;
using L5R.Engine.State;

namespace L5R.Engine.GameSteps.BotActions;

/// <summary>
/// niten-master's own precondition (a weapon controlled by its controller is attached to it)
/// is a stable state fact, but unlike the other adopted reactions it doesn't reset on its
/// own - the weapon stays attached across windows. The adapter adds a !source.Bowed gate
/// (the script itself never checks this) so the action only fires once per bow, matching
/// the real card's "ready on attach" trigger instead of readying an already-ready character
/// every window.
/// </summary>
public sealed class NitenMasterBotAction : IBotScriptAction
{
    public bool IsLegal(GameState game, Card source, Player actingPlayer) =>
        source.Bowed && FindWeapon(game, source, actingPlayer) is not null;

    public void Invoke(GameState game, Card source, Player actingPlayer)
    {
        var weapon = FindWeapon(game, source, actingPlayer)
            ?? throw new InvalidOperationException($"'{source.Id}' has no legal bot weapon.");

        var context = new AbilityContext { Game = game, Player = actingPlayer, Source = source, Target = weapon };
        new NitenMasterReadyOnWeaponAttached().Execute(context);
    }

    private static Card? FindWeapon(GameState game, Card source, Player actingPlayer) =>
        game.AllCards().FirstOrDefault(c => c.AttachedTo == source && c.Traits.Contains("weapon") && c.Controller == actingPlayer);
}
