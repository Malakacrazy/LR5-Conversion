using System.Text.Json;
using L5R.Engine.Abilities;
using L5R.Engine.Dsl;
using L5R.Engine.State;

namespace L5R.Engine.Tests.Cards.PerCard;

public class AsahinaArtisanTests
{
    private static ActionDefinition LoadFirstAction()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Cards", "01-Core", "asahina-artisan.json");
        using var document = JsonDocument.Parse(File.ReadAllText(path));
        return AbilityDefinitionParser.ParseActions(document.RootElement).Single();
    }

    [Test]
    public void DuringConflict_GivesAnotherCraneCharacterPlusThreePoliticalSkill()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var artisan = new Card { Id = "asahina-artisan", Type = CardType.Character, Controller = p1, Faction = "crane" };
        var craneAlly = new Card { Id = "crane-ally", Type = CardType.Character, Controller = p1, Faction = "crane", PrintedPoliticalSkill = 2 };
        p1.PlayArea.Add(artisan);
        p1.PlayArea.Add(craneAlly);

        var action = LoadFirstAction();
        var context = new AbilityContext { Game = game, Player = p1, Source = artisan };
        var executor = new AbilityExecutor(new CostRegistry(), new GameActionRegistry());

        executor.Execute(action, context, chosenTarget: craneAlly);

        Assert.That(artisan.Bowed, Is.True, "bowSelf cost was paid");
        Assert.That(game.EffectivePoliticalSkill(craneAlly), Is.EqualTo(5));
    }

    [Test]
    public void CannotTargetItself()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        var artisan = new Card { Id = "asahina-artisan", Type = CardType.Character, Controller = p1, Faction = "crane" };

        var action = LoadFirstAction();
        var legalTargets = TargetResolver.ResolveLegalTargets(
            action.Target!,
            new AbilityContext { Game = game, Player = p1, Source = artisan });

        Assert.That(legalTargets, Does.Not.Contain(artisan));
    }
}
