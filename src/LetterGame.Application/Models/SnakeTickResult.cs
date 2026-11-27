namespace LetterGame.Application.Models;

/// <summary>Opisuje rezultat pojedynczego kroku wê¿yka.</summary>
public enum SnakeTickResult
{
    NotRunning,
    Moved,
    LetterCollected,
    WordCompleted,
    GameOver
}
