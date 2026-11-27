using System.Windows.Controls;
using System.Windows.Shapes;
using LetterGame.Domain.Models;

namespace LetterGame;

public partial class MainWindow
{
    private IReadOnlyList<WordEntry> _gameWords = [];
    private int _wordChainSessionVersion;
    private readonly System.Windows.Threading.DispatcherTimer _snakeTimer = new() { Interval = TimeSpan.FromMilliseconds(150) };
    private int _snakeGameVersion;
    private const int SnakeCellSize = 24;
    private readonly List<PictureMatchPuzzle> _pictureMatchPuzzles = [];
    private int _pictureMatchGeneration;
    private sealed class PictureMatchPuzzle(WordEntry word)
    {
        public WordEntry Word { get; } = word;
        public Image Image { get; set; } = null!;
        public TextBlock EmojiText { get; set; } = null!;
        public ItemsControl AnswerPanel { get; set; } = null!;
        public ItemsControl LetterBank { get; set; } = null!;
        public TextBlock StatusText { get; set; } = null!;
        public Button CheckButton { get; set; } = null!;
    }

    private sealed record PictureMatchLetter(PictureMatchPuzzle Puzzle, int Index);

}
