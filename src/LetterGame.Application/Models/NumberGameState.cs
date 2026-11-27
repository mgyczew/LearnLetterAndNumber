namespace LetterGame.Application.Models;

/// <summary>Udostêpnia pytanie, odpowiedzi, zbudowan¹ liczbê i wynik gry liczbowej.</summary>
public sealed record NumberGameState(
    NumberGameMode Mode,
    string Prompt,
    string SpokenPrompt,
    IReadOnlyList<int> Choices,
    int? Quantity,
    string BuiltNumber,
    int Score,
    bool IsComplete);
