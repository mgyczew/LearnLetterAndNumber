using LetterGame.Application.Models;
using LetterGame.Domain.Models;

namespace LetterGame.Tests;

public sealed class MathQuestionViewModelTests
{
    [Fact]
    public void Formats_subtraction_for_screen_and_speech()
    {
        var question = new MathQuestion(8, 3, MathLesson.Subtraction, 5, [5, 4, 6, 3]);

        var viewModel = MathQuestionViewModel.From(question);

        Assert.Equal("8 ? 3 = ?", viewModel.QuestionText);
        Assert.Equal("Ile to osiem minus trzy?", viewModel.SpokenQuestion);
        Assert.Equal(question.Choices, viewModel.Choices);
    }

    [Fact]
    public void Rejects_number_lesson_as_an_arithmetic_question()
    {
        var question = new MathQuestion(1, 2, MathLesson.Numbers, 3, [3, 2, 4, 1]);

        Assert.Throws<ArgumentOutOfRangeException>(() => MathQuestionViewModel.From(question));
    }
}
