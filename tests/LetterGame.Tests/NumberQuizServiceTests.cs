using LetterGame.Application.Models;
using LetterGame.Application.Services;
using LetterGame.Domain.Services;

namespace LetterGame.Tests;

public sealed class NumberQuizServiceTests
{
    [Fact]
    public void Generates_questions_that_link_names_digits_and_quantities()
    {
        var service = new NumberQuizService(new Random(123));

        for (var index = 0; index < 300; index++)
        {
            var question = service.CreateQuestion();

            Assert.Equal(4, question.Choices.Count);
            Assert.Equal(4, question.Choices.Distinct().Count());
            Assert.Contains(question.CorrectAnswer, question.Choices);

            if (question.Kind == NumberQuizKind.DigitToName)
            {
                Assert.Contains(question.CorrectAnswer, Enumerable.Range(1, NumberQuizService.MaximumNumber).Select(PolishNumberWords.ToWords));
                Assert.Null(question.Quantity);
            }
            else
            {
                Assert.InRange(int.Parse(question.CorrectAnswer), 1, NumberQuizService.MaximumNumber);
            }

            if (question.Kind == NumberQuizKind.CountObjects)
            {
                Assert.NotNull(question.Quantity);
                Assert.InRange(question.Quantity.Value, 1, NumberQuizService.MaximumNumber);
                Assert.Equal(question.CorrectAnswer, question.Quantity.Value.ToString());
            }
            else
            {
                Assert.Null(question.Quantity);
            }
        }
    }
}
