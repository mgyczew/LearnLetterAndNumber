using LetterGame.Domain.Models;
using LetterGame.Domain.Services;

namespace LetterGame.Application.Models;

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
            MathLesson.Subtraction => ("?", "minus"),
            MathLesson.Multiplication => ("×", "razy"),
            MathLesson.Division => ("÷", "podzieliæ przez"),
            _ => throw new ArgumentOutOfRangeException(nameof(question), question.Lesson, "Expected an arithmetic lesson.")
        };

        return new MathQuestionViewModel(
            $"{question.FirstOperand} {symbol} {question.SecondOperand} = ?",
            $"Ile to {PolishNumberWords.ToWords(question.FirstOperand)} {spokenOperator} {PolishNumberWords.ToWords(question.SecondOperand)}?",
            question.Choices);
    }
}
