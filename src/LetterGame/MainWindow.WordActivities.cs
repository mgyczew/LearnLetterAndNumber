using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using LetterGame.Application.Models;
using LetterGame.Domain.Models;

namespace LetterGame;

public partial class MainWindow
{
    private void HideWordActivityPanels()
    {
        AlphabetGamePanel.Visibility = Visibility.Collapsed;
        TreasureHuntPanel.Visibility = Visibility.Collapsed;
        WordBingoPanel.Visibility = Visibility.Collapsed;
    }

    private void ShowWordActivityPanel(UIElement panel)
    {
        StopSnakeGame();
        _pictureMatchGeneration++;
        GameMenuPanel.Visibility = Visibility.Collapsed;
        WordChainPanel.Visibility = Visibility.Collapsed;
        SnakeGamePanel.Visibility = Visibility.Collapsed;
        PictureMatchPanel.Visibility = Visibility.Collapsed;
        NumberGamePanel.Visibility = Visibility.Collapsed;
        HideWordActivityPanels();
        panel.Visibility = Visibility.Visible;
    }

    private void StartAlphabetGame_Click(object sender, RoutedEventArgs e)
    {
        ShowWordActivityPanel(AlphabetGamePanel);
        _alphabetGame.Start(1);
        _alphabetGameVersion++;
        AlphabetStatusText.Text = string.Empty;
        RenderAlphabetGame();
    }

