using LetterGame.Application.Models;
using LetterGame.Domain.Models;

namespace LetterGame.Application.Services;

/// <summary>Obs³uguje uk³adanie jednego s³owa z liter, podpowiedzi i rosn¹cej d³ugoœci rund.</summary>
public sealed class WordChainGameService(Random? random = null)
{
    private static readonly WordEntry[] FallbackWords =
    [
        new("KOT", "Zwierzê z kategorii zwierzêta", "??", ImageSearchTerm: "kot animal", CategoryName: "Zwierzêta"),
        new("LAS", "Miejsce pe³ne drzew", "??", ImageSearchTerm: "las forest", CategoryName: "Przyroda"),
        new("DOM", "Miejsce, w którym mieszkamy", "??", ImageSearchTerm: "dom house", CategoryName: "Dom"),
        new("PIES", "Zwierzê, które szczeka", "??", ImageSearchTerm: "pies dog", CategoryName: "Zwierzêta")
    ];

    private readonly Random _random = random ?? new Random();
    private IReadOnlyList<WordEntry> _words = [];
    private WordEntry? _targetWord;
    private char[] _letterBank = [];
    private readonly List<int> _selectedLetterIndices = [];
    private int _round;
    private int _score;
    private bool _isRoundComplete;

    public WordChainGameState State => new(
        _round,
        _score,
        _targetWord,
        _letterBank.ToArray(),
        _selectedLetterIndices.ToArray(),
        _isRoundComplete);

    public void Start(IEnumerable<WordEntry> words)
    {
        _words = words
            .Where(word => word.Text.Length is >= 3 and <= 8 && word.Text.All(char.IsLetter))
            .Select(word => word with { Text = word.Text.Trim().ToUpperInvariant() })
            .DistinctBy(word => word.Text, StringComparer.OrdinalIgnoreCase)
            .ToArray();
        if (_words.Count == 0)
        {
            _words = FallbackWords;
        }

        _round = 1;
        _score = 0;
        _targetWord = null;
        StartRound();
    }

    public bool SelectLetter(int index)
    {
        if (_isRoundComplete
            || _targetWord is null
            || index < 0
            || index >= _letterBank.Length
            || _selectedLetterIndices.Contains(index)
            || _selectedLetterIndices.Count >= _targetWord.Text.Length)
        {
            return false;
        }

        _selectedLetterIndices.Add(index);
        return true;
    }

    public bool UndoSelection()
    {
        if (_isRoundComplete || _selectedLetterIndices.Count == 0)
        {
            return false;
        }

        _selectedLetterIndices.RemoveAt(_selectedLetterIndices.Count - 1);
        return true;
    }

    public bool ClearSelection()
    {
        if (_isRoundComplete || _selectedLetterIndices.Count == 0)
        {
            return false;
        }

        _selectedLetterIndices.Clear();
        return true;
    }

    public WordChainAnswerResult SubmitAnswer()
    {
        if (_isRoundComplete || _targetWord is null || _selectedLetterIndices.Count != _targetWord.Text.Length)
        {
            return WordChainAnswerResult.Incomplete;
        }

        var answer = new string(_selectedLetterIndices.Select(index => _letterBank[index]).ToArray());
        if (!string.Equals(answer, _targetWord.Text, StringComparison.OrdinalIgnoreCase))
        {
            return WordChainAnswerResult.Incorrect;
        }

        _score++;
        _isRoundComplete = true;
        return WordChainAnswerResult.RoundSolved;
    }

    public bool NextRound()
    {
        if (!_isRoundComplete)
        {
            return false;
        }

        _round++;
        StartRound();
        return true;
    }

    private void StartRound()
    {
        var desiredLength = Math.Min(3 + _round - 1, 8);
        var targetLength = _words
            .Select(word => word.Text.Length)
            .Where(length => length >= desiredLength)
            .DefaultIfEmpty(_words.Max(word => word.Text.Length))
            .Min();
        var candidates = _words.Where(word => word.Text.Length == targetLength).ToArray();
        var unseenCandidates = candidates
            .Where(word => !string.Equals(word.Text, _targetWord?.Text, StringComparison.OrdinalIgnoreCase))
            .ToArray();
        if (unseenCandidates.Length > 0)
        {
            candidates = unseenCandidates;
        }

        _targetWord = candidates[_random.Next(candidates.Length)];
        _letterBank = _targetWord.Text.OrderBy(_ => _random.Next()).ToArray();
        _selectedLetterIndices.Clear();
        _isRoundComplete = false;
    }
}
