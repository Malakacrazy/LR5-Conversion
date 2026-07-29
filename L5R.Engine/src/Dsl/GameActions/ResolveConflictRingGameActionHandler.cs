using System.Text.Json;
using L5R.Engine.Abilities;

namespace L5R.Engine.Dsl.GameActions;

/// <summary>
/// ringteki resolveConflictRing (akodo-toturi/doji-hotaru/defend-the-wall's shared "resolve
/// ring effect" reaction): re-runs the current conflict's ring element's own effect, same
/// as ringteki's Air/Earth/Fire/Water/VoidRingEffect classes. Each element's "Don't
/// resolve" option is a legal choice, not a missing input - modeled the same way
/// DeckSearchGameActionHandler treats "take nothing": a null ChosenChoice/Target means
/// don't resolve, not an error. Reuses existing handlers for every element's own effect
/// rather than re-implementing honor/fate/bow/draw/discard mutation.
/// </summary>
public sealed class ResolveConflictRingGameActionHandler : IGameActionHandler
{
    private static readonly JsonElement AmountTwo = JsonDocument.Parse("{\"amount\":2}").RootElement;
    private static readonly JsonElement AmountOne = JsonDocument.Parse("{\"amount\":1}").RootElement;

    public void Execute(AbilityContext context, JsonElement? parameters)
    {
        var conflict = context.Game.CurrentConflict
            ?? throw new InvalidOperationException("resolveConflictRing requires an active conflict.");

        var element = conflict.Elements.FirstOrDefault()
            ?? throw new InvalidOperationException("resolveConflictRing requires the conflict to have a declared ring element.");

        switch (element)
        {
            case "air":
                ResolveAir(context);
                break;
            case "earth":
                ResolveEarth(context);
                break;
            case "fire":
                ResolveFire(context);
                break;
            case "water":
                ResolveWater(context);
                break;
            case "void":
                ResolveVoid(context);
                break;
            default:
                throw new NotSupportedException($"resolveConflictRing does not support ring element '{element}'.");
        }
    }

    private static void ResolveAir(AbilityContext context)
    {
        switch (context.ChosenChoice)
        {
            case null:
                return;
            case "Gain 2 Honor":
                new GainHonorGameActionHandler().Execute(context, AmountTwo);
                break;
            case "Take 1 Honor from opponent":
                new TakeHonorGameActionHandler().Execute(context, null);
                break;
            default:
                throw new InvalidOperationException("resolveConflictRing (air) requires context.ChosenChoice to be 'Gain 2 Honor', 'Take 1 Honor from opponent', or null.");
        }
    }

    private static void ResolveEarth(AbilityContext context)
    {
        if (context.ChosenChoice is null)
            return;

        if (context.ChosenChoice != "Draw a card and opponent discards")
            throw new InvalidOperationException("resolveConflictRing (earth) requires context.ChosenChoice to be 'Draw a card and opponent discards' or null.");

        new DrawGameActionHandler().Execute(context, null);
        new ChosenDiscardGameActionHandler().Execute(context, AmountOne);
    }

    private static void ResolveFire(AbilityContext context)
    {
        if (context.Target is null)
            return;

        switch (context.ChosenChoice)
        {
            case "Honor":
                new HonorGameActionHandler().Execute(context, null);
                break;
            case "Dishonor":
                new DishonorGameActionHandler().Execute(context, null);
                break;
            default:
                throw new InvalidOperationException("resolveConflictRing (fire) requires context.ChosenChoice to be 'Honor' or 'Dishonor' when context.Target is set.");
        }
    }

    private static void ResolveWater(AbilityContext context)
    {
        if (context.Target is null)
            return;

        if (context.Target.Bowed)
        {
            new ReadyGameActionHandler().Execute(context, null);
        }
        else
        {
            if (context.Target.Fate != 0)
                throw new InvalidOperationException($"'{context.Target.Id}' must have no fate to be bowed by the water ring.");

            new BowGameActionHandler().Execute(context, null);
        }
    }

    private static void ResolveVoid(AbilityContext context)
    {
        if (context.Target is null)
            return;

        new RemoveFateGameActionHandler().Execute(context, null);
    }
}
