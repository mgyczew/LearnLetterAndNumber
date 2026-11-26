using System.Configuration;
using System.Net.Http;
using System.Windows;
using LetterGame.Application.Services;
using LetterGame.Infrastructure;

namespace LetterGame
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("SlowoTwory/1.0 (educational word game)");
            var catalog = new WordCatalogService(
                new WikipediaWordCatalogSource(httpClient),
                new WikimediaImageSource(httpClient));

            var window = new MainWindow(catalog, new WindowsSpeechReader());
            MainWindow = window;
            window.Show();
        }
    }

}
