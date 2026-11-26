using LetterGame.Application.Models;
using LetterGame.Domain.Models;

namespace LetterGame.Application.Services;

public sealed class WordPuzzleSession
{
    private readonly List<int> _selectedLetterIndices = [];

    public WordEntry? CurrentWord { get; private set; }
    public IReadOnlyList<int> SelectedLetterIndices => _selectedLetterIndices;
    public bool IsSolved { get; private set; }
    public int Score { get; private set; }

    public void SetWord(WordEntry word, bool resetPuzzle)
    {
        if (resetPuzzle || CurrentWord is null || !string.Equals(CurrentWord.Text, word.Text, StringComparison.OrdinalIgnoreCase))
        {
            _selectedLetterIndices.Clear();
            IsSolved = false;
        }

        CurrentWord = word;
    }

    public bool SelectLetter(int index)
    {
        if (CurrentWord is null || IsSolved || index < 0 || index >= CurrentWord.Text.Length || _selectedLetterIndices.Contains(index))
        {
            return false;
        }

        _selectedLetterIndices.Add(index);
        return true;
    }

    public bool UndoSelection()
    {
        if (IsSolved || _selectedLetterIndices.Count == 0)
        {
            return false;
        }

        _selectedLetterIndices.RemoveAt(_selectedLetterIndices.Count - 1);
        return true;
    }

    public bool ClearSelection()
    {
        if (IsSolved || _selectedLetterIndices.Count == 0)
        {
            return false;
        }

        _selectedLetterIndices.Clear();
        return true;
    }

    public WordAnswerResult CheckAnswer()
    {
        if (CurrentWord is null || _selectedLetterIndices.Count != CurrentWord.Text.Length)
        {
            return WordAnswerResult.Incomplete;
        }

        if (IsSolved)
        {
            return WordAnswerResult.Correct;
        }

        var answer = new string(_selectedLetterIndices.Select(index => CurrentWord.Text[index]).ToArray());
        if (!string.Equals(answer, CurrentWord.Text, StringComparison.OrdinalIgnoreCase))
        {
            return WordAnswerResult.Incorrect;
        }

        IsSolved = true;
        Score++;
        return WordAnswerResult.Correct;
    }
}
