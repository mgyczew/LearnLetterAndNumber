using LetterGame.Application.Services;

namespace LetterGame.Tests;

public sealed class WordBingoGameSessionTests
{
    [Fact]
    public void Marks_only_the_called_word_and_detects_a_completed_row()
    {
        var game = new WordBingoGameSession(new Random(7));
        game.Start(1);
        Assert.Null(game.State.CurrentCall);
        Assert.False(game.MarkCurrentWord(0));

        var firstRow = new[] { 0, 1, 2 };
        while (!game.State.HasBingo)
        {
            var calledWord = game.NextCall();
            if (calledWord is null)
            {
                break;
            }

            var index = Array.IndexOf(game.State.BoardWords.ToArray(), calledWord);
            if (firstRow.Contains(index))
            {
                Assert.True(game.MarkCurrentWord(index));
            }
        }

        Assert.True(game.State.HasBingo);
        Assert.True(game.State.IsComplete);
    }

    [Fact]
    public void Requires_a_matching_current_call_and_resets_the_board_when_restarted()
    {
        var game = new WordBingoGameSession(new Random(3));
        game.Start(1);
        string? calledWord;
        var matchingCell = -1;
        do
        {
            calledWord = game.NextCall();
            Assert.NotNull(calledWord);
            matchingCell = Array.IndexOf(game.State.BoardWords.ToArray(), calledWord);
        } while (matchingCell < 0);

        var otherCell = (matchingCell + 1) % game.State.BoardWords.Count;
        Assert.False(game.MarkCurrentWord(otherCell));
        Assert.Empty(game.State.MarkedIndices);

        Assert.True(game.MarkCurrentWord(matchingCell));
        Assert.False(game.MarkCurrentWord(matchingCell));

        game.Start(2);

        Assert.Equal(2, game.State.BoardNumber);
        Assert.Empty(game.State.MarkedIndices);
        Assert.Null(game.State.CurrentCall);
        Assert.False(game.State.HasBingo);
    }
}
