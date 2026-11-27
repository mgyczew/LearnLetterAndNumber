using System.Net;
using LetterGame.Infrastructure;

namespace LetterGame.Tests;

public sealed class WikimediaImageSourceTests
{
    [Fact]
    public async Task Skips_results_without_the_word_in_the_file_title()
    {
        const string responseJson = """
            {
              "query": {
                "pages": {
                  "1": {
                    "title": "File:Dog.jpg",
                    "imageinfo": [{ "thumburl": "https://example.test/dog.jpg", "descriptionurl": "https://example.test/dog" }]
                  },
                  "2": {
                    "title": "File:Domestic_Kot.jpg",
                    "imageinfo": [{ "thumburl": "https://example.test/cat.jpg", "descriptionurl": "https://example.test/cat" }]
                  }
                }
              }
            }
            """;
        using var httpClient = new HttpClient(new StubHttpMessageHandler(responseJson));
        var imageSource = new WikimediaImageSource(httpClient);

        var image = await imageSource.GetImageAsync("KOT animal", CancellationToken.None);

        Assert.NotNull(image);
        Assert.Equal("https://example.test/cat.jpg", image.ImageUrl);
    }

    [Fact]
    public async Task Returns_no_image_when_no_title_matches_the_word()
    {
        const string responseJson = """
            {
              "query": {
                "pages": {
                  "1": {
                    "title": "File:Animal.jpg",
                    "imageinfo": [{ "thumburl": "https://example.test/animal.jpg", "descriptionurl": "https://example.test/animal" }]
                  }
                }
              }
            }
            """;
        using var httpClient = new HttpClient(new StubHttpMessageHandler(responseJson));
        var imageSource = new WikimediaImageSource(httpClient);

        var image = await imageSource.GetImageAsync("KOT animal", CancellationToken.None);

        Assert.Null(image);
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
