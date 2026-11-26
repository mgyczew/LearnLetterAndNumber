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
using LetterGame.Application.Services;
using LetterGame.Domain.Models;

namespace LetterGame;

public partial class MainWindow : Window
{
    private enum MathMode
    {
        Numbers,
        Addition,
        Subtraction,
        Multiplication,
        Division
    }

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
    private readonly Random _random = new();
    private readonly List<int> _selectedLetters = [];
    private readonly Dictionary<string, WordImage?> _imageCache = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<MathMode, int> _mathCorrectAnswers = [];
    private IReadOnlyList<WordEntry> _words = [];
    private WordCategory? _currentCategory;
    private int _wordIndex = -1;
    private int _score;
    private int _categoryLoadVersion;
    private int _displayVersion;
    private bool _answeredCorrectly;
    private bool _soundEnabled = true;
    private bool _mathInitialized;
    private MathMode _mathMode = MathMode.Numbers;
    private int _mathNumber = 1;
    private int _mathScore;
    private int _mathExpectedAnswer;
    private int _mathQuestionVersion;
    private string _mathSpokenQuestion = string.Empty;

    public MainWindow(WordCatalogService catalog, IInstructionReader instructionReader)
    {
        InitializeComponent();
        _catalog = catalog;
        _instructionReader = instructionReader;
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
            SetMathMode(MathMode.Numbers);
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
        if (sender is Button { Tag: string modeName } && Enum.TryParse(modeName, out MathMode mode))
        {
            SetMathMode(mode);
        }
    }

    private void SetMathMode(MathMode mode)
    {
        _mathMode = mode;
        var isNumberLesson = mode == MathMode.Numbers;
        NumberLearningPanel.Visibility = isNumberLesson ? Visibility.Visible : Visibility.Collapsed;
        MathExercisePanel.Visibility = isNumberLesson ? Visibility.Collapsed : Visibility.Visible;
        MathLessonTitle.Text = mode switch
        {
            MathMode.Numbers => "Poznaj liczby",
            MathMode.Addition => "Dodawanie",
            MathMode.Subtraction => "Odejmowanie",
            MathMode.Multiplication => "Mnoøenie",
            MathMode.Division => "Dzielenie",
            _ => "Matematyka"
        };
        MathLessonHint.Text = mode == MathMode.Numbers
            ? "Uczymy siÍ po kolei od 1 do 1000. Moøesz teø wylosowaÊ liczbÍ."
            : GetMathDifficultyHint();

        NumbersModeButton.Style = GetMathModeStyle(mode == MathMode.Numbers);
        AdditionModeButton.Style = GetMathModeStyle(mode == MathMode.Addition);
        SubtractionModeButton.Style = GetMathModeStyle(mode == MathMode.Subtraction);
        MultiplicationModeButton.Style = GetMathModeStyle(mode == MathMode.Multiplication);
        DivisionModeButton.Style = GetMathModeStyle(mode == MathMode.Division);

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
        MathNumberValue.Text = _mathNumber.ToString();
        MathNumberSpelling.Text = MathNumberWords.ToWords(_mathNumber);
        MathNumberProgress.Text = $"Liczba {_mathNumber} z 1000";
        if (readAloud && _soundEnabled)
        {
            _instructionReader.Stop();
            _instructionReader.Speak(MathNumberWords.ToWords(_mathNumber));
        }
    }

    private void PreviousNumber_Click(object sender, RoutedEventArgs e)
    {
        _mathNumber = _mathNumber == 1 ? 1000 : _mathNumber - 1;
        UpdateNumberDisplay(readAloud: true);
    }

    private void NextNumber_Click(object sender, RoutedEventArgs e)
    {
        _mathNumber = _mathNumber == 1000 ? 1 : _mathNumber + 1;
        UpdateNumberDisplay(readAloud: true);
    }

    private void RandomNumber_Click(object sender, RoutedEventArgs e)
    {
        _mathNumber = _random.Next(1, 1001);
        UpdateNumberDisplay(readAloud: true);
    }

