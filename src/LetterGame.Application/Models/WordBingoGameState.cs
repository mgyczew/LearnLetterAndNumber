namespace LetterGame.Application.Models;

/// <summary>Udostêpnia planszê bingo, zaznaczone pola i aktualne s³owo z losowania.</summary>
public sealed record WordBingoGameState(
    int BoardNumber,
    IReadOnlyList<string> BoardWords,
    IReadOnlyList<int> MarkedIndices,
    string? CurrentCall,
    int CallNumber,
    int TotalCalls,
    bool HasBingo,
    bool IsComplete);
