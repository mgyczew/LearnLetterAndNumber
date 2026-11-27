using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using LetterGame.Application.Contracts;
using LetterGame.Application.Models;
using LetterGame.Application.Services;
using LetterGame.Domain.Models;
using LetterGame.Domain.Services;

namespace LetterGame;

/// <summary>Prezentuje ekrany aplikacji i przekazuje akcje uøytkownika do sesji Application.</summary>
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
    private readonly NumberQuizService _numberQuiz;
    private readonly WordChainGameService _wordChainGame;
    private readonly NumberGameSession _numberGame;
    private readonly SnakeGameSession _snakeGame;
    private readonly PictureMatchGameSession _pictureMatchGame;
    private readonly Random _random = new();
    private readonly Dictionary<string, WordImage?> _imageCache = new(StringComparer.OrdinalIgnoreCase);
    private CancellationTokenSource? _categoryLoadCancellation;
    private CancellationTokenSource? _imageLoadCancellation;
    private IReadOnlyList<WordEntry> _words = [];
    private WordCategory? _currentCategory;
    private int _wordIndex = -1;
    private int _categoryLoadVersion;
    private int _displayVersion;
    private bool _soundEnabled = true;
    private bool _mathInitialized;
    private int _mathQuestionVersion;
    private MathQuestionViewModel? _currentMathQuestion;
    private NumberQuizQuestion? _currentNumberQuizQuestion;
    private int _numberQuizScore;
    public MainWindow(
        WordCatalogService catalog,
        IInstructionReader instructionReader,
        MathLearningService mathLearning,
        WordPuzzleSession wordPuzzle,
        NumberQuizService numberQuiz,
        WordChainGameService wordChainGame,
        NumberGameSession numberGame,
        SnakeGameSession snakeGame,
        PictureMatchGameSession pictureMatchGame,
        AlphabetGameSession alphabetGame,
        TreasureHuntGameSession treasureHuntGame,
        WordBingoGameSession wordBingoGame)
    {
        InitializeComponent();
        _catalog = catalog;
        _instructionReader = instructionReader;
        _mathLearning = mathLearning;
        _wordPuzzle = wordPuzzle;
        _numberQuiz = numberQuiz;
        _wordChainGame = wordChainGame;
        _numberGame = numberGame;
        _snakeGame = snakeGame;
        _pictureMatchGame = pictureMatchGame;
        _alphabetGame = alphabetGame;
        _treasureHuntGame = treasureHuntGame;
        _wordBingoGame = wordBingoGame;
        _snakeTimer.Tick += SnakeTimer_Tick;
        CategorySelector.ItemsSource = _catalog.Categories;
        CategorySelector.SelectedIndex = 0;
    }

    private void OpenMath_Click(object sender, RoutedEventArgs e)
    {
        _displayVersion++;
        _numberGameQuestionVersion++;
        _numberQuizQuestionVersion++;
        _instructionReader.Stop();
        StopSnakeGame();
        WordZone.Visibility = Visibility.Collapsed;
        GameZone.Visibility = Visibility.Collapsed;
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
        _numberGameQuestionVersion++;
        _numberQuizQuestionVersion++;
        _instructionReader.Stop();
        StopSnakeGame();
        MathZone.Visibility = Visibility.Collapsed;
        GameZone.Visibility = Visibility.Collapsed;
        WordZone.Visibility = Visibility.Visible;
    }

    private void OpenGames_Click(object sender, RoutedEventArgs e)
    {
        _displayVersion++;
        _mathQuestionVersion++;
        _numberGameQuestionVersion++;
        _numberQuizQuestionVersion++;
        _instructionReader.Stop();
        StopSnakeGame();
        WordZone.Visibility = Visibility.Collapsed;
        MathZone.Visibility = Visibility.Collapsed;
        GameZone.Visibility = Visibility.Visible;
        GameMenuPanel.Visibility = Visibility.Visible;
        WordChainPanel.Visibility = Visibility.Collapsed;
        SnakeGamePanel.Visibility = Visibility.Collapsed;
        PictureMatchPanel.Visibility = Visibility.Collapsed;
        NumberGamePanel.Visibility = Visibility.Collapsed;
        HideWordActivityPanels();
    }

    private void StartWordChain_Click(object sender, RoutedEventArgs e)
    {
        _gameWords = _catalog.GetGameWords(_words);
        _wordChainGame.Start(_gameWords);
        _wordChainSessionVersion++;
        WordChainScoreText.Text = "Punkty: 0";
        GameMenuPanel.Visibility = Visibility.Collapsed;
        HideWordActivityPanels();
        WordChainPanel.Visibility = Visibility.Visible;
        SnakeGamePanel.Visibility = Visibility.Collapsed;
        PictureMatchPanel.Visibility = Visibility.Collapsed;
        NumberGamePanel.Visibility = Visibility.Collapsed;
        WordChainStatusText.Text = string.Empty;
        RefreshWordChainLetters();
    }

    private void BackToGameMenu_Click(object sender, RoutedEventArgs e)
    {
        _wordChainSessionVersion++;
        _numberGameQuestionVersion++;
        _pictureMatchGeneration++;
        StopSnakeGame();
        WordChainPanel.Visibility = Visibility.Collapsed;
        SnakeGamePanel.Visibility = Visibility.Collapsed;
        PictureMatchPanel.Visibility = Visibility.Collapsed;
        NumberGamePanel.Visibility = Visibility.Collapsed;
        HideWordActivityPanels();
        GameMenuPanel.Visibility = Visibility.Visible;
    }

    private void StartSnakeGame_Click(object sender, RoutedEventArgs e)
    {
        _gameWords = _catalog.GetGameWords(_words);
        GameMenuPanel.Visibility = Visibility.Collapsed;
        HideWordActivityPanels();
        WordChainPanel.Visibility = Visibility.Collapsed;
        PictureMatchPanel.Visibility = Visibility.Collapsed;
        NumberGamePanel.Visibility = Visibility.Collapsed;
        SnakeGamePanel.Visibility = Visibility.Visible;
        BeginSnakeRound();
    }

    private void StartSnakeRound_Click(object sender, RoutedEventArgs e)
    {
        BeginSnakeRound();
    }

    private void BeginSnakeRound()
    {
        _snakeTimer.Stop();
        _snakeGameVersion++;
        if (!_snakeGame.Start(_gameWords))
        {
            SnakeStatusText.Text = "Brak s≥Ûw do gry. Wybierz innπ kategoriÍ i sprÛbuj ponownie.";
            return;
        }

        SnakeScoreText.Text = $"Punkty: {_snakeGame.State.Score}";
        StartSnakeButton.Content = "Zagraj ponownie";
        SnakeStatusText.Text = "Steruj strza≥kami. Zbieraj kolejne litery s≥owa.";
        UpdateSnakeGoal();
        RenderSnakeBoard();
        _snakeTimer.Start();
        SnakeBoard.Focus();
    }

    private void UpdateSnakeGoal()
    {
        var state = _snakeGame.State;
        if (state.TargetLetterIndex >= state.TargetWord.Length)
        {
            SnakeGoalText.Text = $"U≥oøone s≥owo: {state.TargetWord}";
            return;
        }

        var progress = string.Concat(state.TargetWord.Select((letter, index) => index < state.TargetLetterIndex ? letter : '?'));
        SnakeGoalText.Text = $"U≥Ûø s≥owo: {progress}   ∑   nastÍpna litera: {state.TargetWord[state.TargetLetterIndex]}";
    }

    private void SnakeTimer_Tick(object? sender, EventArgs e)
    {
        var beforeMove = _snakeGame.State;
        var result = _snakeGame.Tick();
        if (result == SnakeTickResult.NotRunning)
        {
            return;
        }

        if (result == SnakeTickResult.GameOver)
        {
            _snakeTimer.Stop();
            SnakeStatusText.Text = "Koniec gry! Naciúnij ÑZagraj ponownieî, aby sprÛbowaÊ jeszcze raz.";
            RenderSnakeBoard();
            return;
        }

        if (result == SnakeTickResult.LetterCollected)
        {
            if (_soundEnabled)
            {
                _instructionReader.Speak(beforeMove.TargetWord[beforeMove.TargetLetterIndex].ToString().ToLowerInvariant());
            }

            SnakeStatusText.Text = "Dobra robota! Zbieraj nastÍpnπ literÍ.";
        }

        if (result == SnakeTickResult.WordCompleted)
        {
            var state = _snakeGame.State;
            SnakeScoreText.Text = $"Punkty: {state.Score}";
            SnakeStatusText.Text = $"Brawo! U≥oøono s≥owo Ñ{state.TargetWord}î. Za chwilÍ nastÍpne.";
            if (_soundEnabled)
            {
                _instructionReader.Speak(state.TargetWord.ToLowerInvariant());
            }

            _snakeTimer.Stop();
            UpdateSnakeGoal();
            RenderSnakeBoard();
            _ = StartNextSnakeWordAsync(_snakeGameVersion);
            return;
        }

        UpdateSnakeGoal();
        RenderSnakeBoard();
    }

    private async Task StartNextSnakeWordAsync(int version)
    {
        await Task.Delay(TimeSpan.FromSeconds(2));
        if (version != _snakeGameVersion || !_snakeGame.State.IsRunning || SnakeGamePanel.Visibility != Visibility.Visible)
        {
            return;
        }

        if (!_snakeGame.StartNextWord())
        {
            StopSnakeGame();
            return;
        }

        UpdateSnakeGoal();
        SnakeStatusText.Text = "Steruj strza≥kami i zbieraj litery.";
        RenderSnakeBoard();
        _snakeTimer.Start();
    }

    private void RenderSnakeBoard()
    {
        SnakeBoard.Children.Clear();
        var state = _snakeGame.State;
        var foodX = state.FoodPosition.X * SnakeCellSize;
        var foodY = state.FoodPosition.Y * SnakeCellSize;
        SnakeBoard.Children.Add(new Ellipse
        {
            Width = SnakeCellSize - 2,
            Height = SnakeCellSize - 2,
            Fill = new SolidColorBrush(Color.FromRgb(255, 190, 65))
        });
        Canvas.SetLeft(SnakeBoard.Children[^1], foodX + 1);
        Canvas.SetTop(SnakeBoard.Children[^1], foodY + 1);
        var foodLetter = new TextBlock
        {
            Text = state.TargetWord.Length > state.TargetLetterIndex ? state.TargetWord[state.TargetLetterIndex].ToString() : string.Empty,
            Width = SnakeCellSize,
            Height = SnakeCellSize,
            FontSize = 17,
            FontWeight = FontWeights.Bold,
            TextAlignment = TextAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Foreground = new SolidColorBrush(Color.FromRgb(39, 49, 78))
        };
        SnakeBoard.Children.Add(foodLetter);
        Canvas.SetLeft(foodLetter, foodX);
        Canvas.SetTop(foodLetter, foodY);

        for (var index = state.Body.Count - 1; index >= 0; index--)
        {
            var segment = new Border
            {
                Width = SnakeCellSize - 2,
                Height = SnakeCellSize - 2,
                CornerRadius = new CornerRadius(6),
                Background = new SolidColorBrush(index == 0
                    ? Color.FromRgb(91, 99, 232)
                    : Color.FromRgb(129, 139, 242))
            };
            SnakeBoard.Children.Add(segment);
            Canvas.SetLeft(segment, state.Body[index].X * SnakeCellSize + 1);
            Canvas.SetTop(segment, state.Body[index].Y * SnakeCellSize + 1);
        }
    }

    private void StopSnakeGame()
    {
        _snakeGame.Stop();
        _snakeGameVersion++;
        _snakeTimer.Stop();
    }

    private void StartPictureMatch_Click(object sender, RoutedEventArgs e)
    {
        var candidates = _catalog.GetGameWords(_words);
        var resetScore = PictureMatchPanel.Visibility != Visibility.Visible;
        if (!_pictureMatchGame.Start(candidates, resetScore))
        {
            PictureMatchStatusText.Text = "Brak s≥Ûw do przygotowania planszy.";
            return;
        }

        StopSnakeGame();
        GameMenuPanel.Visibility = Visibility.Collapsed;
        HideWordActivityPanels();
        WordChainPanel.Visibility = Visibility.Collapsed;
        SnakeGamePanel.Visibility = Visibility.Collapsed;
        PictureMatchPanel.Visibility = Visibility.Visible;
        PictureMatchStatusText.Text = "Dopasuj wszystkie obrazki, uk≥adajπc ich nazwy.";
        _pictureMatchGeneration++;
        var generation = _pictureMatchGeneration;
        _pictureMatchPuzzles.Clear();
        PictureMatchCardsPanel.Children.Clear();
        PictureMatchScoreText.Text = $"Punkty: {_pictureMatchGame.State.Score}";

        foreach (var puzzleState in _pictureMatchGame.State.Puzzles)
        {
            var puzzle = new PictureMatchPuzzle(puzzleState.Word);
            _pictureMatchPuzzles.Add(puzzle);
            PictureMatchCardsPanel.Children.Add(CreatePictureMatchCard(puzzle));
            _ = LoadPictureMatchImageAsync(puzzle, generation);
        }
    }

    private Border CreatePictureMatchCard(PictureMatchPuzzle puzzle)
    {
        var cardContent = new StackPanel();
        var pictureArea = new Grid
        {
            Height = 122,
            Margin = new Thickness(0, 0, 0, 10),
            Background = new SolidColorBrush(Color.FromRgb(246, 247, 255))
        };
        puzzle.EmojiText = new TextBlock
        {
            Text = puzzle.Word.Emoji,
            FontSize = 32,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(Color.FromRgb(91, 99, 232)),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };
        puzzle.Image = new Image
        {
            Stretch = Stretch.Uniform,
            MaxWidth = 250,
            MaxHeight = 120,
            Tag = GetImageCacheKey(puzzle.Word),
            Visibility = Visibility.Collapsed
        };
        puzzle.Image.ImageFailed += (_, _) =>
        {
            puzzle.Image.Visibility = Visibility.Collapsed;
            puzzle.EmojiText.Visibility = Visibility.Visible;
            puzzle.ImageCreditText.Visibility = Visibility.Collapsed;
            if (puzzle.Image.Tag is string imageCacheKey)
            {
                _imageCache[imageCacheKey] = null;
            }
        };
        pictureArea.Children.Add(puzzle.EmojiText);
        pictureArea.Children.Add(puzzle.Image);
        cardContent.Children.Add(pictureArea);
        puzzle.ImageCreditText = new TextBlock
        {
            Margin = new Thickness(0, 0, 0, 6),
            HorizontalAlignment = HorizontalAlignment.Center,
            FontSize = 11,
            Foreground = new SolidColorBrush(Color.FromRgb(119, 129, 154)),
            Visibility = Visibility.Collapsed
        };
        cardContent.Children.Add(puzzle.ImageCreditText);
        cardContent.Children.Add(new TextBlock
        {
            Text = puzzle.Word.Clue,
            Margin = new Thickness(0, 0, 0, 8),
            FontSize = 14,
            Foreground = new SolidColorBrush(Color.FromRgb(119, 129, 154)),
            TextAlignment = TextAlignment.Center,
            TextWrapping = TextWrapping.Wrap
        });

        puzzle.AnswerPanel = new ItemsControl { HorizontalAlignment = HorizontalAlignment.Center };
        puzzle.AnswerPanel.ItemsPanel = new ItemsPanelTemplate(new FrameworkElementFactory(typeof(WrapPanel)));
        cardContent.Children.Add(puzzle.AnswerPanel);
        puzzle.LetterBank = new ItemsControl { HorizontalAlignment = HorizontalAlignment.Center };
        puzzle.LetterBank.ItemsPanel = new ItemsPanelTemplate(new FrameworkElementFactory(typeof(WrapPanel)));
        cardContent.Children.Add(puzzle.LetterBank);
        puzzle.StatusText = new TextBlock
        {
            Margin = new Thickness(0, 4, 0, 8),
            HorizontalAlignment = HorizontalAlignment.Center,
            FontSize = 14,
            FontWeight = FontWeights.SemiBold,
            TextAlignment = TextAlignment.Center,
            TextWrapping = TextWrapping.Wrap,
            Foreground = new SolidColorBrush(Color.FromRgb(119, 129, 154))
        };
        cardContent.Children.Add(puzzle.StatusText);

        var buttons = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Center };
        puzzle.CheckButton = new Button
        {
            Content = "Sprawdü",
            Style = (Style)FindResource("PrimaryButton"),
            Padding = new Thickness(14, 8, 14, 8),
            FontSize = 14,
            Margin = new Thickness(4)
        };
        puzzle.CheckButton.Click += (_, _) => CheckPictureMatchPuzzle(puzzle);
        var clearButton = new Button
        {
            Content = "WyczyúÊ",
            Style = (Style)FindResource("SecondaryButton"),
            Padding = new Thickness(14, 8, 14, 8),
            FontSize = 14,
            Margin = new Thickness(4)
        };
        clearButton.Click += (_, _) =>
        {
            if (_pictureMatchGame.ClearSelection(puzzle.Word.Text))
            {
                puzzle.StatusText.Text = string.Empty;
                RefreshPictureMatchPuzzle(puzzle);
            }
        };
        buttons.Children.Add(clearButton);
        buttons.Children.Add(puzzle.CheckButton);
        cardContent.Children.Add(buttons);
        RefreshPictureMatchPuzzle(puzzle);

        return new Border
        {
            Width = 340,
            MinHeight = 390,
            Margin = new Thickness(10),
            Padding = new Thickness(18),
            Background = Brushes.White,
            BorderBrush = new SolidColorBrush(Color.FromRgb(228, 232, 242)),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(20),
            Child = cardContent
        };
    }

    private void RefreshPictureMatchPuzzle(PictureMatchPuzzle puzzle)
    {
        puzzle.AnswerPanel.Items.Clear();
        puzzle.LetterBank.Items.Clear();
        var state = _pictureMatchGame.GetPuzzleState(puzzle.Word.Text);
        if (state is null)
        {
            return;
        }

        for (var index = 0; index < puzzle.Word.Text.Length; index++)
        {
            var selectedIndex = index < state.SelectedLetterIndices.Count
                ? state.SelectedLetterIndices[index]
                : -1;
            puzzle.AnswerPanel.Items.Add(new Border
            {
                Width = 42,
                Height = 46,
                Margin = new Thickness(3),
                CornerRadius = new CornerRadius(9),
                BorderThickness = new Thickness(1),
                BorderBrush = new SolidColorBrush(Color.FromRgb(225, 230, 241)),
                Background = new SolidColorBrush(Color.FromRgb(248, 249, 253)),
                Child = new TextBlock
                {
                    Text = selectedIndex >= 0 ? puzzle.Word.Text[selectedIndex].ToString() : string.Empty,
                    FontSize = 20,
                    FontWeight = FontWeights.Bold,
                    Foreground = new SolidColorBrush(Color.FromRgb(39, 49, 78)),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                }
            });
        }

        foreach (var letterIndex in state.LetterBankIndices)
        {
            var selected = state.SelectedLetterIndices.Contains(letterIndex);
            var button = new Button
            {
                Content = puzzle.Word.Text[letterIndex].ToString(),
                Width = 48,
                Height = 50,
                Margin = new Thickness(3),
                Padding = new Thickness(0),
                FontSize = 21,
                FontWeight = FontWeights.Bold,
                Style = (Style)FindResource("LetterTile"),
                IsEnabled = !selected && !state.IsSolved,
                Opacity = selected ? 0.35 : 1,
                Tag = new PictureMatchLetter(puzzle, letterIndex)
            };
            button.Click += PictureMatchLetter_Click;
            puzzle.LetterBank.Items.Add(button);
        }

        puzzle.CheckButton.IsEnabled = !state.IsSolved;
    }

    private void PictureMatchLetter_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: PictureMatchLetter selection }
            && _pictureMatchGame.SelectLetter(selection.Puzzle.Word.Text, selection.Index))
        {
            selection.Puzzle.StatusText.Text = string.Empty;
            RefreshPictureMatchPuzzle(selection.Puzzle);
        }
    }

    private void CheckPictureMatchPuzzle(PictureMatchPuzzle puzzle)
    {
        var result = _pictureMatchGame.SubmitAnswer(puzzle.Word.Text);
        if (result == PictureMatchAnswerResult.AlreadySolved)
        {
            return;
        }

        if (result == PictureMatchAnswerResult.Incomplete)
        {
            puzzle.StatusText.Text = "Wybierz wszystkie litery.";
            puzzle.StatusText.Foreground = new SolidColorBrush(Color.FromRgb(204, 139, 43));
            return;
        }

        if (result == PictureMatchAnswerResult.Incorrect)
        {
            puzzle.StatusText.Text = "SprÛbuj zmieniÊ kolejnoúÊ liter.";
            puzzle.StatusText.Foreground = new SolidColorBrush(Color.FromRgb(207, 87, 100));
            return;
        }

        puzzle.StatusText.Text = "Dopasowano! Brawo!";
        puzzle.StatusText.Foreground = new SolidColorBrush(Color.FromRgb(38, 148, 105));
        PictureMatchScoreText.Text = $"Punkty: {_pictureMatchGame.State.Score}";
        RefreshPictureMatchPuzzle(puzzle);
        var nextPuzzleState = _pictureMatchGame.State.Puzzles.FirstOrDefault(item => !item.IsSolved);
        if (nextPuzzleState is null)
        {
            PictureMatchStatusText.Text = "åwietnie! Wszystkie obrazki dopasowane. NastÍpna plansza za chwilÍ.";
            _ = AdvancePictureMatchBoardAsync(_pictureMatchGeneration);
            return;
        }

        PictureMatchStatusText.Text = $"NastÍpne s≥owo: {nextPuzzleState.Word.Text}";
        var nextPuzzle = _pictureMatchPuzzles.FirstOrDefault(item =>
            string.Equals(item.Word.Text, nextPuzzleState.Word.Text, StringComparison.OrdinalIgnoreCase));
        nextPuzzle?.LetterBank.Items.OfType<Button>().FirstOrDefault()?.Focus();
    }

    private async Task AdvancePictureMatchBoardAsync(int generation)
    {
        await Task.Delay(TimeSpan.FromSeconds(2));
        if (generation == _pictureMatchGeneration
            && PictureMatchPanel.Visibility == Visibility.Visible
            && _pictureMatchGame.State.Puzzles.All(item => item.IsSolved))
        {
            StartPictureMatch_Click(this, new RoutedEventArgs());
        }
    }

    private async Task LoadPictureMatchImageAsync(PictureMatchPuzzle puzzle, int generation)
    {
        try
        {
            var imageCacheKey = GetImageCacheKey(puzzle.Word);
            WordImage? image;
            if (_imageCache.TryGetValue(imageCacheKey, out var cachedImage))
            {
                image = cachedImage;
            }
            else
            {
                image = await _catalog.GetImageAsync(puzzle.Word);
                _imageCache[imageCacheKey] = image;
            }

            if (generation != _pictureMatchGeneration || image is null)
            {
                return;
            }

            puzzle.Image.Source = new BitmapImage(new Uri(image.ImageUrl, UriKind.Absolute));
            puzzle.Image.Visibility = Visibility.Visible;
            puzzle.EmojiText.Visibility = Visibility.Collapsed;
            SetImageCredit(puzzle.ImageCreditText, image.SourceUrl);
        }
        catch (Exception exception) when (exception is HttpRequestException or TaskCanceledException or JsonException or UriFormatException or InvalidOperationException)
        {
            if (generation == _pictureMatchGeneration)
            {
                puzzle.Image.Visibility = Visibility.Collapsed;
                puzzle.EmojiText.Visibility = Visibility.Visible;
                puzzle.ImageCreditText.Visibility = Visibility.Collapsed;
            }
        }
    }

    private void RefreshWordChainLetters()
    {
        WordChainAnswerPanel.Items.Clear();
        WordChainLetterBank.Items.Clear();
        var state = _wordChainGame.State;
        if (state.TargetWord is not { } word)
        {
            return;
        }

        var answer = state.IsRoundComplete
            ? word.Text
            : new string(state.SelectedLetterIndices.Select(index => state.LetterBank[index]).ToArray());
        for (var index = 0; index < word.Text.Length; index++)
        {
            WordChainAnswerPanel.Items.Add(new Border
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
                    Text = index < answer.Length ? answer[index].ToString() : string.Empty,
                    FontSize = 29,
                    FontWeight = FontWeights.Bold,
                    Foreground = new SolidColorBrush(Color.FromRgb(39, 49, 78)),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                }
            });
        }

        for (var letterIndex = 0; letterIndex < state.LetterBank.Count; letterIndex++)
        {
            var selected = state.SelectedLetterIndices.Contains(letterIndex);
            var letterButton = new Button
            {
                Content = state.LetterBank[letterIndex].ToString(),
                Style = (Style)FindResource("LetterTile"),
                IsEnabled = !selected && !state.IsRoundComplete,
                Opacity = selected ? 0.35 : 1,
                Tag = letterIndex
            };
            letterButton.Click += WordChainLetter_Click;
            WordChainLetterBank.Items.Add(letterButton);
        }

        WordChainProgressText.Text = $"Runda {state.Round} ∑ s≥owo {word.Text.Length}-literowe";
        WordChainBankHint.Text = "U≥Ûø nazwÍ pasujπcπ do obrazka.";
        WordChainCategoryText.Text = $"Kategoria: {word.CategoryName ?? "RÛøne"}";
        WordChainClueText.Text = word.Clue;
        WordChainScoreText.Text = $"Punkty: {state.Score}";
        CheckWordChainButton.IsEnabled = !state.IsRoundComplete;
        NextWordChainButton.IsEnabled = state.IsRoundComplete;
        UpdateWordChainImage(word);
    }

    private void UpdateWordChainImage(WordEntry word)
    {
        var imageCacheKey = GetImageCacheKey(word);
        WordChainEmojiText.Text = word.Emoji;
        if (string.Equals(_wordChainImageKey, imageCacheKey, StringComparison.Ordinal))
        {
            return;
        }

        _wordChainImageKey = imageCacheKey;
        var imageVersion = ++_wordChainImageVersion;
        WordChainImage.Source = null;
        WordChainImage.Tag = imageCacheKey;
        WordChainImage.Visibility = Visibility.Collapsed;
        WordChainEmojiText.Visibility = Visibility.Visible;
        WordChainImageCreditText.Inlines.Clear();
        WordChainImageCreditText.Visibility = Visibility.Collapsed;
        _ = LoadWordChainImageAsync(word, imageCacheKey, imageVersion);
    }

    private void WordChainImage_ImageFailed(object sender, ExceptionRoutedEventArgs e)
    {
        WordChainImage.Source = null;
        WordChainImage.Visibility = Visibility.Collapsed;
        WordChainEmojiText.Visibility = Visibility.Visible;
        WordChainImageCreditText.Visibility = Visibility.Collapsed;
        if (WordChainImage.Tag is string imageCacheKey)
        {
            _imageCache[imageCacheKey] = null;
        }
    }

    private async Task LoadWordChainImageAsync(WordEntry word, string imageCacheKey, int imageVersion)
    {
        try
        {
            if (!_imageCache.TryGetValue(imageCacheKey, out var image))
            {
                image = await _catalog.GetImageAsync(word);
                _imageCache[imageCacheKey] = image;
            }

            if (imageVersion != _wordChainImageVersion || WordChainPanel.Visibility != Visibility.Visible || image is null)
            {
                return;
            }

            WordChainImage.Source = new BitmapImage(new Uri(image.ImageUrl, UriKind.Absolute));
            WordChainImage.Visibility = Visibility.Visible;
            WordChainEmojiText.Visibility = Visibility.Collapsed;
            SetImageCredit(WordChainImageCreditText, image.SourceUrl);
        }
        catch (Exception exception) when (exception is HttpRequestException or TaskCanceledException or JsonException or UriFormatException or InvalidOperationException)
        {
            if (imageVersion == _wordChainImageVersion)
            {
                WordChainImage.Visibility = Visibility.Collapsed;
                WordChainEmojiText.Visibility = Visibility.Visible;
                WordChainImageCreditText.Visibility = Visibility.Collapsed;
            }
        }
    }

    private void WordChainLetter_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: int letterIndex } && _wordChainGame.SelectLetter(letterIndex))
        {
            RefreshWordChainLetters();
        }
    }

    private void UndoWordChainLetter_Click(object sender, RoutedEventArgs e)
    {
        if (_wordChainGame.UndoSelection())
        {
            RefreshWordChainLetters();
        }
    }

    private void ClearWordChain_Click(object sender, RoutedEventArgs e)
    {
        if (_wordChainGame.ClearSelection())
        {
            RefreshWordChainLetters();
        }
    }

    private void CheckWordChain_Click(object sender, RoutedEventArgs e)
    {
        var result = _wordChainGame.SubmitAnswer();
        if (result == WordChainAnswerResult.Incomplete)
        {
            WordChainStatusText.Text = "Wybierz wszystkie litery, zanim sprawdzisz.";
            WordChainStatusText.Foreground = new SolidColorBrush(Color.FromRgb(204, 139, 43));
            return;
        }

        if (result == WordChainAnswerResult.Incorrect)
        {
            WordChainStatusText.Text = "SprÛbuj zmieniÊ kolejnoúÊ liter.";
            WordChainStatusText.Foreground = new SolidColorBrush(Color.FromRgb(207, 87, 100));
            return;
        }

        var state = _wordChainGame.State;
        WordChainStatusText.Foreground = new SolidColorBrush(Color.FromRgb(38, 148, 105));
        WordChainStatusText.Text = "Brawo! åwietnie u≥oøone s≥owo. NastÍpne za chwilÍ.";
        RefreshWordChainLetters();
        if (_soundEnabled && state.TargetWord is { } word)
        {
            _instructionReader.Speak(word.Text.ToLowerInvariant());
        }

        if (result == WordChainAnswerResult.RoundSolved)
        {
            _ = AdvanceWordChainRoundAsync();
        }
    }

    private void NextWordChain_Click(object sender, RoutedEventArgs e)
    {
        _ = AdvanceWordChainRoundAsync();
    }

    private async Task AdvanceWordChainRoundAsync()
    {
        var state = _wordChainGame.State;
        if (!state.IsRoundComplete)
        {
            return;
        }

        var completedWord = state.TargetWord?.Text ?? string.Empty;
        WordChainStatusText.Text = $"U≥oøono: {completedWord}. Za chwilÍ kolejne s≥owo.";
        WordChainStatusText.Foreground = new SolidColorBrush(Color.FromRgb(38, 148, 105));
        NextWordChainButton.IsEnabled = false;
        if (_soundEnabled)
        {
            _instructionReader.Speak($"U≥oøone s≥owo: {completedWord}");
        }

        var version = ++_wordChainSessionVersion;
        await Task.Delay(TimeSpan.FromSeconds(2));
        if (version == _wordChainSessionVersion
            && WordChainPanel.Visibility == Visibility.Visible
            && _wordChainGame.NextRound())
        {
            WordChainStatusText.Text = string.Empty;
            RefreshWordChainLetters();
        }
    }

    private void StartNumberGame_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: string modeName }
            || !Enum.TryParse(modeName, out NumberGameMode mode))
        {
            return;
        }

        _numberGameQuestionVersion++;
        _numberGame.Start(mode);
        GameMenuPanel.Visibility = Visibility.Collapsed;
        WordChainPanel.Visibility = Visibility.Collapsed;
        SnakeGamePanel.Visibility = Visibility.Collapsed;
        PictureMatchPanel.Visibility = Visibility.Collapsed;
        NumberGamePanel.Visibility = Visibility.Visible;
        HideWordActivityPanels();
        StopSnakeGame();
        UpdateNumberGameTitle();
        RenderNumberGameQuestion(readPrompt: true);
    }

    private void UpdateNumberGameTitle()
    {
        NumberGameTitleText.Text = _numberGame.State.Mode switch
        {
            NumberGameMode.Count => "POLICZ KROPKI",
            NumberGameMode.Find => "ZNAJDè CYFR ",
            NumberGameMode.Build => "U£”Ø LICZB ",
            _ => "GRA LICZBOWA"
        };
    }

    private void RenderNumberGameQuestion(bool readPrompt = false)
    {
        var state = _numberGame.State;
        NumberGameChoices.Items.Clear();
        NumberGameDotsPanel.Visibility = state.Quantity.HasValue ? Visibility.Visible : Visibility.Collapsed;
        FillDotPanel(NumberGameDotsPanel, state.Quantity ?? 0);
        NumberGameBuiltNumberText.Text = state.BuiltNumber;
        NumberGamePromptText.Text = state.Prompt;
        NumberGameStatusText.Text = string.Empty;
        NumberGameScoreText.Text = $"Punkty: {state.Score}";
        CheckNumberGameButton.Visibility = state.Mode == NumberGameMode.Build ? Visibility.Visible : Visibility.Collapsed;
        ClearNumberGameButton.Visibility = state.Mode == NumberGameMode.Build ? Visibility.Visible : Visibility.Collapsed;
        CheckNumberGameButton.IsEnabled = !state.IsComplete;
        ClearNumberGameButton.IsEnabled = !state.IsComplete;
        NextNumberGameButton.IsEnabled = state.IsComplete;

        foreach (var choice in state.Choices)
        {
            AddNumberGameButton(choice.ToString(), choice);
        }

        if (readPrompt)
        {
            SpeakNumberGamePrompt(state.SpokenPrompt);
        }
    }

    private void AddNumberGameButton(string label, int value)
    {
        var button = new Button
        {
            Content = label,
            Tag = value,
            Width = 92,
            Height = 76,
            Margin = new Thickness(7),
            FontSize = 30,
            Style = (Style)FindResource("PrimaryButton")
        };
        button.Click += NumberGameChoice_Click;
        NumberGameChoices.Items.Add(button);
    }

    private void NumberGameChoice_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: int value })
        {
            return;
        }

        if (_numberGame.State.Mode == NumberGameMode.Build)
        {
            _numberGame.AppendDigit(value);
            NumberGameBuiltNumberText.Text = _numberGame.State.BuiltNumber;
            return;
        }

        ShowNumberGameAnswerResult(_numberGame.SubmitChoice(value));
    }

    private void CheckNumberGameAnswer_Click(object sender, RoutedEventArgs e)
    {
        var result = _numberGame.SubmitBuiltNumber();
        if (result == NumberGameAnswerResult.Incomplete)
        {
            NumberGameStatusText.Text = "Wybierz dwie cyfry.";
            NumberGameStatusText.Foreground = new SolidColorBrush(Color.FromRgb(204, 139, 43));
            return;
        }

        ShowNumberGameAnswerResult(result);
    }

    private void ClearNumberGameAnswer_Click(object sender, RoutedEventArgs e)
    {
        _numberGame.ClearBuiltNumber();
        NumberGameBuiltNumberText.Text = _numberGame.State.BuiltNumber;
    }

    private void ShowNumberGameAnswerResult(NumberGameAnswerResult result)
    {
        if (result != NumberGameAnswerResult.Correct)
        {
            NumberGameStatusText.Text = "SprÛbuj jeszcze raz!";
            NumberGameStatusText.Foreground = new SolidColorBrush(Color.FromRgb(207, 87, 100));
            return;
        }

        var state = _numberGame.State;
        NumberGameScoreText.Text = $"Punkty: {state.Score}";
        NumberGameStatusText.Text = "Brawo! Dobra odpowiedü!";
        NumberGameStatusText.Foreground = new SolidColorBrush(Color.FromRgb(38, 148, 105));
        foreach (var button in NumberGameChoices.Items.OfType<Button>())
        {
            button.IsEnabled = false;
        }
        CheckNumberGameButton.IsEnabled = false;
        NextNumberGameButton.IsEnabled = true;
        if (_soundEnabled)
        {
            _instructionReader.Speak("Brawo! Dobra odpowiedü!");
        }

        _ = AdvanceNumberGameQuestionAsync(_numberGameQuestionVersion);
    }

    private void NextNumberGameQuestion_Click(object sender, RoutedEventArgs e)
    {
        _numberGameQuestionVersion++;
        _numberGame.NextQuestion();
        RenderNumberGameQuestion(readPrompt: true);
    }

    private async Task AdvanceNumberGameQuestionAsync(int completedQuestionVersion)
    {
        await Task.Delay(TimeSpan.FromSeconds(2));
        if (completedQuestionVersion == _numberGameQuestionVersion
            && NumberGamePanel.Visibility == Visibility.Visible
            && _numberGame.State.IsComplete)
        {
            NextNumberGameQuestion_Click(this, new RoutedEventArgs());
        }
    }

    private void SpeakNumberGamePrompt(string text)
    {
        if (_soundEnabled)
        {
            _instructionReader.Stop();
            _instructionReader.Speak(text);
        }
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
        NumberQuizPanel.Visibility = Visibility.Collapsed;
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
        UpdateNumberVisual(_mathLearning.CurrentNumber);
        if (readAloud && _soundEnabled)
        {
            _instructionReader.Stop();
            _instructionReader.Speak(_mathLearning.CurrentNumberWords);
        }
    }

    private void UpdateNumberVisual(int number)
    {
        NumberDotsPanel.Visibility = number <= NumberQuizService.MaximumNumber ? Visibility.Visible : Visibility.Collapsed;
        FillDotPanel(NumberDotsPanel, number <= NumberQuizService.MaximumNumber ? number : 0);

        var thousands = number / 1000;
        var hundreds = number % 1000 / 100;
        var tens = number % 100 / 10;
        var units = number % 10;
        var placeValues = new List<string>();
        if (thousands > 0)
        {
            placeValues.Add($"Tysiπce: {thousands}");
        }
        if (hundreds > 0 || thousands > 0)
        {
            placeValues.Add($"Setki: {hundreds}");
        }
        if (tens > 0 || hundreds > 0 || thousands > 0)
        {
            placeValues.Add($"Dziesiπtki: {tens}");
        }
        placeValues.Add($"Jednoúci: {units}");
        NumberPlaceValueText.Text = string.Join("  ï  ", placeValues);
    }

    private static void FillDotPanel(Panel panel, int count)
    {
        panel.Children.Clear();
        for (var index = 0; index < count; index++)
        {
            panel.Children.Add(new Ellipse
            {
                Width = 18,
                Height = 18,
                Margin = new Thickness(4),
                Fill = new SolidColorBrush(Color.FromRgb(91, 99, 232))
            });
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

    private void StartNumberQuiz_Click(object sender, RoutedEventArgs e)
    {
        NumberLearningPanel.Visibility = Visibility.Collapsed;
        NumberQuizPanel.Visibility = Visibility.Visible;
        GenerateNumberQuizQuestion();
    }

    private void ReturnToNumberLearning_Click(object sender, RoutedEventArgs e)
    {
        _numberQuizQuestionVersion++;
        NumberQuizPanel.Visibility = Visibility.Collapsed;
        NumberLearningPanel.Visibility = Visibility.Visible;
        UpdateNumberDisplay(readAloud: false);
    }

    private void GenerateNumberQuizQuestion()
    {
        _numberQuizQuestionVersion++;
        var question = _numberQuiz.CreateQuestion();
        _currentNumberQuizQuestion = question;
        NumberQuizQuestionText.Text = question.Prompt;
        NumberQuizDotsPanel.Visibility = question.Quantity.HasValue ? Visibility.Visible : Visibility.Collapsed;
        FillDotPanel(NumberQuizDotsPanel, question.Quantity ?? 0);
        NumberQuizAnswers.Items.Clear();
        NumberQuizStatusText.Text = string.Empty;
        NextNumberQuizButton.IsEnabled = false;

        foreach (var choice in question.Choices)
        {
            var answerButton = new Button
            {
                Content = new TextBlock
                {
                    Text = choice,
                    TextAlignment = TextAlignment.Center,
                    TextWrapping = TextWrapping.Wrap
                },
                Tag = choice,
                Width = 190,
                Height = 72,
                Margin = new Thickness(7),
                FontSize = 23,
                Style = (Style)FindResource("PrimaryButton")
            };
            answerButton.Click += NumberQuizAnswer_Click;
            NumberQuizAnswers.Items.Add(answerButton);
        }

        if (_soundEnabled)
        {
            _instructionReader.Stop();
            _instructionReader.Speak(question.SpokenPrompt);
        }
    }

    private void NumberQuizAnswer_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: string answer } || _currentNumberQuizQuestion is not { } question)
        {
            return;
        }

        if (!string.Equals(answer, question.CorrectAnswer, StringComparison.Ordinal))
        {
            NumberQuizStatusText.Text = "SprÛbuj jeszcze raz!";
            NumberQuizStatusText.Foreground = new SolidColorBrush(Color.FromRgb(204, 139, 43));
            return;
        }

        _numberQuizScore++;
        NumberQuizScoreText.Text = $"Dobre odpowiedzi: {_numberQuizScore}";
        NumberQuizStatusText.Text = "Brawo! åwietnie!";
        NumberQuizStatusText.Foreground = new SolidColorBrush(Color.FromRgb(38, 148, 105));
        foreach (var item in NumberQuizAnswers.Items.OfType<Button>())
        {
            item.IsEnabled = false;
        }
        NextNumberQuizButton.IsEnabled = true;

        if (_soundEnabled)
        {
            _instructionReader.Stop();
            _instructionReader.Speak("Brawo! åwietnie!");
        }

        _ = AdvanceNumberQuizQuestionAsync(_numberQuizQuestionVersion);
    }

    private void NextNumberQuiz_Click(object sender, RoutedEventArgs e)
    {
        GenerateNumberQuizQuestion();
    }

    private async Task AdvanceNumberQuizQuestionAsync(int completedQuestionVersion)
    {
        await Task.Delay(TimeSpan.FromSeconds(2));
        if (completedQuestionVersion == _numberQuizQuestionVersion
            && NumberQuizPanel.Visibility == Visibility.Visible)
        {
            GenerateNumberQuizQuestion();
        }
    }

    private void ReadNumberQuizQuestion_Click(object sender, RoutedEventArgs e)
    {
        if (_soundEnabled && _currentNumberQuizQuestion is { } question)
        {
            _instructionReader.Stop();
            _instructionReader.Speak(question.SpokenPrompt);
        }
    }

    private void GenerateMathQuestion()
    {
        _mathQuestionVersion++;
        MathAnswers.IsEnabled = true;
        MathAnswers.Items.Clear();
        MathStatusText.Text = string.Empty;

        _currentMathQuestion = MathQuestionViewModel.From(_mathLearning.CreateQuestion());
        MathQuestionText.Text = _currentMathQuestion.QuestionText;
        MathLessonHint.Text = _mathLearning.GetDifficultyHint();
        RefreshMathProgress();

        foreach (var choice in _currentMathQuestion.Choices)
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
            _instructionReader.Speak(_currentMathQuestion.SpokenQuestion);
        }
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
            if (_currentMathQuestion is { } question)
            {
                _instructionReader.Stop();
                _instructionReader.Speak(question.SpokenQuestion);
            }
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

        _categoryLoadCancellation?.Cancel();
        _categoryLoadCancellation?.Dispose();
        var loadCancellation = new CancellationTokenSource();
        _categoryLoadCancellation = loadCancellation;
        var loadVersion = ++_categoryLoadVersion;
        _currentCategory = category;
        _words = category.OfflineWords;
        _wordIndex = -1;
        ShowNextWord();
        SetStatus("Pobieram obrazki z WikipediiÖ", Colors.DarkSlateBlue);

        try
        {
            var result = await _catalog.GetWordsAsync(category, loadCancellation.Token);
            if (loadCancellation.IsCancellationRequested || loadVersion != _categoryLoadVersion)
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
            SetStatus(result.ApiUnavailable
                ? $"Wikipedia jest chwilowo niedostÍpna. Uøywam s≥Ûw offline ({_words.Count}) i podpowiedzi emoji."
                : result.ApiImageCount > 0
                ? $"Obrazki z Wikipedii: {result.ApiImageCount}  ∑  W zestawie: {_words.Count} s≥Ûw"
                : $"W zestawie: {_words.Count} prostych s≥Ûw",
                result.ApiUnavailable
                    ? Color.FromRgb(204, 139, 43)
                    : result.ApiImageCount > 0 ? Color.FromRgb(38, 148, 105) : Color.FromRgb(104, 115, 138));
        }
        catch (OperationCanceledException) when (loadCancellation.IsCancellationRequested)
        {
        }
        catch (Exception exception) when (exception is HttpRequestException or JsonException or InvalidOperationException or IOException)
        {
            if (loadVersion == _categoryLoadVersion)
            {
                SetStatus($"Nie uda≥o siÍ po≥πczyÊ z Wikipediπ. Uøywam s≥Ûw offline ({_words.Count}).", Color.FromRgb(204, 139, 43));
            }
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
        _imageLoadCancellation?.Cancel();
        _imageLoadCancellation?.Dispose();
        _imageLoadCancellation = null;
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
            _imageLoadCancellation = new CancellationTokenSource();
            _ = LoadImageAsync(word, displayVersion, _imageLoadCancellation.Token);
        }
        if (readInstruction && _soundEnabled)
        {
            _instructionReader.Speak($"U≥Ûø s≥owo z kategorii {_currentCategory.Name}. Wybierz litery w dobrej kolejnoúci.");
        }
    }

    private async Task LoadImageAsync(WordEntry word, int displayVersion, CancellationToken cancellationToken)
    {
        var imageCacheKey = GetImageCacheKey(word);
        if (_imageCache.TryGetValue(imageCacheKey, out var cachedImage))
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
            var image = await _catalog.GetImageAsync(word, cancellationToken);
            if (cancellationToken.IsCancellationRequested || displayVersion != _displayVersion)
            {
                return;
            }

            if (image is null)
            {
                _imageCache[imageCacheKey] = null;
                return;
            }

            _imageCache[imageCacheKey] = image;
            PictureImage.Source = new BitmapImage(new Uri(image.ImageUrl, UriKind.Absolute));
            PictureImage.Visibility = Visibility.Visible;
            PictureText.Visibility = Visibility.Collapsed;
            SetImageCredit(image.SourceUrl);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
        catch (Exception exception) when (exception is HttpRequestException or TaskCanceledException or JsonException or UriFormatException)
        {
            _imageCache[imageCacheKey] = null;
        }
    }

    private static string GetImageCacheKey(WordEntry word) =>
        $"{word.Text}\u001f{word.ImageUrl}\u001f{word.SourceUrl}\u001f{word.ImageSearchTerm}";

    private void SetImageCredit(string sourceUrl)
    {
        SetImageCredit(ImageCreditText, sourceUrl);
    }

    private void SetImageCredit(TextBlock creditText, string sourceUrl)
    {
        creditText.Inlines.Clear();
        if (!Uri.TryCreate(sourceUrl, UriKind.Absolute, out var sourceUri)
            || sourceUri.Scheme is not ("http" or "https"))
        {
            creditText.Visibility = Visibility.Collapsed;
            return;
        }

        var sourceName = sourceUri.Host is "flagcdn.com" or "www.flagcdn.com"
            ? "FlagCDN"
            : "Wikimedia Commons / Wikipedia";
        creditText.Inlines.Add(new Run("èrÛd≥o obrazka: "));
        var link = new Hyperlink(new Run(sourceName))
        {
            NavigateUri = sourceUri
        };
        link.RequestNavigate += SourceLink_RequestNavigate;
        creditText.Inlines.Add(link);
        creditText.Visibility = Visibility.Visible;
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
            _imageCache[GetImageCacheKey(_words[_wordIndex])] = null;
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
        GameSoundButton.Content = SoundButton.Content;
        ListenButton.IsEnabled = _soundEnabled;
        MathReadNumberButton.IsEnabled = _soundEnabled;
        NumberQuizReadButton.IsEnabled = _soundEnabled;
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
        else if (GameZone.Visibility == Visibility.Visible
            && SnakeGamePanel.Visibility == Visibility.Visible
            && _snakeGame.State.IsRunning)
        {
            var requestedDirection = e.Key switch
            {
                Key.Up => (X: 0, Y: -1),
                Key.Down => (X: 0, Y: 1),
                Key.Left => (X: -1, Y: 0),
                Key.Right => (X: 1, Y: 0),
                _ => ((int X, int Y)?)null
            };
            if (requestedDirection is { } direction)
            {
                _snakeGame.SetDirection(direction.X, direction.Y);
                e.Handled = true;
            }
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

    protected override void OnClosed(EventArgs e)
    {
        StopSnakeGame();
        _pictureMatchGeneration++;
        _categoryLoadCancellation?.Cancel();
        _imageLoadCancellation?.Cancel();
        _categoryLoadCancellation?.Dispose();
        _imageLoadCancellation?.Dispose();
        _instructionReader.Stop();
        base.OnClosed(e);
    }
}
