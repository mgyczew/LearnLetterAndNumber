using LetterGame.Domain.Models;

namespace LetterGame.Domain.Services;

public sealed class MathQuestionGenerator(Random? random = null)
{
    private readonly Random _random = random ?? Random.Shared;

    public MathQuestion Create(MathLesson lesson, int correctAnswers)
    {
        if (lesson == MathLesson.Numbers)
        {
            throw new ArgumentException("The number lesson does not generate arithmetic questions.", nameof(lesson));
        }

        if (correctAnswers < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(correctAnswers));
        }

        var (first, second, answer) = lesson switch
        {
            MathLesson.Addition => CreateAddition(correctAnswers),
            MathLesson.Subtraction => CreateSubtraction(correctAnswers),
            MathLesson.Multiplication => CreateMultiplication(correctAnswers),
            MathLesson.Division => CreateDivision(correctAnswers),
            _ => throw new ArgumentOutOfRangeException(nameof(lesson))
        };

        return new MathQuestion(first, second, lesson, answer, CreateChoices(answer));
    }

    private (int First, int Second, int Answer) CreateAddition(int correctAnswers)
    {
        var limit = GetBasicLimit(correctAnswers);
        var first = _random.Next(0, limit + 1);
        var second = _random.Next(0, limit - first + 1);
        if (first == 0 && second == 0)
        {
            first = 1;
        }

        return (first, second, first + second);
    }

    private (int First, int Second, int Answer) CreateSubtraction(int correctAnswers)
    {
        var limit = GetBasicLimit(correctAnswers);
        var first = _random.Next(1, limit + 1);
        var second = _random.Next(0, first + 1);
        return (first, second, first - second);
    }

    private (int First, int Second, int Answer) CreateMultiplication(int correctAnswers)
    {
        int first;
        int second;
        if (correctAnswers < 5)
        {
            first = new[] { 2, 5, 10 }[_random.Next(3)];
            second = _random.Next(1, 6);
        }
        else if (correctAnswers < 20)
        {
            first = _random.Next(1, 11);
            second = _random.Next(1, 11);
        }
        else
        {
            first = _random.Next(1, 101);
            second = _random.Next(1, 11);
        }

        return (first, second, first * second);
    }

    private (int First, int Second, int Answer) CreateDivision(int correctAnswers)
    {
        int divisor;
        int quotient;
        if (correctAnswers < 5)
        {
            divisor = new[] { 2, 5, 10 }[_random.Next(3)];
            quotient = _random.Next(1, 6);
        }
        else if (correctAnswers < 20)
        {
            divisor = _random.Next(2, 11);
            quotient = _random.Next(1, 11);
        }
        else
        {
            divisor = _random.Next(2, 11);
            quotient = _random.Next(1, 101);
        }

        return (divisor * quotient, divisor, quotient);
    }

    private static int GetBasicLimit(int correctAnswers) =>
        correctAnswers < 5 ? 20 : correctAnswers < 20 ? 100 : 1000;

    private int[] CreateChoices(int answer)
    {
        var choices = new HashSet<int> { answer };
        var offsets = new[] { 1, -1, 2, -2, 5, -5, 10, -10, 20, -20, 50, -50 };
        foreach (var offset in offsets.OrderBy(_ => _random.Next()))
        {
            var candidate = answer + offset;
            if (candidate is >= 0 and <= 1000)
            {
                choices.Add(candidate);
            }

            if (choices.Count == 4)
            {
                break;
            }
        }

        while (choices.Count < 4)
        {
            choices.Add(_random.Next(0, 1001));
        }

        return choices.OrderBy(_ => _random.Next()).ToArray();
    }
}
