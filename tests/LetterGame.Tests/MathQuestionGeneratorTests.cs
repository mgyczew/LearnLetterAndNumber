using LetterGame.Domain.Models;
using LetterGame.Domain.Services;

namespace LetterGame.Tests;

public sealed class MathQuestionGeneratorTests
{
    [Theory]
    [InlineData(0, 20)]
    [InlineData(4, 20)]
    [InlineData(5, 100)]
    [InlineData(19, 100)]
    [InlineData(20, 1000)]
    public void Addition_respects_progression_limit(int correctAnswers, int limit)
    {
        var generator = new MathQuestionGenerator(new Random(123));

        for (var index = 0; index < 100; index++)
        {
            var question = generator.Create(MathLesson.Addition, correctAnswers);

            Assert.Equal(question.FirstOperand + question.SecondOperand, question.CorrectAnswer);
            Assert.InRange(question.CorrectAnswer, 1, limit);
            AssertValidChoices(question);
        }
    }

    [Theory]
    [InlineData(0, 20)]
    [InlineData(5, 100)]
    [InlineData(20, 1000)]
    public void Subtraction_never_generates_negative_answers(int correctAnswers, int limit)
    {
        var generator = new MathQuestionGenerator(new Random(321));

        for (var index = 0; index < 100; index++)
        {
            var question = generator.Create(MathLesson.Subtraction, correctAnswers);

            Assert.Equal(question.FirstOperand - question.SecondOperand, question.CorrectAnswer);
            Assert.InRange(question.CorrectAnswer, 0, limit);
            AssertValidChoices(question);
        }
    }

    [Theory]
    [InlineData(0, 2, 10)]
    [InlineData(5, 1, 10)]
    [InlineData(20, 1, 100)]
    public void Multiplication_respects_operand_progression(int correctAnswers, int minimumFirst, int maximumFirst)
    {
        var generator = new MathQuestionGenerator(new Random(456));

        for (var index = 0; index < 100; index++)
        {
            var question = generator.Create(MathLesson.Multiplication, correctAnswers);

            Assert.InRange(question.FirstOperand, minimumFirst, maximumFirst);
            Assert.InRange(question.SecondOperand, 1, correctAnswers < 5 ? 5 : 10);
            Assert.Equal(question.FirstOperand * question.SecondOperand, question.CorrectAnswer);
            AssertValidChoices(question);
        }
    }

    [Theory]
    [InlineData(0)]
    [InlineData(5)]
    [InlineData(20)]
    public void Division_questions_have_whole_number_answers(int correctAnswers)
    {
        var generator = new MathQuestionGenerator(new Random(789));

        for (var index = 0; index < 100; index++)
        {
            var question = generator.Create(MathLesson.Division, correctAnswers);

            Assert.NotEqual(0, question.SecondOperand);
            Assert.Equal(0, question.FirstOperand % question.SecondOperand);
            Assert.Equal(question.FirstOperand / question.SecondOperand, question.CorrectAnswer);
            AssertValidChoices(question);
        }
    }

    [Fact]
    public void Number_lesson_does_not_generate_arithmetic_questions()
    {
        var generator = new MathQuestionGenerator(new Random(1));

        Assert.Throws<ArgumentException>(() => generator.Create(MathLesson.Numbers, 0));
    }

    private static void AssertValidChoices(MathQuestion question)
    {
        Assert.Equal(4, question.Choices.Count);
        Assert.Equal(4, question.Choices.Distinct().Count());
        Assert.Contains(question.CorrectAnswer, question.Choices);
        Assert.All(question.Choices, choice => Assert.InRange(choice, 0, 1000));
    }
}
