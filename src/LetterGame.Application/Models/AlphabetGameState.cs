using LetterGame.Domain.Models;

namespace LetterGame.Application.Models;

/// <summary>Widok stanu poziomu, s³owa docelowego, kafelków i wyniku ruchomego alfabetu.</summary>
public sealed record AlphabetGameState(
    int Level,
    WordEntry? Word,
    IReadOnlyList<string> LetterTiles,
    IReadOnlyList<int> SelectedTileIndices,
    int Score,
    bool IsSolved);
