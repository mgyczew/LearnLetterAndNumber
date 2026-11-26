using System.Text.Json;
using LetterGame.Application.Contracts;
using LetterGame.Domain.Models;

namespace LetterGame.Infrastructure;

public sealed class WikimediaImageSource(HttpClient httpClient) : IWordImageSource
{
    public async Task<WordImage?> GetImageAsync(string searchTerm, CancellationToken cancellationToken)
    {
        var url = "https://commons.wikimedia.org/w/api.php?action=query&generator=search&gsrnamespace=6&gsrlimit=1&gsrsearch="
            + Uri.EscapeDataString(searchTerm)
            + "&prop=imageinfo&iiprop=url&iiurlwidth=480&format=json";

        using var response = await httpClient.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();
        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);

        if (!document.RootElement.TryGetProperty("query", out var query)
            || !query.TryGetProperty("pages", out var pages))
        {
            return null;
        }

        foreach (var page in pages.EnumerateObject())
        {
            if (!page.Value.TryGetProperty("imageinfo", out var imageInfo) || imageInfo.GetArrayLength() == 0)
            {
                continue;
            }

            var image = imageInfo[0];
            var imageUrl = image.TryGetProperty("thumburl", out var thumbnail) ? thumbnail.GetString() : null;
            var sourceUrl = image.TryGetProperty("descriptionurl", out var description) ? description.GetString() : null;
            if (imageUrl is not null && sourceUrl is not null)
            {
                return new WordImage(imageUrl, sourceUrl);
            }
        }

        return null;
    }
}
