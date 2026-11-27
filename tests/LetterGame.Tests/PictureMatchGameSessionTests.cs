using LetterGame.Application.Models;
using LetterGame.Application.Services;
using LetterGame.Domain.Models;

namespace LetterGame.Tests;

public sealed class PictureMatchGameSessionTests
{
    [Fact]
    public void Validates_letter_order_and_tracks_solved_puzzles()
    {
        var game = new PictureMatchGameSession(new Random(1));
        Assert.True(game.Start([new WordEntry("LAS", "Przyroda", "??")], resetScore: true));

        Assert.Equal(PictureMatchAnswerResult.Incomplete, game.SubmitAnswer("LAS"));
        Assert.True(game.SelectLetter("LAS", 2));
        Assert.True(game.SelectLetter("LAS", 1));
        Assert.True(game.SelectLetter("LAS", 0));
        Assert.Equal(PictureMatchAnswerResult.Incorrect, game.SubmitAnswer("LAS"));

        Assert.True(game.ClearSelection("LAS"));
        Assert.True(game.SelectLetter("LAS", 0));
        Assert.True(game.SelectLetter("LAS", 1));
        Assert.True(game.SelectLetter("LAS", 2));
        Assert.Equal(PictureMatchAnswerResult.Correct, game.SubmitAnswer("LAS"));
        Assert.Equal(1, game.State.Score);
        Assert.True(game.State.Puzzles.Single().IsSolved);
    }
}
