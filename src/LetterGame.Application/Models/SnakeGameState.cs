namespace LetterGame.Application.Models;

/// <summary>Opisuje pozycjê wê¿yka, cel zbierania liter i bie¿¹cy wynik.</summary>
public sealed record SnakeGameState(
    IReadOnlyList<(int X, int Y)> Body,
    (int X, int Y) Direction,
    (int X, int Y) FoodPosition,
    string TargetWord,
    int TargetLetterIndex,
    int Score,
    bool IsRunning);
