namespace LetterGame.Domain.Models;

/// <summary>Reprezentuje dzia³anie matematyczne, poprawny wynik i zestaw odpowiedzi do wyboru.</summary>
public sealed record MathQuestion(
    int FirstOperand,
    int SecondOperand,
    MathLesson Lesson,
    int CorrectAnswer,
    IReadOnlyList<int> Choices);
