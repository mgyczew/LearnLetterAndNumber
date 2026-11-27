using LetterGame.Application.Models;
using LetterGame.Application.Services;

namespace LetterGame.Tests;

public sealed class NumberGameSessionTests
{
    [Fact]
    public void Count_question_contains_the_correct_quantity_and_distinct_choices()
    {
        var session = new NumberGameSession(new Random(5));
        session.Start(NumberGameMode.Count);

        var state = session.State;
        Assert.InRange(state.Quantity!.Value, 1, 10);
        Assert.Contains(state.Quantity.Value, state.Choices);
        Assert.Equal(4, state.Choices.Distinct().Count());
    }

    [Fact]
    public void Build_mode_accepts_two_digits_and_can_clear_them()
    {
        var session = new NumberGameSession(new Random(6));
        session.Start(NumberGameMode.Build);

        Assert.True(session.AppendDigit(4));
        Assert.Equal(NumberGameAnswerResult.Incomplete, session.SubmitBuiltNumber());
        Assert.True(session.AppendDigit(2));
        Assert.False(session.AppendDigit(9));
        Assert.Equal("42", session.State.BuiltNumber);
        Assert.NotEqual(NumberGameAnswerResult.Incomplete, session.SubmitBuiltNumber());

        session.ClearBuiltNumber();
        Assert.Equal(string.Empty, session.State.BuiltNumber);
    }

    [Fact]
    public void Find_digit_question_offers_all_digits()
    {
        var session = new NumberGameSession(new Random(7));
        session.Start(NumberGameMode.Find);

        Assert.Equal(Enumerable.Range(0, 10), session.State.Choices.OrderBy(value => value));
        Assert.False(string.IsNullOrWhiteSpace(session.State.SpokenPrompt));
    }
}
