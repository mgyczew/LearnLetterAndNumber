using LetterGame.Domain.Models;
using LetterGame.Domain.Services;

namespace LetterGame.Application.Services;

/// <summary>Koordynuje lekcjê liczb i dzia³añ oraz zapamiêtuje wyniki ucznia.</summary>
public sealed class MathLearningService(MathQuestionGenerator questionGenerator)
{
    private readonly Dictionary<MathLesson, int> _correctAnswers = [];
    private MathQuestion? _currentQuestion;

    public MathLesson SelectedLesson { get; private set; } = MathLesson.Numbers;
    public int CurrentNumber { get; private set; } = 1;
    public int Score { get; private set; }
    public int CorrectAnswersInSelectedLesson => _correctAnswers.GetValueOrDefault(SelectedLesson);
    public string CurrentNumberWords => PolishNumberWords.ToWords(CurrentNumber);

    public void SelectLesson(MathLesson lesson)
    {
        SelectedLesson = lesson;
        _currentQuestion = null;
    }

    public void MoveNumber(int offset)
    {
        CurrentNumber = ((CurrentNumber - 1 + offset) % 1000 + 1000) % 1000 + 1;
    }

    public void ChooseRandomNumber()
    {
        CurrentNumber = Random.Shared.Next(1, 1001);
    }

    public string GetDifficultyHint() => SelectedLesson switch
    {
        MathLesson.Addition => CorrectAnswersInSelectedLesson < 5 ? "Zaczynamy od dodawania do 20." : CorrectAnswersInSelectedLesson < 20 ? "Teraz dodajemy do 100." : "Spróbuj dodawaæ do 1000.",
        MathLesson.Subtraction => CorrectAnswersInSelectedLesson < 5 ? "Zaczynamy od odejmowania do 20." : CorrectAnswersInSelectedLesson < 20 ? "Teraz odejmujemy do 100." : "Spróbuj odejmowaæ do 1000.",
        MathLesson.Multiplication => CorrectAnswersInSelectedLesson < 5 ? "Na pocz¹tek ³atwe tabliczki: 2, 5 i 10." : CorrectAnswersInSelectedLesson < 20 ? "Æwiczymy tabliczkê mno¿enia do 10." : "Spokojnie æwiczymy wyniki a¿ do 1000.",
        MathLesson.Division => CorrectAnswersInSelectedLesson < 5 ? "Dzielimy ma³e liczby po równo." : CorrectAnswersInSelectedLesson < 20 ? "Æwiczymy proste dzielenie do 100." : "Spokojnie æwiczymy dzielenie a¿ do 1000.",
        _ => "Uczymy siê po kolei od 1 do 1000. Mo¿esz te¿ wylosowaæ liczbê."
    };

    public MathQuestion CreateQuestion()
    {
        _currentQuestion = questionGenerator.Create(SelectedLesson, CorrectAnswersInSelectedLesson);
        return _currentQuestion;
    }

    public bool SubmitAnswer(int answer)
    {
        if (_currentQuestion is null || answer != _currentQuestion.CorrectAnswer)
        {
            return false;
        }

        Score++;
        _correctAnswers[SelectedLesson] = CorrectAnswersInSelectedLesson + 1;
        _currentQuestion = null;
        return true;
    }
}
