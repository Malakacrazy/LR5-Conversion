namespace L5R.Engine.Cards;

/// <summary>
/// The three name catalogs ported from ringteki's own source (MIT licensed):
/// server/game/effects.js, server/game/GameActions/GameActions.ts, server/game/costs.js.
/// Extend these lists only when a real Core Set card in task 9 needs a name that isn't
/// here yet and turns out to genuinely be missing from this transcription - don't add
/// speculative entries.
/// </summary>
public static class RingtekiCatalog
{
    public static readonly NameRegistry Effects = new(new[]
    {
        "addElementAsAttacker", "addFaction", "addKeyword", "addTrait",
        "attachmentFactionRestriction", "attachmentLimit", "attachmentMyControlOnly",
        "attachmentRestrictTraitAmount", "attachmentTraitRestriction", "attachmentUniqueRestriction",
        "blank", "calculatePrintedMilitarySkill", "canBeSeenWhenFacedown",
        "canOnlyBeDeclaredAsAttackerWithElement", "cannotApplyLastingEffects", "cannotBeAttacked",
        "cannotHaveConflictsDeclaredOfType", "cannotHaveOtherRestrictedAttachments",
        "cannotParticipateAsAttacker", "cannotParticipateAsDefender", "cardCannot",
        "changeContributionFunction", "changeType", "contributeToConflict", "copyCharacter",
        "customDetachedCard", "delayedEffect", "doesNotBow", "doesNotReady", "fateCostToAttack",
        "fateCostToTarget", "gainAbility", "gainExtraFateWhenPlayed", "gainPlayAction",
        "hideWhenFaceUp", "honorStatusDoesNotAffectLeavePlay", "honorStatusDoesNotModifySkill",
        "honorStatusReverseModifySkill", "immunity", "increaseLimitOnAbilities", "loseKeyword",
        "modifyBaseMilitarySkillMultiplier", "modifyBasePoliticalSkillMultiplier",
        "modifyBaseProvinceStrength", "modifyBothSkills", "modifyGlory", "modifyMilitarySkill",
        "attachmentMilitarySkillModifier", "modifyMilitarySkillMultiplier", "modifyPoliticalSkill",
        "attachmentPoliticalSkillModifier", "modifyPoliticalSkillMultiplier", "modifyProvinceStrength",
        "modifyProvinceStrengthMultiplier", "modifyProvinceStrengthBonus", "mustBeChosen",
        "mustBeDeclaredAsAttacker", "mustBeDeclaredAsDefender", "setBaseDash", "setBaseMilitarySkill",
        "setBasePoliticalSkill", "setBaseProvinceStrength", "setDash", "setGlory", "setBaseGlory",
        "setMilitarySkill", "setPoliticalSkill", "setProvinceStrength", "setProvinceStrengthBonus",
        "switchBaseSkills", "suppressEffects", "takeControl", "unlessActionCost", "addElement",
        "cannotBidInDuels", "cannotDeclareRing", "considerRingAsClaimed", "additionalAction",
        "additionalCardPlayed", "additionalCharactersInConflict", "additionalConflict",
        "additionalTriggerCost", "additionalPlayCost", "alternateFatePool",
        "cannotDeclareConflictsOfType", "canPlayFromOwn", "canPlayFromOpponents",
        "changePlayerGloryModifier", "changePlayerSkillModifier", "customDetachedPlayer",
        "gainActionPhasePriority", "increaseCost", "modifyCardsDrawnInDrawPhase", "playerCannot",
        "playerDelayedEffect", "reduceCost", "reduceNextPlayedCardCost", "setConflictDeclarationType",
        "setMaxConflicts", "setConflictTotalSkill", "showTopConflictCard", "showTopDynastyCard",
        "eventsCannotBeCancelled", "cannotContribute", "changeConflictSkillFunction",
        "modifyConflictElementsToResolve", "restrictNumberOfDefenders", "resolveConflictEarly",
        "forceConflictUnopposed"
    });

    public static readonly NameRegistry GameActions = new(new[]
    {
        "addToken", "attach", "attachToRing", "bow", "break", "cardLastingEffect",
        "claimImperialFavor", "createToken", "detach", "discardCard", "discardFromPlay",
        "dishonor", "duel", "flipDynasty", "honor", "lookAt", "moveCard", "moveToConflict",
        "placeFate", "playCard", "performGloryCount", "putIntoConflict", "putIntoPlay", "ready",
        "removeFate", "removeFromGame", "resolveAbility", "returnToDeck", "returnToHand",
        "reveal", "sendHome", "sacrifice", "takeControl", "turnFacedown", "chosenDiscard",
        "deckSearch", "discardAtRandom", "draw", "gainFate", "gainHonor", "initiateConflict",
        "loseFate", "loseHonor", "loseImperialFavor", "modifyBid", "playerLastingEffect",
        "refillFaceup", "setHonorDial", "shuffleDeck", "takeFate", "takeHonor", "placeFateOnRing",
        "resolveConflictRing", "resolveRingEffect", "returnRing", "ringLastingEffect",
        "selectRing", "switchConflictElement", "switchConflictType", "takeFateFromRing",
        "takeRing", "claimRing", "discardStatusToken", "moveStatusToken", "cancel", "handler",
        "cardMenu", "chooseAction", "conditional", "ifAble", "joint", "multiple", "menuPrompt",
        "selectCard", "sequential"
    });

    public static readonly NameRegistry Costs = new(new[]
    {
        "bowSelf", "bowParent", "bow", "sacrificeSelf", "sacrificeSpecific", "sacrifice",
        "returnToHand", "returnToDeck", "returnSelfToHand", "shuffleIntoDeck",
        "discardCardSpecific", "discardCard", "removeFateFromSelf", "removeFate",
        "removeFateFromParent", "removeFromGame", "dishonorSelf", "dishonor",
        "discardStatusToken", "breakSelf", "putSelfIntoPlay", "reveal", "discardImperialFavor",
        "payPrintedFateCost", "payReduceableFateCost", "payTargetDependentFateCost", "payFate",
        "payHonor", "giveHonorToOpponent", "payFateToRing", "giveFateToOpponent",
        "variableHonorCost", "returnRings", "chooseFate", "discardCardsUpToVariableX",
        "discardCardsExactlyVariableX"
    });
}
