namespace LetterGame.Application.Models;

/// <summary>Udostêpnia wynik i wszystkie zagadki na aktualnej planszy obrazkowej.</summary>
public sealed record PictureMatchGameState(
    int Score,
    IReadOnlyList<PictureMatchPuzzleState> Puzzles);
