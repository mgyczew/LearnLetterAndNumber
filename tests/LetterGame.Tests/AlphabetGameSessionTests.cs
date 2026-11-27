using LetterGame.Application.Models;
using LetterGame.Application.Services;

namespace LetterGame.Tests;

public sealed class AlphabetGameSessionTests
{
    [Fact]
    public void Builds_a_word_from_its_letter_tiles_and_keeps_the_selected_level()
    {
        var game = new AlphabetGameSession(new Random(5));
        game.Start(2);

        var state = game.State;
        Assert.Equal(2, state.Level);
        Assert.NotNull(state.Word);
        Assert.NotNull(state.Word.CategoryName);

        foreach (var token in Tokenize(state.Word.Text))
        {
            var index = Enumerable.Range(0, game.State.LetterTiles.Count)
                .First(candidate => !game.State.SelectedTileIndices.Contains(candidate)
                    && game.State.LetterTiles[candidate] == token);
            Assert.True(game.SelectTile(index));
        }

        Assert.Equal(WordAnswerResult.Correct, game.CheckAnswer());
        Assert.True(game.State.IsSolved);
        Assert.Equal(1, game.State.Score);
    }

    [Fact]
    public void Rejects_duplicate_or_invalid_tiles_and_reports_incomplete_answers()
    {
        var game = new AlphabetGameSession(new Random(5));
        game.Start(1);

        Assert.Equal(WordAnswerResult.Incomplete, game.CheckAnswer());
        Assert.False(game.SelectTile(-1));
        Assert.False(game.SelectTile(game.State.LetterTiles.Count));
        Assert.True(game.SelectTile(0));
        Assert.False(game.SelectTile(0));
        Assert.True(game.UndoSelection());
        Assert.False(game.UndoSelection());
    }

    private static IReadOnlyList<string> Tokenize(string word)
    {
        var tokens = new List<string>();
        for (var index = 0; index < word.Length;)
        {
            var digraph = new[] { "SZ", "CZ", "RZ", "CH" }
                .FirstOrDefault(candidate => word.AsSpan(index).StartsWith(candidate, StringComparison.OrdinalIgnoreCase));
            if (digraph is null)
            {
                tokens.Add(word[index].ToString());
                index++;
            }
            else
            {
                tokens.Add(digraph);
                index += digraph.Length;
            }
        }

        return tokens;
    }
}
