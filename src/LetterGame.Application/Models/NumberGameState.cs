namespace LetterGame.Application.Models;

public sealed record NumberGameState(
    NumberGameMode Mode,
    string Prompt,
    string SpokenPrompt,
    IReadOnlyList<int> Choices,
    int? Quantity,
    string BuiltNumber,
    int Score,
    bool IsComplete);
