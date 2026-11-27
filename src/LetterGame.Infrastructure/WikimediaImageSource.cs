using System.Text;
using System.Text.Json;
using LetterGame.Application.Contracts;
using LetterGame.Domain.Models;

namespace LetterGame.Infrastructure;

/// <summary>Wyszukuje obrazy na Wikimedia Commons i zwraca ich adres wraz ze stron¹ Ÿród³ow¹.</summary>
public sealed class WikimediaImageSource(HttpClient httpClient) : IWordImageSource
{
    public async Task<WordImage?> GetImageAsync(string searchTerm, CancellationToken cancellationToken)
    {
        var url = "https://commons.wikimedia.org/w/api.php?action=query&generator=search&gsrnamespace=6&gsrlimit=10&gsrsearch="
            + Uri.EscapeDataString(searchTerm)
            + "&prop=imageinfo&iiprop=url&iiurlwidth=480&format=json";

        using var response = await httpClient.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();
        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);

        if (!document.RootElement.TryGetProperty("query", out var query)
            || query.ValueKind != JsonValueKind.Object
            || !query.TryGetProperty("pages", out var pages)
            || pages.ValueKind != JsonValueKind.Object)
        {
            return null;
        }

        var wordToken = GetTokens(searchTerm).FirstOrDefault();
        if (wordToken is null)
        {
            return null;
        }

        foreach (var page in pages.EnumerateObject())
        {
            if (!page.Value.TryGetProperty("title", out var titleElement)
                || titleElement.ValueKind != JsonValueKind.String
                || !GetTokens(titleElement.GetString() ?? string.Empty).Contains(wordToken, StringComparer.OrdinalIgnoreCase)
                || !page.Value.TryGetProperty("imageinfo", out var imageInfo)
                || imageInfo.ValueKind != JsonValueKind.Array
                || imageInfo.GetArrayLength() == 0)
            {
                continue;
            }

            var image = imageInfo[0];
            if (image.ValueKind != JsonValueKind.Object)
            {
                continue;
            }

            var imageUrl = GetStringProperty(image, "thumburl") ?? GetStringProperty(image, "url");
            var sourceUrl = GetStringProperty(image, "descriptionurl");
            if (imageUrl is not null && sourceUrl is not null)
            {
                return new WordImage(imageUrl, sourceUrl);
            }
        }

        return null;
    }

    private static string? GetStringProperty(JsonElement element, string propertyName) =>
        element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String
            ? property.GetString()
            : null;

    private static IEnumerable<string> GetTokens(string value)
    {
        var token = new StringBuilder();
        foreach (var character in value.Normalize(NormalizationForm.FormC))
        {
            if (char.IsLetterOrDigit(character))
            {
                token.Append(character);
                continue;
            }

            if (token.Length > 0)
            {
                yield return token.ToString();
                token.Clear();
            }
        }

        if (token.Length > 0)
        {
            yield return token.ToString();
        }
    }
}
