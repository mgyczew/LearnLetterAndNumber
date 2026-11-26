using LetterGame.Application.Models;
using LetterGame.Application.Services;
using LetterGame.Domain.Models;

namespace LetterGame.Tests;

public sealed class WordPuzzleSessionTests
{
    [Fact]
    public void Correct_answer_solves_puzzle_and_awards_score_once()
    {
        var session = new WordPuzzleSession();
        session.SetWord(new WordEntry("LAS", "Drzewo", "??"), resetPuzzle: true);

        Assert.True(session.SelectLetter(0));
        Assert.True(session.SelectLetter(1));
        Assert.True(session.SelectLetter(2));
        Assert.Equal(WordAnswerResult.Correct, session.CheckAnswer());
        Assert.True(session.IsSolved);
        Assert.Equal(1, session.Score);
        Assert.Equal(WordAnswerResult.Correct, session.CheckAnswer());
        Assert.Equal(1, session.Score);
    }

    [Fact]
    public void Rejects_duplicate_selection_and_reports_incomplete_answer()
    {
        var session = new WordPuzzleSession();
        session.SetWord(new WordEntry("LAS", "Drzewo", "??"), resetPuzzle: true);

        Assert.True(session.SelectLetter(0));
        Assert.False(session.SelectLetter(0));
        Assert.Equal(WordAnswerResult.Incomplete, session.CheckAnswer());
    }

    [Fact]
    public void Incorrect_order_can_be_cleared_and_retried()
    {
        var session = new WordPuzzleSession();
        session.SetWord(new WordEntry("LAS", "Drzewo", "??"), resetPuzzle: true);
        session.SelectLetter(2);
        session.SelectLetter(1);
        session.SelectLetter(0);

        Assert.Equal(WordAnswerResult.Incorrect, session.CheckAnswer());
        Assert.True(session.ClearSelection());
        session.SelectLetter(0);
        session.SelectLetter(1);
        session.SelectLetter(2);

        Assert.Equal(WordAnswerResult.Correct, session.CheckAnswer());
    }
}
