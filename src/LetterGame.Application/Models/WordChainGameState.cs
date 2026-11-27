namespace LetterGame.Application.Models;

public sealed record WordChainGameState(
    int Round,
    int Score,
    IReadOnlyList<string> Targets,
    IReadOnlyList<char> LetterBank,
    IReadOnlyList<int> SelectedLetterIndices,
    IReadOnlyList<string?> Answers,
    int ActiveWordIndex,
    bool IsRoundComplete);
