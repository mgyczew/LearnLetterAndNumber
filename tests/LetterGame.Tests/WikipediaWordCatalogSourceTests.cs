using System.Net;
using LetterGame.Domain.Models;
using LetterGame.Infrastructure;

namespace LetterGame.Tests;

public sealed class WikipediaWordCatalogSourceTests
{
    [Fact]
    public async Task Enriches_curated_words_with_wikipedia_image_and_article_links()
    {
        const string responseJson = """
            {
              "query": {
                "pages": {
                  "1": {
                    "title": "KOT",
                    "canonicalurl": "https://pl.wikipedia.org/wiki/Kot",
                    "thumbnail": { "source": "https://images.example.test/kot.jpg" }
                  }
                }
              }
            }
            """;
        using var httpClient = new HttpClient(new StubHttpMessageHandler(responseJson));
        var source = new WikipediaWordCatalogSource(httpClient);
        var category = new WordCategory(
            "Zwierzêta",
            "Kategoria:Zwierzêta",
            "??",
            "animal",
            [
                new WordEntry("KOT", "Zwierzê domowe", "??"),
                new WordEntry("LIS", "Zwierzê leœne", "??")
            ]);

        var words = await source.GetWordsAsync(category, CancellationToken.None);

        Assert.Equal(2, words.Count);
        Assert.Equal("https://images.example.test/kot.jpg", words[0].ImageUrl);
        Assert.Equal("https://pl.wikipedia.org/wiki/Kot", words[0].SourceUrl);
        Assert.Null(words[1].ImageUrl);
        Assert.Null(words[1].SourceUrl);
    }

    private sealed class StubHttpMessageHandler(string responseJson) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(responseJson)
            });
    }
}
