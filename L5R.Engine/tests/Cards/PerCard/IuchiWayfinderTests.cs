using System.Text.Json;
using L5R.Engine.Abilities;
using L5R.Engine.Dsl;
using L5R.Engine.State;

namespace L5R.Engine.Tests.Cards.PerCard;

public class IuchiWayfinderTests
{
    private static TriggeredAbilityDefinition LoadFirstTriggeredAbility()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Cards", "01-Core", "iuchi-wayfinder.json");
        using var document = JsonDocument.Parse(File.ReadAllText(path));
        return AbilityDefinitionParser.ParseTriggeredAbilities(document.RootElement).Single();
    }

    [Test]
    public void WhenItEntersPlay_CanLookAtAnOpponentsFacedownProvinceWithoutFlippingIt()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var wayfinder = new Card { Id = "iuchi-wayfinder", Type = CardType.Character, Controller = p1 };
        var opponentProvince = new Card { Id = "opponent-province", Type = CardType.Province, Controller = p2, Location = "province", Facedown = true };
        p1.PlayArea.Add(wayfinder);
        p2.Provinces.Add(opponentProvince);

        var ability = LoadFirstTriggeredAbility();
        var context = new AbilityContext { Game = game, Player = p1, Source = wayfinder };
        var executor = new AbilityExecutor(new CostRegistry(), new GameActionRegistry());

        executor.ExecuteTriggered(ability, context, eventCard: wayfinder, chosenTarget: opponentProvince);

        Assert.That(opponentProvince.Facedown, Is.True, "lookAt never flips the card");
    }

    [Test]
    public void WhenAnotherCharacterEntersPlay_ConditionIsNotMet()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var wayfinder = new Card { Id = "iuchi-wayfinder", Type = CardType.Character, Controller = p1 };
        var someoneElse = new Card { Id = "someone-else", Type = CardType.Character, Controller = p1 };
        p1.PlayArea.Add(wayfinder);
        p1.PlayArea.Add(someoneElse);

        var ability = LoadFirstTriggeredAbility();
        var context = new AbilityContext { Game = game, Player = p1, Source = wayfinder };
        var executor = new AbilityExecutor(new CostRegistry(), new GameActionRegistry());

        Assert.Throws<InvalidOperationException>(
            () => executor.ExecuteTriggered(ability, context, eventCard: someoneElse));
    }

    [Test]
    public void AFaceupOpponentsProvince_IsNotALegalTarget()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var wayfinder = new Card { Id = "iuchi-wayfinder", Type = CardType.Character, Controller = p1 };
        var faceup = new Card { Id = "faceup-province", Type = CardType.Province, Controller = p2, Location = "province", Facedown = false };
        p1.PlayArea.Add(wayfinder);
        p2.Provinces.Add(faceup);

        var ability = LoadFirstTriggeredAbility();
        var context = new AbilityContext { Game = game, Player = p1, Source = wayfinder };
        var legalTargets = TargetResolver.ResolveLegalTargets(ability.Target!, context);

        Assert.That(legalTargets, Does.Not.Contain(faceup));
    }

    [Test]
    public void ItsOwnController_ProvinceIsNotALegalTarget()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var wayfinder = new Card { Id = "iuchi-wayfinder", Type = CardType.Character, Controller = p1 };
        var ownProvince = new Card { Id = "own-province", Type = CardType.Province, Controller = p1, Location = "province", Facedown = true };
        p1.PlayArea.Add(wayfinder);
        p1.Provinces.Add(ownProvince);

        var ability = LoadFirstTriggeredAbility();
        var context = new AbilityContext { Game = game, Player = p1, Source = wayfinder };
        var legalTargets = TargetResolver.ResolveLegalTargets(ability.Target!, context);

        Assert.That(legalTargets, Does.Not.Contain(ownProvince), "controller: opponent");
    }
}
