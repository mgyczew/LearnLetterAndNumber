using LetterGame.Domain.Models;

namespace LetterGame.Application.Models;

/// <summary>Udostêpnia bie¿¹ce s³owo, literki, rundê i wynik gry s³ownej.</summary>
public sealed record WordChainGameState(
    int Round,
    int Score,
    WordEntry? TargetWord,
    IReadOnlyList<char> LetterBank,
    IReadOnlyList<int> SelectedLetterIndices,
    bool IsRoundComplete);
