using L5R.Engine.Abilities;
using L5R.Engine.Dsl.GameActions;

namespace L5R.Engine.Cards.Scripts;

/// <summary>
/// kakita-kaezin: while participating, duel a participating opponent character (military
/// type); if this character wins, every uninvolved participant is sent home, otherwise this
/// character is sent home instead. context.DuelWinner is the caller-set fact (see its own
/// doc comment) standing in for a real skill+honor-bid duel resolution this engine doesn't
/// model.
/// </summary>
public sealed class KakitaKaezinDuelAndSendHomeByOutcome : ICardScript
{
    public void Execute(AbilityContext context)
    {
        var kaezin = context.Source;
        var conflict = context.Game.CurrentConflict
            ?? throw new InvalidOperationException($"'{kaezin.Id}' requires an active conflict.");

        if (!conflict.Attackers.Contains(kaezin) && !conflict.Defenders.Contains(kaezin))
            throw new InvalidOperationException($"'{kaezin.Id}' can only be used while participating.");

        var target = context.Target
            ?? throw new InvalidOperationException($"'{kaezin.Id}' requires context.Target to be set.");

        if (target.Controller != context.Game.Opponent(context.Player))
            throw new InvalidOperationException($"'{target.Id}' must be controlled by the opponent.");

        if (!conflict.Attackers.Contains(target) && !conflict.Defenders.Contains(target))
            throw new InvalidOperationException($"'{target.Id}' is not participating.");

        var winner = context.DuelWinner
            ?? throw new InvalidOperationException($"'{kaezin.Id}' requires context.DuelWinner to be set.");

        if (winner == kaezin)
        {
            var uninvolved = conflict.Attackers.Concat(conflict.Defenders).Where(c => c != kaezin && c != target).ToList();
            foreach (var card in uninvolved)
            {
                context.Target = card;
                new SendHomeGameActionHandler().Execute(context, null);
            }
        }
        else
        {
            context.Target = kaezin;
            new SendHomeGameActionHandler().Execute(context, null);
        }
    }
}
