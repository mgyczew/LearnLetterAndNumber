using LetterGame.Application.Services;
using LetterGame.Domain.Models;
using LetterGame.Domain.Services;

namespace LetterGame.Tests;

public sealed class MathLearningServiceTests
{
    [Fact]
    public void Difficulty_hint_advances_after_five_and_twenty_correct_answers()
    {
        var service = new MathLearningService(new MathQuestionGenerator(new Random(42)));
        service.SelectLesson(MathLesson.Addition);

        Assert.Contains("20", service.GetDifficultyHint());
        AnswerCorrectly(service, 5);
        Assert.Contains("100", service.GetDifficultyHint());
        AnswerCorrectly(service, 15);
        Assert.Contains("1000", service.GetDifficultyHint());
    }

    [Fact]
    public void Correct_answers_are_tracked_per_lesson()
    {
        var service = new MathLearningService(new MathQuestionGenerator(new Random(7)));
        service.SelectLesson(MathLesson.Addition);
        AnswerCorrectly(service, 1);
        service.SelectLesson(MathLesson.Subtraction);

        Assert.Equal(0, service.CorrectAnswersInSelectedLesson);
        AnswerCorrectly(service, 1);
        Assert.Equal(2, service.Score);
        Assert.Equal(1, service.CorrectAnswersInSelectedLesson);
    }

    [Fact]
    public void Number_navigation_wraps_at_supported_boundaries()
    {
        var service = new MathLearningService(new MathQuestionGenerator(new Random(1)));

        service.MoveNumber(-1);
        Assert.Equal(1000, service.CurrentNumber);
        service.MoveNumber(1);
        Assert.Equal(1, service.CurrentNumber);
    }

    private static void AnswerCorrectly(MathLearningService service, int count)
    {
        for (var index = 0; index < count; index++)
        {
            var question = service.CreateQuestion();
            Assert.True(service.SubmitAnswer(question.CorrectAnswer));
        }
    }
}
