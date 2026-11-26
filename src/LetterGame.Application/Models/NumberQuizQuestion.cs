namespace LetterGame.Application.Models;

public sealed record NumberQuizQuestion(
    NumberQuizKind Kind,
    string Prompt,
    string SpokenPrompt,
    string CorrectAnswer,
    IReadOnlyList<string> Choices,
    int? Quantity = null);
