using LetterGame.Application.Models;
using LetterGame.Application.Services;
using LetterGame.Domain.Models;

namespace LetterGame.Tests;

public sealed class WordChainGameServiceTests
{
    [Fact]
    public void Starts_with_mat_and_tam_and_requires_both_words()
    {
        var game = new WordChainGameService(new Random(1));
        game.Start([]);

        var firstState = game.State;
        Assert.Equal(["MAT", "TAM"], firstState.Targets);
        Assert.Equal(3, firstState.LetterBank.Count);
        Assert.Equal(WordChainAnswerResult.Incomplete, game.SubmitAnswer());

        SelectTarget(game, firstState.Targets[0]);
        Assert.Equal(WordChainAnswerResult.FirstWordSolved, game.SubmitAnswer());
        Assert.Equal("MAT", game.State.Answers[0]);
        Assert.Equal(1, game.State.ActiveWordIndex);

        SelectTarget(game, firstState.Targets[1]);
        Assert.Equal(WordChainAnswerResult.RoundSolved, game.SubmitAnswer());
        Assert.Equal(2, game.State.Score);
        Assert.True(game.State.IsRoundComplete);
    }

    [Fact]
    public void Next_round_increases_the_word_length()
    {
        var game = new WordChainGameService(new Random(2));
        game.Start([]);
        SolveRound(game);

        Assert.True(game.NextRound());
        Assert.Equal(2, game.State.Round);
        Assert.Equal(4, game.State.Targets[0].Length);
        Assert.False(game.State.IsRoundComplete);
    }

    [Fact]
    public void Cannot_reuse_a_letter_tile_in_the_same_answer()
    {
        var game = new WordChainGameService(new Random(3));
        game.Start([]);

        Assert.True(game.SelectLetter(0));
        Assert.False(game.SelectLetter(0));
        Assert.True(game.UndoSelection());
        Assert.Empty(game.State.SelectedLetterIndices);
    }

    private static void SolveRound(WordChainGameService game)
    {
        var targets = game.State.Targets;
        SelectTarget(game, targets[0]);
        Assert.Equal(WordChainAnswerResult.FirstWordSolved, game.SubmitAnswer());
        SelectTarget(game, targets[1]);
        Assert.Equal(WordChainAnswerResult.RoundSolved, game.SubmitAnswer());
    }

    private static void SelectTarget(WordChainGameService game, string target)
    {
        var availableIndices = Enumerable.Range(0, game.State.LetterBank.Count).ToList();
        foreach (var letter in target)
        {
            var index = availableIndices.First(index => game.State.LetterBank[index] == letter);
            availableIndices.Remove(index);
            Assert.True(game.SelectLetter(index));
        }
    }
}