    private void ReadNumber_Click(object sender, RoutedEventArgs e)
    {
        UpdateNumberDisplay(readAloud: true);
    }

    private string GetMathDifficultyHint()
    {
        var solved = _mathCorrectAnswers.GetValueOrDefault(_mathMode);
        return _mathMode switch
        {
            MathMode.Addition => solved < 5 ? "Zaczynamy od dodawania do 20." : solved < 20 ? "Teraz dodajemy do 100." : "SprÛbuj dodawaÊ do 1000.",
            MathMode.Subtraction => solved < 5 ? "Zaczynamy od odejmowania do 20." : solved < 20 ? "Teraz odejmujemy do 100." : "SprÛbuj odejmowaÊ do 1000.",
            MathMode.Multiplication => solved < 5 ? "Na poczπtek ≥atwe tabliczki: 2, 5 i 10." : solved < 20 ? "∆wiczymy tabliczkÍ mnoøenia do 10." : "Spokojnie Êwiczymy wyniki aø do 1000.",
            MathMode.Division => solved < 5 ? "Dzielimy ma≥e liczby po rÛwno." : solved < 20 ? "∆wiczymy proste dzielenie do 100." : "Spokojnie Êwiczymy dzielenie aø do 1000.",
            _ => string.Empty
        };
    }

