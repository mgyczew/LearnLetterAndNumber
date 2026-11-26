using System.Diagnostics;
using System.Net.Http;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using LetterGame.Application.Contracts;
using LetterGame.Application.Models;
using LetterGame.Application.Services;
using LetterGame.Domain.Models;
using LetterGame.Domain.Services;

namespace LetterGame;

public partial class MainWindow : Window
{
    private static readonly IReadOnlyDictionary<string, Color> ColorSwatches = new Dictionary<string, Color>(StringComparer.OrdinalIgnoreCase)
    {
        ["BIA£Y"] = Colors.White,
        ["CZARNY"] = Colors.Black,
        ["CZERWONY"] = Color.FromRgb(229, 57, 53),
        ["NIEBIESKI"] = Color.FromRgb(30, 136, 229),
        ["ZIELONY"] = Color.FromRgb(67, 160, 71),
        ["Ø”£TY"] = Color.FromRgb(253, 216, 53),
        ["POMARA—CZOWY"] = Color.FromRgb(251, 140, 0),
        ["FIOLETOWY"] = Color.FromRgb(142, 36, 170),
        ["R”ØOWY"] = Color.FromRgb(236, 64, 122),
        ["BR•ZOWY"] = Color.FromRgb(121, 85, 72),
        ["SZARY"] = Color.FromRgb(158, 158, 158),
        ["GRANATOWY"] = Color.FromRgb(26, 35, 126),
        ["TURKUSOWY"] = Color.FromRgb(38, 166, 154),
        ["Z£OTY"] = Color.FromRgb(212, 175, 55),
        ["SREBRNY"] = Color.FromRgb(192, 192, 192),
        ["BEØOWY"] = Color.FromRgb(215, 204, 200),
        ["KREMOWY"] = Color.FromRgb(255, 253, 208),
        ["B£ KITNY"] = Color.FromRgb(144, 202, 249),
        ["RUDY"] = Color.FromRgb(176, 74, 36),
        ["MI TOWY"] = Color.FromRgb(165, 214, 167)
    };

    private readonly WordCatalogService _catalog;
    private readonly IInstructionReader _instructionReader;
    private readonly MathLearningService _mathLearning;
    private readonly WordPuzzleSession _wordPuzzle;
    private readonly Random _random = new();
    private readonly Dictionary<string, WordImage?> _imageCache = new(StringComparer.OrdinalIgnoreCase);
    private IReadOnlyList<WordEntry> _words = [];
    private WordCategory? _currentCategory;
    private int _wordIndex = -1;
    private int _categoryLoadVersion;
    private int _displayVersion;
    private bool _soundEnabled = true;
    private bool _mathInitialized;
    private int _mathQuestionVersion;
    private string _mathSpokenQuestion = string.Empty;

    public MainWindow(
        WordCatalogService catalog,
        IInstructionReader instructionReader,
        MathLearningService mathLearning,
        WordPuzzleSession wordPuzzle)
    {
        InitializeComponent();
        _catalog = catalog;
        _instructionReader = instructionReader;
        _mathLearning = mathLearning;
        _wordPuzzle = wordPuzzle;
        CategorySelector.ItemsSource = _catalog.Categories;
        CategorySelector.SelectedIndex = 0;
    }

    private void OpenMath_Click(object sender, RoutedEventArgs e)
    {
        _displayVersion++;
        _instructionReader.Stop();
        WordZone.Visibility = Visibility.Collapsed;
        MathZone.Visibility = Visibility.Visible;

        if (!_mathInitialized)
        {
            _mathInitialized = true;
            SetMathMode(MathLesson.Numbers);
        }
        else if (MathExercisePanel.Visibility == Visibility.Visible && !MathAnswers.IsEnabled)
        {
            GenerateMathQuestion();
        }
    }

    private void OpenWords_Click(object sender, RoutedEventArgs e)
    {
        _mathQuestionVersion++;
        _instructionReader.Stop();
        MathZone.Visibility = Visibility.Collapsed;
        WordZone.Visibility = Visibility.Visible;
    }

