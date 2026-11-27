namespace LetterGame.Application.Services;

using LetterGame.Application.Models;

public sealed class WordBingoGameSession(Random? random = null)
{
    private static readonly string[] CallWords =
    [
        "KOT", "PIES", "DOM", "LODY", "AUTO", "¯ABA", "MAMA", "LAS", "RYBA", "SOWA", "MLEKO", "KWIAT", "S£OÑ", "BALON"
    ];

    private static readonly string[][] Boards =
    [
        ["KOT", "DOM", "LODY", "AUTO", "RYBA", "LAS", "¯ABA", "MLEKO", "SOWA"],
        ["PIES", "LAS", "AUTO", "KOT", "S£OÑ", "DOM", "BALON", "LODY", "KWIAT"],
        ["RYBA", "KWIAT", "MAMA", "SOWA", "PIES", "¯ABA", "DOM", "BALON", "MLEKO"],
        ["S£OÑ", "MAMA", "KOT", "LODY", "KWIAT", "RYBA", "LAS", "AUTO", "PIES"]
    ];

    private readonly Random _random = random ?? new Random();
    private readonly List<int> _markedIndices = [];
    private int _boardNumber = 1;
    private string? _currentCall;
    private string[] _callOrder = [];
    private int _callIndex;
    private bool _hasBingo;

    public WordBingoGameState State => new(
        _boardNumber,
        Boards[_boardNumber - 1].ToArray(),
        _markedIndices.ToArray(),
        _currentCall,
        _callIndex,
        _callOrder.Length,
        _hasBingo,
        _hasBingo || (_callIndex >= _callOrder.Length && _currentCall is null));

    /// <summary>Rozpoczyna grê na jednej z czterech gotowych plansz.</summary>
    public void Start(int boardNumber)
    {
        _boardNumber = Math.Clamp(boardNumber, 1, Boards.Length);
        _markedIndices.Clear();
        _callOrder = CallWords.OrderBy(_ => _random.Next()).ToArray();
        _callIndex = 0;
        _currentCall = null;
        _hasBingo = false;
    }

    /// <summary>Losuje nastêpne s³owo z puli; po bingo lub wyczerpaniu puli zwraca null.</summary>
    public string? NextCall()
    {
        if (_hasBingo || _callIndex >= _callOrder.Length)
        {
            _currentCall = null;
            return null;
        }

        _currentCall = _callOrder[_callIndex++];
        return _currentCall;
    }

    /// <summary>Zaznacza pole tylko wtedy, gdy zawiera aktualnie losowane s³owo.</summary>
    public bool MarkCurrentWord(int cellIndex)
    {
        var board = Boards[_boardNumber - 1];
        if (_hasBingo
            || _currentCall is null
            || cellIndex < 0
            || cellIndex >= board.Length
            || _markedIndices.Contains(cellIndex)
            || !string.Equals(board[cellIndex], _currentCall, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        _markedIndices.Add(cellIndex);
        _hasBingo = HasCompleteLine();
        return true;
    }

    private bool HasCompleteLine()
    {
        int[][] lines =
        [
            [0, 1, 2], [3, 4, 5], [6, 7, 8],
            [0, 3, 6], [1, 4, 7], [2, 5, 8],
            [0, 4, 8], [2, 4, 6]
        ];

        return lines.Any(line => line.All(_markedIndices.Contains));
    }
}
