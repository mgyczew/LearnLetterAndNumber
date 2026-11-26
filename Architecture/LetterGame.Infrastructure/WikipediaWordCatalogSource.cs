using System.Text.Json;
using LetterGame.Application.Contracts;
using LetterGame.Domain.Models;

namespace LetterGame.Infrastructure;

public sealed class WikipediaWordCatalogSource(HttpClient httpClient) : IWordCatalogSource
{
    private sealed record PageImage(string? ThumbnailUrl, string ArticleUrl);

    public async Task<IReadOnlyList<WordEntry>> GetWordsAsync(WordCategory category, CancellationToken cancellationToken)
    {
        var titles = category.OfflineWords
            .Where(word => word.ImageUrl is null)
            .Select(word => word.Text)
            .Where(title => title.Length is >= 3 and <= 12 && title.All(char.IsLetter))
            .Distinct(StringComparer.CurrentCultureIgnoreCase)
            .ToArray();

        var pageImages = await FetchPageImagesAsync(titles, cancellationToken);
        return category.OfflineWords.Select(word =>
        {
            pageImages.TryGetValue(word.Text, out var image);
            return word with
            {
                ImageUrl = word.ImageUrl ?? image?.ThumbnailUrl,
                SourceUrl = word.SourceUrl ?? image?.ArticleUrl
            };
        }).ToArray();
    }

    private async Task<Dictionary<string, PageImage>> FetchPageImagesAsync(string[] titles, CancellationToken cancellationToken)
    {
        var images = new Dictionary<string, PageImage>(StringComparer.CurrentCultureIgnoreCase);
        foreach (var titleBatch in titles.Chunk(50))
        {
            var url = "https://pl.wikipedia.org/w/api.php?action=query&prop=pageimages%7Cinfo&inprop=url"
                + "&piprop=thumbnail&pithumbsize=420&titles="
                + Uri.EscapeDataString(string.Join("|", titleBatch))
                + "&format=json";

            using var response = await httpClient.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();
            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);

            if (!document.RootElement.TryGetProperty("query", out var query)
                || !query.TryGetProperty("pages", out var pages))
            {
                continue;
            }

            foreach (var page in pages.EnumerateObject())
            {
                var pageData = page.Value;
                if (!pageData.TryGetProperty("title", out var titleElement)
                    || !pageData.TryGetProperty("canonicalurl", out var article))
                {
                    continue;
                }

                var title = titleElement.GetString();
                var articleUrl = article.GetString();
                var thumbnailUrl = pageData.TryGetProperty("thumbnail", out var thumbnail)
                    && thumbnail.TryGetProperty("source", out var source)
                    ? source.GetString()
                    : null;

                if (title is not null && articleUrl is not null)
                {
                    images[title] = new PageImage(thumbnailUrl, articleUrl);
                }
            }
        }

        return images;
    }
}
