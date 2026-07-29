using L5R.Engine.Abilities;
using L5R.Engine.Cards.Scripts;
using L5R.Engine.State;

namespace L5R.Engine.Tests.Cards.PerCard;

public class CourtGamesTests
{
    private static (GameState Game, Card CourtGames, Card MyCharacter, Card OpponentCharacter) NewGamePoliticalConflict()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var courtGames = new Card { Id = "court-games", Type = CardType.Event, Controller = p1 };
        var myCharacter = new Card { Id = "my-character", Type = CardType.Character, Controller = p1 };
        var opponentCharacter = new Card { Id = "opponent-character", Type = CardType.Character, Controller = p2 };
        p1.PlayArea.Add(myCharacter);
        p2.PlayArea.Add(opponentCharacter);

        var conflict = new Conflict { AttackingPlayer = p1, DefendingPlayer = p2, ConflictType = "political" };
        conflict.Attackers.Add(myCharacter);
        conflict.Defenders.Add(opponentCharacter);
        game.CurrentConflict = conflict;

        return (game, courtGames, myCharacter, opponentCharacter);
    }

    [Test]
    public void HonoringAFriendlyParticipant_HonorsIt()
    {
        var (game, courtGames, myCharacter, _) = NewGamePoliticalConflict();
        var context = new AbilityContext { Game = game, Player = game.Player1, Source = courtGames, Target = myCharacter, ChosenChoice = "Honor a friendly character" };

        new CourtGamesHonorOrDishonorParticipant().Execute(context);

        Assert.That(myCharacter.IsHonored, Is.True);
    }

    [Test]
    public void DishonoringAnOpposingParticipant_DishonorsIt()
    {
        var (game, courtGames, _, opponentCharacter) = NewGamePoliticalConflict();
        var context = new AbilityContext { Game = game, Player = game.Player1, Source = courtGames, Target = opponentCharacter, ChosenChoice = "Dishonor an opposing character" };

        new CourtGamesHonorOrDishonorParticipant().Execute(context);

        Assert.That(opponentCharacter.IsDishonored, Is.True);
    }

    [Test]
    public void HonoringAnOpposingCharacter_Throws()
    {
        var (game, courtGames, _, opponentCharacter) = NewGamePoliticalConflict();
        var context = new AbilityContext { Game = game, Player = game.Player1, Source = courtGames, Target = opponentCharacter, ChosenChoice = "Honor a friendly character" };

        Assert.Throws<InvalidOperationException>(() => new CourtGamesHonorOrDishonorParticipant().Execute(context));
    }

    [Test]
    public void DuringAMilitaryConflict_Throws()
    {
        var (game, courtGames, myCharacter, _) = NewGamePoliticalConflict();
        game.CurrentConflict!.ConflictType = "military";
        var context = new AbilityContext { Game = game, Player = game.Player1, Source = courtGames, Target = myCharacter, ChosenChoice = "Honor a friendly character" };

        Assert.Throws<InvalidOperationException>(() => new CourtGamesHonorOrDishonorParticipant().Execute(context));
    }
}
