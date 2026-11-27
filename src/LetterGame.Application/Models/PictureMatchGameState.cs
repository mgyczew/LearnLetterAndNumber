namespace LetterGame.Application.Models;

public sealed record PictureMatchGameState(
    int Score,
    IReadOnlyList<PictureMatchPuzzleState> Puzzles);
