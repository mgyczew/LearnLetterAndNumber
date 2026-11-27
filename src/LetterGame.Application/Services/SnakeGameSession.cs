using LetterGame.Application.Models;
using LetterGame.Domain.Models;

namespace LetterGame.Application.Services;

public sealed class SnakeGameSession(Random? random = null)
{
    public const int Columns = 30;
    public const int Rows = 18;

    private const int MinimumWordLength = 3;
    private const int MaximumWordLength = 6;
    private readonly Random _random = random ?? new Random();
    private readonly List<(int X, int Y)> _body = [];
    private IReadOnlyList<string> _targetWords = [];
    private (int X, int Y) _direction = (1, 0);
    private (int X, int Y) _foodPosition;
    private string _targetWord = string.Empty;
    private int _targetLetterIndex;
    private int _score;
    private bool _isRunning;

    public SnakeGameState State => new(
        _body.ToArray(),
        _direction,
        _foodPosition,
        _targetWord,
        _targetLetterIndex,
        _score,
        _isRunning);

    public bool Start(IEnumerable<WordEntry> words)
    {
        _targetWords = words
            .Select(word => word.Text)
            .Where(word => word.Length is >= MinimumWordLength and <= MaximumWordLength && word.All(char.IsLetter))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
        if (_targetWords.Count == 0)
        {
            _isRunning = false;
            return false;
        }

        _body.Clear();
        var centerX = Columns / 2;
        var centerY = Rows / 2;
        _body.Add((centerX, centerY));
        _body.Add((centerX - 1, centerY));
        _body.Add((centerX - 2, centerY));
        _direction = (1, 0);
        _score = 0;
        InitializeNextWord();
        _isRunning = true;
        return true;
    }

    public void SetDirection(int x, int y)
    {
        var requested = (X: x, Y: y);
        if (!_isRunning
            || Math.Abs(x) + Math.Abs(y) != 1
            || requested == _direction
            || requested == (-_direction.X, -_direction.Y))
        {
            return;
        }

        _direction = requested;
    }

    public SnakeTickResult Tick()
    {
        if (!_isRunning || _body.Count == 0)
        {
            return SnakeTickResult.NotRunning;
        }

        var head = _body[0];
        var nextHead = (head.X + _direction.X, head.Y + _direction.Y);
        var collectingLetter = nextHead == _foodPosition;
        var bodyToCheck = collectingLetter ? _body : _body.Take(_body.Count - 1);
        if (nextHead.Item1 < 0 || nextHead.Item1 >= Columns
            || nextHead.Item2 < 0 || nextHead.Item2 >= Rows
            || bodyToCheck.Contains(nextHead))
        {
            _isRunning = false;
            return SnakeTickResult.GameOver;
        }

        _body.Insert(0, nextHead);
        if (!collectingLetter)
        {
            _body.RemoveAt(_body.Count - 1);
            return SnakeTickResult.Moved;
        }

        _targetLetterIndex++;
        if (_targetLetterIndex >= _targetWord.Length)
        {
            _score++;
            return SnakeTickResult.WordCompleted;
        }

        SpawnFood();
        return SnakeTickResult.LetterCollected;
    }

    public bool StartNextWord()
    {
        if (!_isRunning || _targetWords.Count == 0)
        {
            return false;
        }

        _targetWord = _targetWords[_random.Next(_targetWords.Count)];
        _targetLetterIndex = 0;
        SpawnFood();
        return true;
    }

    public void Stop() => _isRunning = false;

    private void InitializeNextWord()
    {
        _targetWord = _targetWords[_random.Next(_targetWords.Count)];
        _targetLetterIndex = 0;
        SpawnFood();
    }

    private void SpawnFood()
    {
        do
        {
            _foodPosition = (_random.Next(Columns), _random.Next(Rows));
        }
        while (_body.Contains(_foodPosition));
    }
}
