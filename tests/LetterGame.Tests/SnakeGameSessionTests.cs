using LetterGame.Application.Models;
using LetterGame.Application.Services;
using LetterGame.Domain.Models;

namespace LetterGame.Tests;

public sealed class SnakeGameSessionTests
{
    [Fact]
    public void Starts_moves_and_stops_without_a_view_layer()
    {
        var game = new SnakeGameSession(new Random(1));
        Assert.True(game.Start([new WordEntry("KOT", "Zwierzê", "??")]));
        Assert.Equal(3, game.State.Body.Count);
        Assert.Equal(SnakeTickResult.Moved, game.Tick());
        Assert.Equal((16, 9), game.State.Body[0]);

        game.SetDirection(-1, 0);
        Assert.Equal((1, 0), game.State.Direction);
        game.Stop();
        Assert.Equal(SnakeTickResult.NotRunning, game.Tick());
    }

    [Fact]
    public void Cannot_start_without_words_of_supported_length()
    {
        var game = new SnakeGameSession(new Random(2));

        Assert.False(game.Start([new WordEntry("OK", "S³owo", "??")]));
        Assert.False(game.State.IsRunning);
    }
}
