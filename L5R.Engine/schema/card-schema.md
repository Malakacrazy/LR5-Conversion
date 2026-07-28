# card-schema.json — field reference

Maps every field in `card-schema.json` back to the equivalent concept in
`reference/ringteki/docs/implementing-cards.md` (referred to below as "the doc") and the
ringteki source files it documents. Read the doc first if a field's *purpose* isn't clear
from this file — this doc explains what each field maps to, not why the underlying
mechanic exists.

## Design decisions worth knowing before authoring a card

- **Effect/game action/cost names are plain strings, not JSON Schema enums.** ringteki's
  `effects.js` (~100 entries), `GameActions/GameActions.ts` (~80 entries) and `costs.js`
  (~25 entries) are the source catalogs, ported to C# registries (`EffectRegistry`,
  `GameActionRegistry`, `CostRegistry`). Baking those names into the JSON Schema as enums
  would duplicate the registries and the two would drift apart silently as the engine
  grows. Instead, an unknown name is a **card-load-time error** in the engine — loud and
  immediate, not a schema validation nuance.
- **`params` on `gameActionEntry` and `costEntry` is free-form.** Typing all ~80 game
  actions' individual parameter shapes into this schema would be a second, competing
  definition of what each action accepts. The C# implementation of each action validates
  its own `params` when the card loads.
- **`predicate` and `valueRef` are closed vocabularies, not embedded scripting.** ringteki
  predicates are arbitrary JS lambdas (`card => card.isFaction('crane') && ...`); a JSON
  card can't embed code. The schema instead defines a fixed instruction set (`isType`,
  `hasTrait`, `compareStat`, boolean combinators, etc.) that covers the common cases shown
  in the doc. A card whose condition needs something outside this vocabulary — or a
  dynamic value beyond a simple named counter — uses `scriptOverride`. Don't extend the
  vocabulary speculatively for hypothetical future cards; extend it when a real Core Set
  card in task 9 needs an op that doesn't exist yet. (`isDuringConflict` was added this
  way — porting the "discard a cheap character during a conflict" pattern needed a
  game-state-level check, not just card-level ones.)
- **This schema validates shape, not every ringteki invariant.** For example, the doc says
  "player effects should not have a match property" — that's a runtime invariant the C#
  loader checks (since whether an effect name is card/ring/player-scoped lives in the
  registry, not in this schema), not something expressible as a JSON Schema conditional
  here.

## Top-level fields

| Field | Doc / source equivalent |
|---|---|
| `id` | ringteki card `.id` static property (doc step 2). Matches the emeralddb slug, which is also where `printedCost`/stats/`text`/`traits` are sourced from at card-authoring time — this schema doesn't duplicate emeralddb, it's the layer on top that adds engine behavior. |
| `type` | `CardTypes` enum, `Constants.ts`. |
| `faction`, `traits`, `unique`, `elements`, `printedCost`, `military`, `political`, `glory`, `strength`, `strengthBonus`, `fate`, `honor`, `influenceCost`, `influencePool` | Printed card stats, sourced from emeralddb's `CardData` shape (see the project's `reference/emeralddb-data/core-set.json`). |
| `text` | Printed rules text. Reference/UI only, never interpreted by the engine — `abilities` below is what the engine actually runs. |
| `abilities` | Container for everything doc sections "Persistent effects" / "Actions" / "Triggered abilities" cover. |
| `scriptOverride` | Escape valve. See below. |

## `abilities.persistentEffects[]` → doc "Persistent effects"

| Field | Doc section |
|---|---|
| `match` | "Matching conditions vs matching specific cards". `"self"` = passing `this` in ringteki. |
| `condition` | "Conditional effects". |
| `targetController` | "Targeting opponent or all matching cards" / "Player modifying effects". |
| `targetLocation` | "Applying effects to cards which aren't in play". |
| `effect` | An `effectEntry` or array of them ("Applying multiple effects at once"). |

## `abilities.whileAttached[]` → doc "Attachment-based effects"

Same shape as a persistent effect minus `match`/`targetController` (implicitly "the card
this is attached to").

## `effectEntry` → `effects.js` catalog entry

`name` is the effect key (e.g. `modifyMilitarySkill`, `addKeyword`). `value` corresponds
to what the doc's "Dynamic skill" section calls a static/dynamic/flexible value: a literal
for static effects, or a `valueRef` (see below) for dynamic ones. Effects that take no
value (`doesNotBow`, `switchBaseSkills`, ...) omit it.

## `action[]` → doc "Actions", `cardaction.js`

Direct field-for-field match: `title`, `condition`, `cost`, `target`/`targets`,
`gameAction`, `effect`/`effectArgs`, `phase`, `location`, `limit`, `max`, `anyPlayer`,
`doesNotTarget` all name-match their doc/source counterparts.

## `triggeredAbility[]` → doc "Triggered abilities", `triggeredability.js`

`trigger` replaces ringteki's five separate declaration methods
(`this.reaction`/`forcedReaction`/`interrupt`/`forcedInterrupt`/`wouldInterrupt`) with one
enum field, since they're all the same shape and differ only in trigger semantics
(`AbilityTypes` in `Constants.ts`). `when` and `aggregateWhen` match the doc's "Defining
the triggering condition" section — keys are `EventNames.*` values. `handler: "cancel"` is
the one non-`gameAction` handler the doc shows (`'Would' interrupts`, `context.cancel()`);
anything else needing a bespoke handler function is a `scriptOverride` candidate.

## `target` → doc "Choosing / targeting cards", "Multiple targets", "Targeting rings", "Select options"

`mode` selects between plain card targeting (default), `ring` (`ringCondition` applies,
see `gamesteps/selectringprompt.js`), and `select` (`choices` applies). `gameAction`
restricting legality is the doc's "restrict the card chosen to those for which that game
action is legal" behavior.

## `limit` → doc "Ability limits", `abilitylimit.js`

`per: "fixed"` = `AbilityLimit.fixed(max)` (never resets). The other `per` values map to
`AbilityLimit.perConflict/perPhase/perRound/unlimitedPerConflict`.

## `costEntry` → `costs.js` catalog entry

`name` is the cost key (`bowSelf`, `sacrificeSelf`, `dishonor`, `payFate`, ...). Costs that
select a card to pay (`bow`, `sacrifice`, `dishonor`, `discardCard`, `returnToHand`, ...)
take target-shaped `params` (`cardCondition`, `location`, etc.) mirroring the
`properties` argument those functions take in `costs.js`.

## `scriptOverride`

Not in the doc directly — it's the project's own answer to "the few cards whose logic
doesn't fit generic parameters" (see the project roadmap). `reason` must say *why* the
card needed it, so a reviewer doesn't have to reverse-engineer the decision later. Task 9
gates on this: no card gets ported by silently reaching for `scriptOverride` without
writing down why the generic vocabulary above didn't cover it.

## Worked example

`schema/examples/lantern-keeper.example.json` — an original example (not a real printed
card) with a conditional persistent effect, a costed action, and a forced interrupt.
Validated against this schema by `tests/Schema/CardSchemaTests.cs`, so it can't drift out
of sync with the schema above. One snippet for illustration, the persistent effect that
implements "each other participating character you control gets +1 political skill":

```json
{
  "condition": { "op": "hasStatus", "status": "isParticipating" },
  "match": {
    "op": "and",
    "of": [
      { "op": "isType", "type": "character" },
      { "op": "hasStatus", "status": "isParticipating" },
      { "op": "not", "of": { "op": "isSelf" } }
    ]
  },
  "effect": { "name": "modifyPoliticalSkill", "value": 1 }
}
```
