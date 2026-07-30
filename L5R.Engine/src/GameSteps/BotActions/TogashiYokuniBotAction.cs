using System.Linq;
using L5R.Engine.Abilities;
using L5R.Engine.Cards.Scripts;
using L5R.Engine.Dsl;
using L5R.Engine.State;

namespace L5R.Engine.GameSteps.BotActions;

/// <summary>
/// togashi-yokuni: choose a printed ability on another character and gain a copy of it until
/// the end of the phase. Reachable through the normal ChooseScriptedAction scan, same as any
/// other plain character's own printed action - unlike the "genuine transient event" or
/// zone-invisible cards this session adopted via dedicated firers, nothing about this card's
/// own trigger condition is time-sensitive. Picks the first other character in play (either
/// player's) that has at least one bridged Card.Actions entry with a Definition (only cards
/// whose JSON actions[] went through CardFactory's ActionDefinition bridge have one - plain
/// stat-only characters don't), and copies its first such ability. "max: perRound(1)" is a
/// documented no-op, matching every other "max"/"limit" field's established precedent (see
/// the script's own doc comment) - this action is legal every time a valid target exists.
/// </summary>
public sealed class TogashiYokuniBotAction : IBotScriptAction
{
    public bool IsLegal(GameState game, Card source, Player actingPlayer) =>
        FindTargetAndAbility(game, source) is not null;

    public void Invoke(GameState game, Card source, Player actingPlayer)
    {
        var (target, ability) = FindTargetAndAbility(game, source)
            ?? throw new InvalidOperationException($"'{source.Id}' has no legal bot target.");

        var context = new AbilityContext { Game = game, Player = actingPlayer, Source = source, Target = target, ChosenAbility = ability };
        new TogashiYokuniCopyAnotherCharactersAbility().Execute(context);
    }

    private static (Card Target, ActionDefinition Ability)? FindTargetAndAbility(GameState game, Card source)
    {
        foreach (var character in game.AllCards().Where(c => c != source && c.Type == CardType.Character))
        {
            var ability = character.Actions.Select(a => a.Definition).FirstOrDefault(d => d is not null);
            if (ability is not null)
                return (character, ability);
        }

        return null;
    }
}
