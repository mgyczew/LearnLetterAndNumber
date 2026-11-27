using LetterGame.Application.Models;
using LetterGame.Domain.Services;

namespace LetterGame.Application.Services;

/// <summary>Obs³uguje tryby liczenia obiektów, rozpoznawania cyfr i budowania liczby.</summary>
public sealed class NumberGameSession(Random? random = null)
{
    private readonly Random _random = random ?? new Random();
    private NumberGameMode _mode;
    private int _target;
    private string _builtNumber = string.Empty;
    private string _prompt = string.Empty;
    private string _spokenPrompt = string.Empty;
    private int[] _choices = [];
    private int? _quantity;
    private int _score;
    private bool _isComplete;

    public NumberGameState State => new(
        _mode,
        _prompt,
        _spokenPrompt,
        _choices.ToArray(),
        _quantity,
        _builtNumber,
        _score,
        _isComplete);

    public void Start(NumberGameMode mode)
    {
        _mode = mode;
        _score = 0;
        CreateQuestion();
    }

    public void NextQuestion()
    {
        if (_isComplete)
        {
            CreateQuestion();
        }
    }

    public NumberGameAnswerResult SubmitChoice(int value)
    {
        if (_isComplete || _mode == NumberGameMode.Build || !_choices.Contains(value))
        {
            return NumberGameAnswerResult.Incorrect;
        }

        return SubmitAnswer(value == _target);
    }

    public bool AppendDigit(int digit)
    {
        if (_mode != NumberGameMode.Build
            || _isComplete
            || digit is < 0 or > 9
            || _builtNumber.Length >= 2)
        {
            return false;
        }

        _builtNumber += digit.ToString();
        return true;
    }

    public NumberGameAnswerResult SubmitBuiltNumber()
    {
        if (_mode != NumberGameMode.Build || _isComplete)
        {
            return NumberGameAnswerResult.Incorrect;
        }

        if (_builtNumber.Length != 2)
        {
            return NumberGameAnswerResult.Incomplete;
        }

        return SubmitAnswer(int.TryParse(_builtNumber, out var answer) && answer == _target);
    }

    public void ClearBuiltNumber()
    {
        if (_mode == NumberGameMode.Build && !_isComplete)
        {
            _builtNumber = string.Empty;
        }
    }

    private NumberGameAnswerResult SubmitAnswer(bool isCorrect)
    {
        if (!isCorrect)
        {
            return NumberGameAnswerResult.Incorrect;
        }

        _score++;
        _isComplete = true;
        return NumberGameAnswerResult.Correct;
    }

    private void CreateQuestion()
    {
        _target = 0;
        _builtNumber = string.Empty;
        _quantity = null;
        _isComplete = false;

        switch (_mode)
        {
            case NumberGameMode.Count:
                _target = _random.Next(1, 11);
                _quantity = _target;
                _prompt = "Ile jest kropek?";
                _spokenPrompt = _prompt;
                var answers = new HashSet<int> { _target };
                while (answers.Count < 4)
                {
                    answers.Add(Math.Clamp(_target + _random.Next(-3, 4), 1, 10));
                }
                _choices = answers.OrderBy(_ => _random.Next()).ToArray();
                break;

            case NumberGameMode.Find:
                _target = _random.Next(0, 10);
                _prompt = "Pos³uchaj i wska¿ cyfrê.";
                _spokenPrompt = PolishNumberWords.ToWords(_target);
                _choices = Enumerable.Range(0, 10).OrderBy(_ => _random.Next()).ToArray();
                break;

            case NumberGameMode.Build:
                _target = _random.Next(10, 100);
                _prompt = $"U³ó¿ liczbê: {PolishNumberWords.ToWords(_target)}";
                _spokenPrompt = $"U³ó¿ liczbê {PolishNumberWords.ToWords(_target)}";
                _choices = Enumerable.Range(0, 10).OrderBy(_ => _random.Next()).ToArray();
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(_mode));
        }
    }
}
