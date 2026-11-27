using LetterGame.Domain.Models;
using LetterGame.Domain.Services;

namespace LetterGame.Application.Models;

/// <summary>Przygotowuje zadanie matematyczne w formie tekstowej i g³osowej dla interfejsu.</summary>
public sealed record MathQuestionViewModel(
    string QuestionText,
    string SpokenQuestion,
    IReadOnlyList<int> Choices)
{
    public static MathQuestionViewModel From(MathQuestion question)
    {
        var (symbol, spokenOperator) = question.Lesson switch
        {
            MathLesson.Addition => ("+", "plus"),
            MathLesson.Subtraction => ("\u2212", "minus"),
            MathLesson.Multiplication => ("×", "razy"),
            MathLesson.Division => ("÷", "podzieliæ przez"),
            _ => throw new ArgumentOutOfRangeException(nameof(question), question.Lesson, "Expected an arithmetic lesson.")
        };

        var firstNumberWords = PolishNumberWords.ToWords(question.FirstOperand);
        var secondNumberWords = PolishNumberWords.ToWords(question.SecondOperand);
        var spokenQuestion = question.Lesson == MathLesson.Subtraction
            ? $"Ile to jest: {firstNumberWords} minus {secondNumberWords}?"
            : $"Ile to {firstNumberWords} {spokenOperator} {secondNumberWords}?";

        return new MathQuestionViewModel(
            $"{question.FirstOperand} {symbol} {question.SecondOperand} = ?",
            spokenQuestion,
            question.Choices);
    }
}
