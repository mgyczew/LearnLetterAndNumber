using LetterGame.Application.Models;
using LetterGame.Domain.Services;

namespace LetterGame.Application.Services;

/// <summary>Tworzy pytania quizowe o zapisie liczb cyframi i s³ownie.</summary>
public sealed class NumberQuizService(Random? random = null)
{
    public const int MaximumNumber = 20;

    private readonly Random _random = random ?? Random.Shared;

    public NumberQuizQuestion CreateQuestion()
    {
        var number = _random.Next(1, MaximumNumber + 1);
        var kind = (NumberQuizKind)_random.Next(Enum.GetValues<NumberQuizKind>().Length);
        var numberWords = PolishNumberWords.ToWords(number);

        return kind switch
        {
            NumberQuizKind.NameToDigit => new NumberQuizQuestion(
                kind,
                $"Wybierz zapis cyfrowy liczby: {numberWords}.",
                $"Wybierz zapis cyfrowy liczby: {numberWords}.",
                number.ToString(),
                CreateChoices(number, useWords: false)),
            NumberQuizKind.DigitToName => new NumberQuizQuestion(
                kind,
                $"Jak zapisujemy s³ownie liczbê {number}?",
                $"Jak zapisujemy s³ownie liczbê {number}?",
                numberWords,
                CreateChoices(number, useWords: true)),
            NumberQuizKind.CountObjects => new NumberQuizQuestion(
                kind,
                "Policz kropki. Ile ich jest?",
                "Policz kropki. Ile ich jest?",
                number.ToString(),
                CreateChoices(number, useWords: false),
                number),
            _ => throw new ArgumentOutOfRangeException(nameof(kind))
        };
    }

    private string[] CreateChoices(int correctAnswer, bool useWords)
    {
        var choices = new HashSet<int> { correctAnswer };
        while (choices.Count < 4)
        {
            choices.Add(_random.Next(1, MaximumNumber + 1));
        }

        return choices
            .OrderBy(_ => _random.Next())
            .Select(number => useWords ? PolishNumberWords.ToWords(number) : number.ToString())
            .ToArray();
    }
}