    private void GenerateMathQuestion()
    {
        _mathQuestionVersion++;
        MathAnswers.IsEnabled = true;
        MathAnswers.Items.Clear();
        MathStatusText.Text = string.Empty;
        MathLessonHint.Text = GetMathDifficultyHint();

        var (first, second, answer, symbol, spokenOperator) = CreateMathProblem();
        _mathExpectedAnswer = answer;
        MathQuestionText.Text = $"{first} {symbol} {second} = ?";
        _mathSpokenQuestion = $"Ile to {MathNumberWords.ToWords(first)} {spokenOperator} {MathNumberWords.ToWords(second)}?";
        RefreshMathProgress();

        foreach (var choice in CreateMathChoices(answer).OrderBy(_ => _random.Next()))
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

    private (int First, int Second, int Answer, string Symbol, string SpokenOperator) CreateMathProblem()
    {
        var solved = _mathCorrectAnswers.GetValueOrDefault(_mathMode);
        switch (_mathMode)
        {
            case MathMode.Addition:
            {
                var limit = solved < 5 ? 20 : solved < 20 ? 100 : 1000;
                var first = _random.Next(0, limit + 1);
                var second = _random.Next(0, limit - first + 1);
                if (first == 0 && second == 0)
                {
                    first = 1;
                }

                return (first, second, first + second, "+", "plus");
            }
            case MathMode.Subtraction:
            {
                var limit = solved < 5 ? 20 : solved < 20 ? 100 : 1000;
                var first = _random.Next(1, limit + 1);
                var second = _random.Next(0, first + 1);
                return (first, second, first - second, "?", "minus");
            }
            case MathMode.Multiplication:
            {
                int first;
                int second;
                if (solved < 5)
                {
                    first = new[] { 2, 5, 10 }[_random.Next(3)];
                    second = _random.Next(1, 6);
                }
                else if (solved < 20)
                {
                    first = _random.Next(1, 11);
                    second = _random.Next(1, 11);
                }
                else
                {
                    first = _random.Next(1, 101);
                    second = _random.Next(1, 11);
                }

                return (first, second, first * second, "◊", "razy");
            }
            case MathMode.Division:
            {
                int divisor;
                int quotient;
                if (solved < 5)
                {
                    divisor = new[] { 2, 5, 10 }[_random.Next(3)];
                    quotient = _random.Next(1, 6);
                }
                else if (solved < 20)
                {
                    divisor = _random.Next(2, 11);
                    quotient = _random.Next(1, 11);
                }
                else
                {
                    divisor = _random.Next(2, 11);
                    quotient = _random.Next(1, 101);
                }

                return (divisor * quotient, divisor, quotient, "˜", "podzieliÊ przez");
            }
            default:
                return (1, 1, 2, "+", "plus");
        }
    }

    private int[] CreateMathChoices(int answer)
    {
        var choices = new HashSet<int> { answer };
        var offsets = new[] { 1, -1, 2, -2, 5, -5, 10, -10, 20, -20, 50, -50 };
        foreach (var offset in offsets.OrderBy(_ => _random.Next()))
        {
            var candidate = answer + offset;
            if (candidate is >= 0 and <= 1000)
            {
                choices.Add(candidate);
            }

            if (choices.Count == 4)
            {
                break;
            }
        }

        while (choices.Count < 4)
        {
            choices.Add(_random.Next(0, 1001));
        }

        return choices.ToArray();
    }

    private void RefreshMathProgress()
    {
        var solved = _mathCorrectAnswers.GetValueOrDefault(_mathMode);
        MathProgressText.Text = $"Dobrze rozwiπzane: {solved}";
    }

    private void MathAnswer_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: int answer } || answer != _mathExpectedAnswer)
        {
            MathStatusText.Text = "SprÛbuj jeszcze raz! Dasz radÍ.";
            MathStatusText.Foreground = new SolidColorBrush(Color.FromRgb(204, 139, 43));
            return;
        }

        _mathScore++;
        MathScoreText.Text = _mathScore.ToString();
        _mathCorrectAnswers[_mathMode] = _mathCorrectAnswers.GetValueOrDefault(_mathMode) + 1;
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
        _selectedLetters.Clear();
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
        if (resetPuzzle)
        {
            _selectedLetters.Clear();
            _answeredCorrectly = false;
        }

        if (_currentCategory is null || _wordIndex < 0 || _wordIndex >= _words.Count)
        {
            return;
        }

        var word = _words[_wordIndex];
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
        CheckButton.IsEnabled = !_answeredCorrectly;
        CheckButton.Opacity = _answeredCorrectly ? 0.55 : 1;
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
        if (_words.Count == 0)
        {
            return;
        }

        var word = _words[_wordIndex].Text;
        AnswerPanel.Items.Clear();
        LetterBank.Items.Clear();

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
                    Text = index < _selectedLetters.Count ? word[_selectedLetters[index]].ToString() : string.Empty,
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
            var selected = _selectedLetters.Contains(letterIndex);
            var letterButton = new Button
            {
                Content = word[letterIndex].ToString(),
                Style = (Style)FindResource("LetterTile"),
                IsEnabled = !selected && !_answeredCorrectly,
                Opacity = selected ? 0.35 : 1,
                Tag = letterIndex
            };
            letterButton.Click += Letter_Click;
            LetterBank.Items.Add(letterButton);
        }
    }

    private void Letter_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: int letterIndex } && !_answeredCorrectly)
        {
            _selectedLetters.Add(letterIndex);
            ClearStatus();
            RefreshLetters();
        }
    }

    private void Undo_Click(object sender, RoutedEventArgs e)
    {
        if (_selectedLetters.Count > 0 && !_answeredCorrectly)
        {
            _selectedLetters.RemoveAt(_selectedLetters.Count - 1);
            ClearStatus();
            RefreshLetters();
        }
    }

    private void Clear_Click(object sender, RoutedEventArgs e)
    {
        if (!_answeredCorrectly)
        {
            _selectedLetters.Clear();
            ClearStatus();
            RefreshLetters();
        }
    }

    private void Check_Click(object sender, RoutedEventArgs e)
    {
        if (_words.Count == 0)
        {
            return;
        }

        var word = _words[_wordIndex].Text;
        if (_selectedLetters.Count != word.Length)
        {
            SetStatus("U≥Ûø wszystkie litery i sprÛbuj ponownie.", Color.FromRgb(204, 139, 43));
            return;
        }

        var answer = new string(_selectedLetters.Select(index => word[index]).ToArray());
        if (string.Equals(answer, word, StringComparison.OrdinalIgnoreCase))
        {
            _answeredCorrectly = true;
            _score++;
            ScoreText.Text = _score.ToString();
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
        if (completedWordVersion == _displayVersion && _answeredCorrectly)
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
