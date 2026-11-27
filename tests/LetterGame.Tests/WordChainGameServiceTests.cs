using LetterGame.Application.Models;
using LetterGame.Application.Services;
using LetterGame.Domain.Models;

namespace LetterGame.Tests;

public sealed class WordChainGameServiceTests
{
    [Fact]
    public void Starts_with_one_word_and_completes_after_a_single_answer()
    {
        var game = new WordChainGameService(new Random(1));
        game.Start(CreateWords());

        var state = game.State;
        Assert.NotNull(state.TargetWord);
        Assert.Equal(3, state.TargetWord.Text.Length);
        Assert.NotNull(state.TargetWord.CategoryName);
        Assert.Equal(state.TargetWord.Text.Length, state.LetterBank.Count);
        Assert.Equal(WordChainAnswerResult.Incomplete, game.SubmitAnswer());

        SelectTarget(game);
        Assert.Equal(WordChainAnswerResult.RoundSolved, game.SubmitAnswer());
        Assert.Equal(1, game.State.Score);
        Assert.True(game.State.IsRoundComplete);
    }

    [Fact]
    public void Next_round_increases_the_word_length()
    {
        var game = new WordChainGameService(new Random(2));
        game.Start(CreateWords());
        SolveRound(game);

        Assert.True(game.NextRound());
        Assert.Equal(2, game.State.Round);
        Assert.Equal(4, game.State.TargetWord?.Text.Length);
        Assert.False(game.State.IsRoundComplete);
    }

    [Fact]
    public void Cannot_reuse_a_letter_tile_in_the_same_answer()
    {
        var game = new WordChainGameService(new Random(3));
        game.Start(CreateWords());

        Assert.True(game.SelectLetter(0));
        Assert.False(game.SelectLetter(0));
        Assert.True(game.UndoSelection());
        Assert.Empty(game.State.SelectedLetterIndices);
    }

    private static void SolveRound(WordChainGameService game)
    {
        SelectTarget(game);
        Assert.Equal(WordChainAnswerResult.RoundSolved, game.SubmitAnswer());
    }

    private static void SelectTarget(WordChainGameService game)
    {
        var target = game.State.TargetWord!.Text;
        var availableIndices = Enumerable.Range(0, game.State.LetterBank.Count).ToList();
        foreach (var letter in target)
        {
            var index = availableIndices.First(index => game.State.LetterBank[index] == letter);
            availableIndices.Remove(index);
            Assert.True(game.SelectLetter(index));
        }
    }

    private static WordEntry[] CreateWords() =>
    [
        new("LAS", "Miejsce pe³ne drzew", "??", CategoryName: "Przyroda"),
        new("KOT", "Zwierzê", "??", CategoryName: "Zwierzêta"),
        new("PIES", "Zwierzê, które szczeka", "??", CategoryName: "Zwierzêta"),
        new("MAMA", "Bliska osoba", "??", CategoryName: "Rodzina")
    ];
}
