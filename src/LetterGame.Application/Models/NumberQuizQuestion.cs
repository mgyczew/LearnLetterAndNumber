namespace LetterGame.Application.Models;

/// <summary>Reprezentuje pytanie quizu liczbowego, odpowiedzi i poprawny wybór.</summary>
public sealed record NumberQuizQuestion(
    NumberQuizKind Kind,
    string Prompt,
    string SpokenPrompt,
    string CorrectAnswer,
    IReadOnlyList<string> Choices,
    int? Quantity = null);