    private void AlphabetLevel_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: int level })
        {
            return;
        }

        _alphabetGame.SetLevel(level);
        _alphabetGameVersion++;
        AlphabetStatusText.Text = string.Empty;
        RenderAlphabetGame();
    }

    private void RenderAlphabetGame()
    {
        var state = _alphabetGame.State;
        AlphabetAnswerPanel.Items.Clear();
        AlphabetLetterBank.Items.Clear();
        if (state.Word is not { } word)
        {
            return;
        }

        AlphabetEmojiText.Text = word.Emoji;
        AlphabetCategoryText.Text = $"Poziom {state.Level} · {word.CategoryName}";
        AlphabetClueText.Text = word.Clue;
        for (var index = 0; index < state.LetterTiles.Count; index++)
        {
            var selectedTileIndex = index < state.SelectedTileIndices.Count
                ? state.SelectedTileIndices[index]
                : -1;
            AlphabetAnswerPanel.Items.Add(new Border
            {
                Width = 66,
                Height = 70,
                Margin = new Thickness(5),
                CornerRadius = new CornerRadius(14),
                BorderThickness = new Thickness(2),
                BorderBrush = new SolidColorBrush(Color.FromRgb(225, 230, 241)),
                Background = new SolidColorBrush(Color.FromRgb(248, 249, 253)),
                Child = new TextBlock
                {
                    Text = selectedTileIndex >= 0 ? state.LetterTiles[selectedTileIndex] : string.Empty,
                    FontSize = 25,
                    FontWeight = FontWeights.Bold,
                    Foreground = new SolidColorBrush(Color.FromRgb(39, 49, 78)),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                }
            });
        }

        for (var index = 0; index < state.LetterTiles.Count; index++)
        {
            var selected = state.SelectedTileIndices.Contains(index);
            var tileButton = new Button
            {
                Content = state.LetterTiles[index],
                Style = (Style)FindResource("LetterTile"),
                IsEnabled = !selected && !state.IsSolved,
                Opacity = selected ? 0.35 : 1,
                Tag = index
            };
            tileButton.Click += AlphabetLetter_Click;
            AlphabetLetterBank.Items.Add(tileButton);
        }

        AlphabetScoreText.Text = $"Punkty: {state.Score}";
        CheckAlphabetButton.IsEnabled = !state.IsSolved;
        NextAlphabetButton.IsEnabled = true;
    }

    private void AlphabetLetter_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: int index } && _alphabetGame.SelectTile(index))
        {
            AlphabetStatusText.Text = string.Empty;
            RenderAlphabetGame();
        }
    }

    private void UndoAlphabetLetter_Click(object sender, RoutedEventArgs e)
    {
        if (_alphabetGame.UndoSelection())
        {
            AlphabetStatusText.Text = string.Empty;
            RenderAlphabetGame();
        }
    }

    private void ClearAlphabetLetters_Click(object sender, RoutedEventArgs e)
    {
        if (_alphabetGame.ClearSelection())
        {
            AlphabetStatusText.Text = string.Empty;
            RenderAlphabetGame();
        }
    }

    private void CheckAlphabetWord_Click(object sender, RoutedEventArgs e)
    {
        var result = _alphabetGame.CheckAnswer();
        if (result != WordAnswerResult.Correct)
        {
            AlphabetStatusText.Text = result == WordAnswerResult.Incomplete
                ? "Wybierz wszystkie litery i spróbuj ponownie."
                : "Spróbuj jeszcze raz! Mo¿esz u¿yæ podpowiedzi.";
            AlphabetStatusText.Foreground = new SolidColorBrush(Color.FromRgb(204, 139, 43));
            return;
        }

        AlphabetStatusText.Text = "Brawo! Œwietnie u³o¿one s³owo!";
        AlphabetStatusText.Foreground = new SolidColorBrush(Color.FromRgb(38, 148, 105));
        RenderAlphabetGame();
        if (_soundEnabled && _alphabetGame.State.Word is { } word)
        {
            _instructionReader.Stop();
            _instructionReader.Speak($"Brawo! {word.Text.ToLowerInvariant()}.");
        }

        _ = AdvanceAlphabetWordAsync(_alphabetGameVersion);
    }

    private void NextAlphabetWord_Click(object sender, RoutedEventArgs e)
    {
        _alphabetGame.NextWord();
        _alphabetGameVersion++;
        AlphabetStatusText.Text = string.Empty;
        RenderAlphabetGame();
    }

    private async Task AdvanceAlphabetWordAsync(int completedWordVersion)
    {
        await Task.Delay(TimeSpan.FromSeconds(2));
        if (completedWordVersion == _alphabetGameVersion
            && AlphabetGamePanel.Visibility == Visibility.Visible
            && _alphabetGame.State.IsSolved)
        {
            NextAlphabetWord_Click(this, new RoutedEventArgs());
        }
    }

    private void ListenAlphabetWord_Click(object sender, RoutedEventArgs e)
    {
        if (_soundEnabled && _alphabetGame.State.Word is { } word)
        {
            _instructionReader.Stop();
            _instructionReader.Speak(word.Text.ToLowerInvariant());
        }
    }

    private void StartTreasureHunt_Click(object sender, RoutedEventArgs e)
    {
        ShowWordActivityPanel(TreasureHuntPanel);
        _treasureHuntGame.Start();
        TreasureStatusText.Text = string.Empty;
        RenderTreasureHunt();
    }

    private void RenderTreasureHunt()
    {
        var state = _treasureHuntGame.State;
        TreasureFoundCountText.Text = $"Znalezione: {state.FoundCount} z {state.TotalWords}";
        TreasureMissionText.Text = state.Mission;
        if (state.IsComplete || state.CurrentWord is not { } word)
        {
            TreasureProgressText.Text = "Wyprawa zakoñczona!";
            TreasureEmojiText.Text = "??";
            TreasureWordText.Text = "BRAWO!";
            FoundTreasureButton.IsEnabled = false;
            return;
        }

        TreasureProgressText.Text = $"Skarb {state.CurrentIndex + 1} z {state.TotalWords}";
        TreasureEmojiText.Text = word.Emoji;
        TreasureWordText.Text = word.Text;
        FoundTreasureButton.IsEnabled = true;
    }

    private void MarkTreasureFound_Click(object sender, RoutedEventArgs e)
    {
        if (_treasureHuntGame.MarkFound())
        {
            TreasureStatusText.Text = _treasureHuntGame.State.IsComplete
                ? "Wszystkie skarby znalezione! Œwietna robota!"
                : "Znaleziono! Szukamy nastêpnego skarbu.";
            TreasureStatusText.Foreground = new SolidColorBrush(Color.FromRgb(38, 148, 105));
            RenderTreasureHunt();
        }
    }

    private void SkipTreasureWord_Click(object sender, RoutedEventArgs e)
    {
        if (_treasureHuntGame.SkipWord())
        {
            TreasureStatusText.Text = "Szukamy nastêpnego s³owa.";
            TreasureStatusText.Foreground = new SolidColorBrush(Color.FromRgb(204, 139, 43));
            RenderTreasureHunt();
        }
    }

    private void NextTreasureMission_Click(object sender, RoutedEventArgs e)
    {
        _treasureHuntGame.NextMission();
        TreasureMissionText.Text = _treasureHuntGame.State.Mission;
    }

    private void ListenTreasureWord_Click(object sender, RoutedEventArgs e)
    {
        var state = _treasureHuntGame.State;
        if (_soundEnabled && state.CurrentWord is { } word)
        {
            _instructionReader.Stop();
            _instructionReader.Speak(word.Text.ToLowerInvariant());
        }
    }

    private void StartWordBingo_Click(object sender, RoutedEventArgs e)
    {
        ShowWordActivityPanel(WordBingoPanel);
        _wordBingoGame.Start(1);
        WordBingoStatusText.Text = string.Empty;
        RenderWordBingo();
    }

    private void SelectWordBingoBoard_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: int boardNumber })
        {
            return;
        }

        _wordBingoGame.Start(boardNumber);
        WordBingoStatusText.Text = string.Empty;
        RenderWordBingo();
    }

    private void RenderWordBingo()
    {
        var state = _wordBingoGame.State;
        WordBingoBoard.Children.Clear();
        WordBingoCalledWordText.Text = state.CurrentCall ?? "Naciœnij „Losuj s³owo”";
        WordBingoProgressText.Text = $"Plansza {state.BoardNumber} · s³owo {state.CallNumber} z {state.TotalCalls}";
        NextBingoCallButton.IsEnabled = !state.IsComplete;

        for (var index = 0; index < state.BoardWords.Count; index++)
        {
            var isMarked = state.MarkedIndices.Contains(index);
            var cell = new Button
            {
                Content = state.BoardWords[index],
                Width = 142,
                Height = 92,
                Margin = new Thickness(5),
                FontSize = 23,
                FontWeight = FontWeights.Bold,
                IsEnabled = !isMarked && !state.HasBingo,
                Style = (Style)FindResource(isMarked ? "SecondaryButton" : "PrimaryButton"),
                Tag = index
            };
            cell.Click += WordBingoCell_Click;
            WordBingoBoard.Children.Add(cell);
        }
    }

    private void NextBingoCall_Click(object sender, RoutedEventArgs e)
    {
        var word = _wordBingoGame.NextCall();
        if (word is null)
        {
            WordBingoStatusText.Text = _wordBingoGame.State.HasBingo
                ? "Bingo! Gratulacje!"
                : "Wszystkie s³owa zosta³y ju¿ wylosowane. Wybierz inn¹ planszê.";
            RenderWordBingo();
            return;
        }

        WordBingoStatusText.Text = "ZnajdŸ to s³owo na swojej planszy.";
        RenderWordBingo();
        if (_soundEnabled)
        {
            _instructionReader.Stop();
            _instructionReader.Speak(word.ToLowerInvariant());
        }
    }

    private void WordBingoCell_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: int index })
        {
            return;
        }

        if (!_wordBingoGame.MarkCurrentWord(index))
        {
            WordBingoStatusText.Text = _wordBingoGame.State.CurrentCall is { } currentCall
                ? $"Poszukaj s³owa: {currentCall}."
                : "Najpierw wylosuj s³owo.";
            WordBingoStatusText.Foreground = new SolidColorBrush(Color.FromRgb(204, 139, 43));
            return;
        }

        WordBingoStatusText.Text = _wordBingoGame.State.HasBingo
            ? "BINGO! Œwietna robota!"
            : "Dobrze! Czekaj na nastêpne s³owo.";
        WordBingoStatusText.Foreground = new SolidColorBrush(Color.FromRgb(38, 148, 105));
        RenderWordBingo();
    }

    private void ListenBingoCall_Click(object sender, RoutedEventArgs e)
    {
        if (_soundEnabled && _wordBingoGame.State.CurrentCall is { } word)
        {
            _instructionReader.Stop();
            _instructionReader.Speak(word.ToLowerInvariant());
        }
    }
}
