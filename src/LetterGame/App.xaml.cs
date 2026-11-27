using System.Net.Http;
using System.Windows;
using LetterGame.Application.Services;
using LetterGame.Domain.Services;
using LetterGame.Infrastructure;

namespace LetterGame
{
    /// <summary>Composition root aplikacji: tworzy adaptery i sesje, a następnie uruchamia główne okno.</summary>
    public partial class App : System.Windows.Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(20) };
            httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(
                "LetterGame/1.0 (https://github.com/mgyczew/LearnLetterAndNumber)");
            var catalog = new WordCatalogService(
                new WikipediaWordCatalogSource(httpClient),
                new WikimediaImageSource(httpClient));

            var mathLearning = new MathLearningService(new MathQuestionGenerator());
            var wordPuzzle = new WordPuzzleSession();
            var wordChainGame = new WordChainGameService();
            var numberGame = new NumberGameSession();
            var snakeGame = new SnakeGameSession();
            var pictureMatchGame = new PictureMatchGameSession();
            var numberQuiz = new NumberQuizService();
            var alphabetGame = new AlphabetGameSession();
            var treasureHuntGame = new TreasureHuntGameSession();
            var wordBingoGame = new WordBingoGameSession();
            var speechReader = new WindowsSpeechReader();
            var window = new MainWindow(
                catalog,
                speechReader,
                mathLearning,
                wordPuzzle,
                numberQuiz,
                wordChainGame,
                numberGame,
                snakeGame,
                pictureMatchGame,
                alphabetGame,
                treasureHuntGame,
                wordBingoGame);
            window.Closed += (_, _) =>
            {
                speechReader.Dispose();
                httpClient.Dispose();
            };
            MainWindow = window;
            window.Show();
        }
    }

}
