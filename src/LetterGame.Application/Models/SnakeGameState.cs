namespace LetterGame.Application.Models;

public sealed record SnakeGameState(
    IReadOnlyList<(int X, int Y)> Body,
    (int X, int Y) Direction,
    (int X, int Y) FoodPosition,
    string TargetWord,
    int TargetLetterIndex,
    int Score,
    bool IsRunning);
