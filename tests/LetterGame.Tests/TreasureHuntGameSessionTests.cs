using LetterGame.Application.Services;

namespace LetterGame.Tests;

public sealed class TreasureHuntGameSessionTests
{
    [Fact]
    public void Tracks_found_objects_and_cycles_special_missions()
    {
        var game = new TreasureHuntGameSession();
        game.Start();
        var firstMission = game.State.Mission;

        Assert.Equal(12, game.State.TotalWords);
        Assert.NotNull(game.State.CurrentWord);
        Assert.True(game.MarkFound());
        Assert.Equal(1, game.State.FoundCount);
        Assert.NotEqual(firstMission, GetNextMission(game));

        while (!game.State.IsComplete)
        {
            Assert.True(game.MarkFound());
        }

        Assert.Equal(12, game.State.FoundCount);
        Assert.Null(game.State.CurrentWord);
        Assert.False(game.MarkFound());
    }

    [Fact]
    public void Skipping_an_object_advances_the_clue_without_counting_it_as_found()
    {
        var game = new TreasureHuntGameSession();
        game.Start();

        Assert.True(game.SkipWord());

        Assert.Equal(1, game.State.CurrentIndex);
        Assert.Equal(0, game.State.FoundCount);
        Assert.Equal("OKNO", game.State.CurrentWord?.Text);
    }

    private static string GetNextMission(TreasureHuntGameSession game)
    {
        game.NextMission();
        return game.State.Mission;
    }
}
