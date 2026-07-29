using L5R.Engine.Abilities;

namespace L5R.Engine.Cards;

/// <summary>
/// Escape valve for cards whose logic doesn't fit card-schema.json's generic vocabulary.
/// A scriptOverride's `handler` names a type implementing this, resolved by reflection
/// at card-load time.
///
/// Execute is a default interface member (throwing NotImplementedException) rather than a
/// plain abstract method so the ~90 still-empty Scripts stubs keep compiling untouched as
/// each one is implemented in turn - only the card actually being ported overrides it. No
/// JsonElement parameters (unlike IGameActionHandler) - the entire point of a script is that
/// there's no JSON driving it; a script reads whatever caller-supplied choice it needs
/// directly off the existing AbilityContext (Target/CostTarget/ChosenX fields), the same
/// trust-the-caller convention as every other target/choice in this engine. No separate
/// condition-check method either - like every existing gameAction handler, a script throws
/// InvalidOperationException when its own condition/cost/target isn't satisfiable.
/// </summary>
public interface ICardScript
{
    void Execute(AbilityContext context) => throw new NotImplementedException();

    /// <summary>
    /// ringteki DrawCard.canPlay override - an extra legality gate on top of the normal
    /// cost/location checks, controlling whether the card can be played from hand at all
    /// (distinct from Execute, which resolves an in-play ability). Defaults to true ("no
    /// additional restriction") so only the cards that actually need it override it.
    /// Consulted by PlayCardGameActionHandler via Card.PlayScript - the one narrow, optional
    /// wiring point between ICardScript and the generic pipeline (Execute has none; every
    /// Execute-based script is still only ever invoked by direct test instantiation).
    /// </summary>
    bool CanPlay(AbilityContext context) => true;
}
