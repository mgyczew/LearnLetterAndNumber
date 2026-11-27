using LetterGame.Application.Models;
using LetterGame.Domain.Models;

namespace LetterGame.Application.Services;

public sealed class WordChainGameService(Random? random = null)
{
    private static readonly (string First, string Second)[] FallbackPairs =
    [
        ("MAT", "TAM"),
        ("KOS", "SOK"),
        ("KOT", "TOK"),
        ("DOM", "MOD"),
        ("BARK", "BRAK"),
        ("TORA", "ROTA"),
        ("KORA", "OKRA"),
        ("KARTA", "KARAT")
    ];

    private readonly Random _random = random ?? new Random();
    private IReadOnlyList<(string First, string Second)> _wordPairs = [];
    private string[] _targets = [];
    private char[] _letterBank = [];
    private readonly List<int> _selectedLetterIndices = [];
    private readonly string?[] _answers = new string?[2];
    private int _round;
    private int _score;
    private int _activeWordIndex;
    private bool _isRoundComplete;

    public WordChainGameState State => new(
        _round,
        _score,
        _targets.ToArray(),
        _letterBank.ToArray(),
        _selectedLetterIndices.ToArray(),
        _answers.ToArray(),
        _activeWordIndex,
        _isRoundComplete);

    public void Start(IEnumerable<WordEntry> words)
    {
        _wordPairs = CreateWordPairs(words);
        _round = 1;
        _score = 0;
        StartRound(3);
    }

    public bool SelectLetter(int index)
    {
        if (_isRoundComplete
            || _targets.Length != 2
            || index < 0
            || index >= _letterBank.Length
            || _selectedLetterIndices.Contains(index)
            || _selectedLetterIndices.Count >= _targets[_activeWordIndex].Length)
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
        if (_targets.Length != 2 || _selectedLetterIndices.Count != _targets[_activeWordIndex].Length)
        {
            return WordChainAnswerResult.Incomplete;
        }

        var answer = new string(_selectedLetterIndices.Select(index => _letterBank[index]).ToArray());
        if (!string.Equals(answer, _targets[_activeWordIndex], StringComparison.OrdinalIgnoreCase))
        {
            return WordChainAnswerResult.Incorrect;
        }

        _answers[_activeWordIndex] = answer;
        _score++;
        if (_activeWordIndex == 0)
        {
            _activeWordIndex = 1;
            _selectedLetterIndices.Clear();
            return WordChainAnswerResult.FirstWordSolved;
        }

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
        StartRound(Math.Min(3 + _round - 1, 5));
        return true;
    }

    private void StartRound(int minimumLength)
    {
        var pair = SelectWordPair(minimumLength);
        _targets = [pair.First, pair.Second];
        _letterBank = pair.First.OrderBy(_ => _random.Next()).ToArray();
        _selectedLetterIndices.Clear();
        Array.Fill(_answers, null);
        _activeWordIndex = 0;
        _isRoundComplete = false;
    }

    private (string First, string Second) SelectWordPair(int minimumLength)
    {
        if (_round == 1)
        {
            return FallbackPairs[0];
        }

        var pairs = _wordPairs
            .Concat(FallbackPairs)
            .Where(pair => pair.First.Length == pair.Second.Length
                && pair.First.Length >= minimumLength
                && pair.First.Length <= 6)
            .DistinctBy(pair => string.Join("|", new[] { pair.First, pair.Second }.OrderBy(word => word, StringComparer.OrdinalIgnoreCase)))
            .ToArray();
        if (pairs.Length == 0)
        {
            pairs = FallbackPairs.Where(pair => pair.First.Length == 3).ToArray();
        }

        var targetLength = pairs.Min(pair => pair.First.Length);
        var candidates = pairs.Where(pair => pair.First.Length == targetLength).ToArray();
        return candidates[_random.Next(candidates.Length)];
    }

    private static IReadOnlyList<(string First, string Second)> CreateWordPairs(IEnumerable<WordEntry> words)
    {
        var catalogPairs = words
            .Select(word => word.Text.Trim().ToUpperInvariant())
            .Where(word => word.Length is >= 3 and <= 6 && word.All(char.IsLetter))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .GroupBy(GetAnagramKey)
            .Where(group => group.Count() >= 2)
            .Select(group => group.Take(2).ToArray())
            .Select(pair => (First: pair[0], Second: pair[1]));

        return catalogPairs
            .Concat(FallbackPairs)
            .DistinctBy(pair => string.Join("|", new[] { pair.First, pair.Second }.OrderBy(word => word, StringComparer.OrdinalIgnoreCase)))
            .ToArray();
    }

    private static string GetAnagramKey(string word) => new(word.OrderBy(letter => letter).ToArray());
}
