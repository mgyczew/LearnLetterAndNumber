using LetterGame.Application.Models;
using LetterGame.Domain.Models;

namespace LetterGame.Application.Services;

public sealed class PictureMatchGameSession(Random? random = null)
{
    private readonly Random _random = random ?? new Random();
    private readonly List<Puzzle> _puzzles = [];
    private int _score;

    public PictureMatchGameState State => new(
        _score,
        _puzzles.Select(puzzle => new PictureMatchPuzzleState(
            puzzle.Word,
            puzzle.SelectedLetterIndices.ToArray(),
            puzzle.LetterBankIndices.ToArray(),
            puzzle.IsSolved)).ToArray());

    public bool Start(IEnumerable<WordEntry> words, bool resetScore)
    {
        if (resetScore)
        {
            _score = 0;
        }

        var selectedWords = words
            .Where(word => word.Text.Length is >= 3 and <= 9 && word.Text.All(char.IsLetter))
            .DistinctBy(word => word.Text, StringComparer.OrdinalIgnoreCase)
            .OrderBy(_ => _random.Next())
            .Take(3)
            .ToArray();

        _puzzles.Clear();
        _puzzles.AddRange(selectedWords.Select(word => new Puzzle(
            word,
            Enumerable.Range(0, word.Text.Length).OrderBy(_ => _random.Next()).ToArray())));
        return _puzzles.Count > 0;
    }

    public bool SelectLetter(string wordText, int index)
    {
        var puzzle = FindPuzzle(wordText);
        if (puzzle is null
            || puzzle.IsSolved
            || index < 0
            || index >= puzzle.Word.Text.Length
            || puzzle.SelectedLetterIndices.Contains(index))
        {
            return false;
        }

        puzzle.SelectedLetterIndices.Add(index);
        return true;
    }

    public bool ClearSelection(string wordText)
    {
        var puzzle = FindPuzzle(wordText);
        if (puzzle is null || puzzle.IsSolved || puzzle.SelectedLetterIndices.Count == 0)
        {
            return false;
        }

        puzzle.SelectedLetterIndices.Clear();
        return true;
    }

    public PictureMatchAnswerResult SubmitAnswer(string wordText)
    {
        var puzzle = FindPuzzle(wordText);
        if (puzzle is null || puzzle.IsSolved)
        {
            return PictureMatchAnswerResult.AlreadySolved;
        }

        if (puzzle.SelectedLetterIndices.Count != puzzle.Word.Text.Length)
        {
            return PictureMatchAnswerResult.Incomplete;
        }

        var answer = new string(puzzle.SelectedLetterIndices.Select(index => puzzle.Word.Text[index]).ToArray());
        if (!string.Equals(answer, puzzle.Word.Text, StringComparison.OrdinalIgnoreCase))
        {
            return PictureMatchAnswerResult.Incorrect;
        }

        puzzle.IsSolved = true;
        _score++;
        return PictureMatchAnswerResult.Correct;
    }

    public PictureMatchPuzzleState? GetPuzzleState(string wordText)
    {
        var puzzle = FindPuzzle(wordText);
        return puzzle is null
            ? null
            : new PictureMatchPuzzleState(
                puzzle.Word,
                puzzle.SelectedLetterIndices.ToArray(),
                puzzle.LetterBankIndices.ToArray(),
                puzzle.IsSolved);
    }

    private Puzzle? FindPuzzle(string wordText) => _puzzles.FirstOrDefault(puzzle =>
        string.Equals(puzzle.Word.Text, wordText, StringComparison.OrdinalIgnoreCase));

    private sealed class Puzzle(WordEntry word, int[] letterBankIndices)
    {
        public WordEntry Word { get; } = word;
        public int[] LetterBankIndices { get; } = letterBankIndices;
        public List<int> SelectedLetterIndices { get; } = [];
        public bool IsSolved { get; set; }
    }
}