    private void MathMode_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: string modeName } && Enum.TryParse(modeName, out MathLesson mode))
        {
            SetMathMode(mode);
        }
    }

    private void SetMathMode(MathLesson mode)
    {
        _mathQuestionVersion++;
        _mathLearning.SelectLesson(mode);
        var isNumberLesson = mode == MathLesson.Numbers;
        NumberLearningPanel.Visibility = isNumberLesson ? Visibility.Visible : Visibility.Collapsed;
        MathExercisePanel.Visibility = isNumberLesson ? Visibility.Collapsed : Visibility.Visible;
        MathLessonTitle.Text = mode switch
        {
            MathLesson.Numbers => "Poznaj liczby",
            MathLesson.Addition => "Dodawanie",
            MathLesson.Subtraction => "Odejmowanie",
            MathLesson.Multiplication => "Mnoøenie",
            MathLesson.Division => "Dzielenie",
            _ => "Matematyka"
        };
        MathLessonHint.Text = _mathLearning.GetDifficultyHint();

        NumbersModeButton.Style = GetMathModeStyle(mode == MathLesson.Numbers);
        AdditionModeButton.Style = GetMathModeStyle(mode == MathLesson.Addition);
        SubtractionModeButton.Style = GetMathModeStyle(mode == MathLesson.Subtraction);
        MultiplicationModeButton.Style = GetMathModeStyle(mode == MathLesson.Multiplication);
        DivisionModeButton.Style = GetMathModeStyle(mode == MathLesson.Division);

        if (isNumberLesson)
        {
            UpdateNumberDisplay(readAloud: _soundEnabled);
        }
        else
        {
            GenerateMathQuestion();
        }
    }

    private Style GetMathModeStyle(bool selected) =>
        (Style)FindResource(selected ? "PrimaryButton" : "SecondaryButton");

    private void UpdateNumberDisplay(bool readAloud)
    {
        MathNumberValue.Text = _mathLearning.CurrentNumber.ToString();
        MathNumberSpelling.Text = _mathLearning.CurrentNumberWords;
        MathNumberProgress.Text = $"Liczba {_mathLearning.CurrentNumber} z 1000";
        if (readAloud && _soundEnabled)
        {
            _instructionReader.Stop();
            _instructionReader.Speak(_mathLearning.CurrentNumberWords);
        }
    }

    private void PreviousNumber_Click(object sender, RoutedEventArgs e)
    {
        _mathLearning.MoveNumber(-1);
        UpdateNumberDisplay(readAloud: true);
    }

    private void NextNumber_Click(object sender, RoutedEventArgs e)
    {
        _mathLearning.MoveNumber(1);
        UpdateNumberDisplay(readAloud: true);
    }

    private void RandomNumber_Click(object sender, RoutedEventArgs e)
    {
        _mathLearning.ChooseRandomNumber();
        UpdateNumberDisplay(readAloud: true);
    }

    private void ReadNumber_Click(object sender, RoutedEventArgs e)
    {
        UpdateNumberDisplay(readAloud: true);
    }

    private void GenerateMathQuestion()
    {
        _mathQuestionVersion++;
        MathAnswers.IsEnabled = true;
        MathAnswers.Items.Clear();
        MathStatusText.Text = string.Empty;

        var question = _mathLearning.CreateQuestion();
        var (symbol, spokenOperator) = GetMathOperators(question.Lesson);
        MathQuestionText.Text = $"{question.FirstOperand} {symbol} {question.SecondOperand} = ?";
        _mathSpokenQuestion = $"Ile to {PolishNumberWords.ToWords(question.FirstOperand)} {spokenOperator} {PolishNumberWords.ToWords(question.SecondOperand)}?";
        MathLessonHint.Text = _mathLearning.GetDifficultyHint();
        RefreshMathProgress();

        foreach (var choice in question.Choices)
        {
            var answerButton = new Button
            {
                Content = choice.ToString(),
                Tag = choice,
                MinWidth = 120,
                Height = 76,
                Margin = new Thickness(8),
                FontSize = 30,
                Style = (Style)FindResource("PrimaryButton")
            };
            answerButton.Click += MathAnswer_Click;
            MathAnswers.Items.Add(answerButton);
        }

        if (_soundEnabled)
        {
            _instructionReader.Stop();
            _instructionReader.Speak(_mathSpokenQuestion);
        }
    }

    private static (string Symbol, string SpokenOperator) GetMathOperators(MathLesson lesson)
    {
        return lesson switch
        {
            MathLesson.Addition => ("+", "plus"),
            MathLesson.Subtraction => ("?", "minus"),
            MathLesson.Multiplication => ("◊", "razy"),
            MathLesson.Division => ("˜", "podzieliÊ przez"),
            _ => throw new ArgumentOutOfRangeException(nameof(lesson))
        };
    }

    private void RefreshMathProgress()
    {
        MathProgressText.Text = $"Dobrze rozwiπzane: {_mathLearning.CorrectAnswersInSelectedLesson}";
    }

    private void MathAnswer_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: int answer } || !_mathLearning.SubmitAnswer(answer))
        {
            MathStatusText.Text = "SprÛbuj jeszcze raz! Dasz radÍ.";
            MathStatusText.Foreground = new SolidColorBrush(Color.FromRgb(204, 139, 43));
            return;
        }

        MathScoreText.Text = _mathLearning.Score.ToString();
        MathAnswers.IsEnabled = false;
        MathStatusText.Text = "Brawo! åwietnie policzone!";
        MathStatusText.Foreground = new SolidColorBrush(Color.FromRgb(38, 148, 105));
        RefreshMathProgress();
        if (_soundEnabled)
        {
            _instructionReader.Stop();
            _instructionReader.Speak("Brawo! åwietnie policzone!");
        }

        _ = AdvanceMathQuestionAsync(_mathQuestionVersion);
    }

    private void ReadMathQuestion_Click(object sender, RoutedEventArgs e)
    {
        if (_soundEnabled)
        {
            _instructionReader.Stop();
            _instructionReader.Speak(_mathSpokenQuestion);
        }
    }

    private async Task AdvanceMathQuestionAsync(int completedQuestionVersion)
    {
        await Task.Delay(TimeSpan.FromSeconds(3));
        if (completedQuestionVersion == _mathQuestionVersion && MathZone.Visibility == Visibility.Visible)
        {
            GenerateMathQuestion();
        }
    }

    private async void Category_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (CategorySelector.SelectedItem is not WordCategory category)
        {
            return;
        }

        var loadVersion = ++_categoryLoadVersion;
        _currentCategory = category;
        _words = category.OfflineWords;
        _wordIndex = -1;
        ShowNextWord();
        SetStatus("Pobieram wiÍkszy zestaw s≥Ûw z darmowego APIÖ", Colors.DarkSlateBlue);

        try
        {
            var result = await _catalog.GetWordsAsync(category);
            if (loadVersion != _categoryLoadVersion)
            {
                return;
            }

            var currentWord = _wordIndex >= 0 ? _words[_wordIndex].Text : null;
            _words = result.Words;
            var preservedIndex = currentWord is null
                ? -1
                : _words.ToList().FindIndex(word => string.Equals(word.Text, currentWord, StringComparison.OrdinalIgnoreCase));
            if (preservedIndex >= 0)
            {
                _wordIndex = preservedIndex;
                ShowCurrentWord(readInstruction: false, resetPuzzle: false);
            }
            else
            {
                _wordIndex = -1;
                ShowNextWord();
            }
            SetStatus(result.ApiImageCount > 0
                ? $"Obrazki z Wikipedii: {result.ApiImageCount}  ∑  W zestawie: {_words.Count} s≥Ûw"
                : $"W zestawie: {_words.Count} prostych s≥Ûw",
                result.ApiImageCount > 0 ? Color.FromRgb(38, 148, 105) : Color.FromRgb(104, 115, 138));
        }
        catch (OperationCanceledException)
        {
        }
    }

    private void ShowNextWord()
    {
        if (_words.Count == 0 || _currentCategory is null)
        {
            return;
        }

        _instructionReader.Stop();
        _wordIndex = (_wordIndex + 1) % _words.Count;
        ShowCurrentWord(readInstruction: true, resetPuzzle: true);
    }

    private void ShowCurrentWord(bool readInstruction, bool resetPuzzle)
    {
        var displayVersion = ++_displayVersion;
        if (_currentCategory is null || _wordIndex < 0 || _wordIndex >= _words.Count)
        {
            return;
        }

        var word = _words[_wordIndex];
        _wordPuzzle.SetWord(word, resetPuzzle);
        CategoryLabel.Text = _currentCategory.Name;
        ClueText.Text = word.Clue;
        PictureImage.Source = null;
        PictureImage.Visibility = Visibility.Collapsed;
        ImageCreditText.Inlines.Clear();
        var hasColorSwatch = _currentCategory.Name == "Kolory" && ColorSwatches.TryGetValue(word.Text, out var swatchColor);
        if (hasColorSwatch)
        {
            ColorSwatch.Background = new SolidColorBrush(swatchColor);
        }

        ColorSwatch.Visibility = hasColorSwatch ? Visibility.Visible : Visibility.Collapsed;
        PictureText.Text = word.Emoji;
        PictureText.Visibility = hasColorSwatch ? Visibility.Collapsed : Visibility.Visible;
        CheckButton.IsEnabled = !_wordPuzzle.IsSolved;
        CheckButton.Opacity = _wordPuzzle.IsSolved ? 0.55 : 1;
        RefreshLetters();
        if (!hasColorSwatch)
        {
            _ = LoadImageAsync(word, displayVersion);
        }
        if (readInstruction && _soundEnabled)
        {
            _instructionReader.Speak($"U≥Ûø s≥owo z kategorii {_currentCategory.Name}. Wybierz litery w dobrej kolejnoúci.");
        }
    }

    private async Task LoadImageAsync(WordEntry word, int displayVersion)
    {
        if (_imageCache.TryGetValue(word.Text, out var cachedImage))
        {
            if (cachedImage is not null && displayVersion == _displayVersion)
            {
                PictureImage.Source = new BitmapImage(new Uri(cachedImage.ImageUrl, UriKind.Absolute));
                PictureImage.Visibility = Visibility.Visible;
                PictureText.Visibility = Visibility.Collapsed;
                SetImageCredit(cachedImage.SourceUrl);
            }

            return;
        }

        try
        {
            var image = await _catalog.GetImageAsync(word);
            if (displayVersion != _displayVersion)
            {
                return;
            }

            if (image is null)
            {
                _imageCache[word.Text] = null;
                return;
            }

            _imageCache[word.Text] = image;
            PictureImage.Source = new BitmapImage(new Uri(image.ImageUrl, UriKind.Absolute));
            PictureImage.Visibility = Visibility.Visible;
            PictureText.Visibility = Visibility.Collapsed;
            SetImageCredit(image.SourceUrl);
        }
        catch (Exception exception) when (exception is HttpRequestException or TaskCanceledException or JsonException or UriFormatException)
        {
            _imageCache[word.Text] = null;
        }
    }

    private void SetImageCredit(string sourceUrl)
    {
        ImageCreditText.Inlines.Clear();
        var sourceUri = new Uri(sourceUrl, UriKind.Absolute);
        var sourceName = sourceUri.Host is "flagcdn.com" or "www.flagcdn.com"
            ? "FlagCDN"
            : "Wikimedia Commons / Wikipedia";
        ImageCreditText.Inlines.Add(new Run("èrÛd≥o obrazka: "));
        var link = new Hyperlink(new Run(sourceName))
        {
            NavigateUri = sourceUri
        };
        link.RequestNavigate += SourceLink_RequestNavigate;
        ImageCreditText.Inlines.Add(link);
    }

    private void RefreshLetters()
    {
        AnswerPanel.Items.Clear();
        LetterBank.Items.Clear();
        if (_wordPuzzle.CurrentWord is not { } currentWord)
        {
            return;
        }

        var word = currentWord.Text;

        for (var index = 0; index < word.Length; index++)
        {
            var letterBox = new Border
            {
                Width = 66,
                Height = 70,
                Margin = new Thickness(6),
                CornerRadius = new CornerRadius(14),
                BorderThickness = new Thickness(2),
                BorderBrush = new SolidColorBrush(Color.FromRgb(225, 230, 241)),
                Background = new SolidColorBrush(Color.FromRgb(248, 249, 253)),
                Child = new TextBlock
                {
                    Text = index < _wordPuzzle.SelectedLetterIndices.Count
                        ? word[_wordPuzzle.SelectedLetterIndices[index]].ToString()
                        : string.Empty,
                    FontSize = 29,
                    FontWeight = FontWeights.Bold,
                    Foreground = new SolidColorBrush(Color.FromRgb(39, 49, 78)),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                }
            };
            AnswerPanel.Items.Add(letterBox);
        }

        foreach (var letterIndex in Enumerable.Range(0, word.Length).OrderBy(_ => _random.Next()))
        {
            var selected = _wordPuzzle.SelectedLetterIndices.Contains(letterIndex);
            var letterButton = new Button
            {
                Content = word[letterIndex].ToString(),
                Style = (Style)FindResource("LetterTile"),
                IsEnabled = !selected && !_wordPuzzle.IsSolved,
                Opacity = selected ? 0.35 : 1,
                Tag = letterIndex
            };
            letterButton.Click += Letter_Click;
            LetterBank.Items.Add(letterButton);
        }
    }

    private void Letter_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: int letterIndex } && _wordPuzzle.SelectLetter(letterIndex))
        {
            ClearStatus();
            RefreshLetters();
        }
    }

    private void Undo_Click(object sender, RoutedEventArgs e)
    {
        if (_wordPuzzle.UndoSelection())
        {
            ClearStatus();
            RefreshLetters();
        }
    }

    private void Clear_Click(object sender, RoutedEventArgs e)
    {
        if (_wordPuzzle.ClearSelection())
        {
            ClearStatus();
            RefreshLetters();
        }
    }

    private void Check_Click(object sender, RoutedEventArgs e)
    {
        if (_wordPuzzle.CurrentWord is null)
        {
            return;
        }

        var result = _wordPuzzle.CheckAnswer();
        if (result == WordAnswerResult.Incomplete)
        {
            SetStatus("U≥Ûø wszystkie litery i sprÛbuj ponownie.", Color.FromRgb(204, 139, 43));
            return;
        }

        if (result == WordAnswerResult.Correct)
        {
            ScoreText.Text = _wordPuzzle.Score.ToString();
            SetStatus("Brawo! åwietnie u≥oøone s≥owo!", Color.FromRgb(38, 148, 105));
            CheckButton.IsEnabled = false;
            RefreshLetters();
            if (_soundEnabled)
            {
                _instructionReader.Stop();
                _instructionReader.Speak("Brawo! Dobrze u≥oøone s≥owo.");
            }
            _ = AdvanceAfterCorrectAnswerAsync(_displayVersion);
        }
        else
        {
            SetStatus("Jeszcze raz ó sprÛbuj zmieniÊ kolejnoúÊ liter.", Color.FromRgb(207, 87, 100));
        }
    }

    private void Next_Click(object sender, RoutedEventArgs e)
    {
        ShowNextWord();
        ClearStatus();
    }

    private async Task AdvanceAfterCorrectAnswerAsync(int completedWordVersion)
    {
        await Task.Delay(TimeSpan.FromSeconds(3));
        if (completedWordVersion == _displayVersion && _wordPuzzle.IsSolved)
        {
            ShowNextWord();
        }
    }

    private void SetStatus(string message, Color color)
    {
        StatusText.Text = message;
        StatusText.Foreground = new SolidColorBrush(color);
    }

    private void ClearStatus()
    {
        StatusText.Text = string.Empty;
    }

    private void PictureImage_Failed(object sender, ExceptionRoutedEventArgs e)
    {
        if (_currentCategory?.Name == "Kolory"
            && _wordIndex >= 0
            && _wordIndex < _words.Count
            && ColorSwatches.TryGetValue(_words[_wordIndex].Text, out var swatchColor))
        {
            ColorSwatch.Background = new SolidColorBrush(swatchColor);
            ColorSwatch.Visibility = Visibility.Visible;
            PictureText.Visibility = Visibility.Collapsed;
            return;
        }

        PictureImage.Visibility = Visibility.Collapsed;
        PictureText.Visibility = Visibility.Visible;
        ImageCreditText.Inlines.Clear();
        if (_wordIndex >= 0 && _wordIndex < _words.Count)
        {
            _imageCache[_words[_wordIndex].Text] = null;
        }
    }

    private void SourceLink_RequestNavigate(object sender, RequestNavigateEventArgs e)
    {
        if (e.Uri.Host is "pl.wikipedia.org" or "commons.wikimedia.org" or "flagcdn.com" or "www.flagcdn.com")
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
            e.Handled = true;
        }
    }

    private void ToggleFullScreen_Click(object sender, RoutedEventArgs e)
    {
        ToggleFullScreen();
    }

    private void ToggleSound_Click(object sender, RoutedEventArgs e)
    {
        _soundEnabled = !_soundEnabled;
        SoundButton.Content = _soundEnabled ? "DüwiÍk: w≥πczony" : "DüwiÍk: wy≥πczony";
        MathSoundButton.Content = SoundButton.Content;
        ListenButton.IsEnabled = _soundEnabled;
        MathReadNumberButton.IsEnabled = _soundEnabled;
        MathReadQuestionButton.IsEnabled = _soundEnabled;
        if (!_soundEnabled)
        {
            _instructionReader.Stop();
        }
    }

    private void ListenWord_Click(object sender, RoutedEventArgs e)
    {
        if (_soundEnabled && _wordIndex >= 0 && _wordIndex < _words.Count)
        {
            _instructionReader.Stop();
            _instructionReader.Speak($"S≥owo to: {_words[_wordIndex].Text.ToLowerInvariant()}");
        }
    }

    private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.F11)
        {
            ToggleFullScreen();
            e.Handled = true;
        }
        else if (e.Key == Key.Escape && WindowStyle == WindowStyle.None)
        {
            WindowStyle = WindowStyle.SingleBorderWindow;
            ResizeMode = ResizeMode.CanResize;
            WindowState = WindowState.Normal;
            Width = 1200;
            Height = 850;
            Left = (SystemParameters.WorkArea.Width - Width) / 2;
            Top = (SystemParameters.WorkArea.Height - Height) / 2;
            e.Handled = true;
        }
    }

    private void ToggleFullScreen()
    {
        if (WindowStyle == WindowStyle.None)
        {
            WindowStyle = WindowStyle.SingleBorderWindow;
            ResizeMode = ResizeMode.CanResize;
            WindowState = WindowState.Normal;
            Width = 1200;
            Height = 850;
            Left = (SystemParameters.WorkArea.Width - Width) / 2;
            Top = (SystemParameters.WorkArea.Height - Height) / 2;
        }
        else
        {
            WindowStyle = WindowStyle.None;
            ResizeMode = ResizeMode.NoResize;
            WindowState = WindowState.Maximized;
        }
    }

    private void Exit_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
