namespace LetterGame.Domain.Models;

public sealed record MathQuestion(
    int FirstOperand,
    int SecondOperand,
    MathLesson Lesson,
    int CorrectAnswer,
    IReadOnlyList<int> Choices);
