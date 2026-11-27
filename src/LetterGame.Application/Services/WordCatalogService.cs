using System.Net.Http;
using System.Text.Json;
using LetterGame.Application.Contracts;
using LetterGame.Application.Models;
using LetterGame.Domain.Catalog;
using LetterGame.Domain.Models;

namespace LetterGame.Application.Services;

/// <summary>ㄨczy lokalne s這wnictwo z dostawcami s堯w i obraz闚, zachowuj鉍 dzia豉nie offline.</summary>
public sealed class WordCatalogService(IWordCatalogSource wordSource, IWordImageSource imageSource)
{
    /// <summary>Kategorie i podstawowe s這wa dost瘼ne bez po章czenia z internetem.</summary>
    public IReadOnlyList<WordCategory> Categories => WordCategories.All;

    /// <summary>Zwraca unikalne, poprawne s這wa z katalogu do wykorzystania w grach.</summary>
    public IReadOnlyList<WordEntry> GetGameWords(IEnumerable<WordEntry> additionalWords) => additionalWords
        .Concat(Categories.SelectMany(category => category.OfflineWords))
        .Where(word => word.Text.All(char.IsLetter) && word.Text.Length is >= 3 and <= 14)
        .DistinctBy(word => word.Text, StringComparer.OrdinalIgnoreCase)
        .ToArray();

    /// <summary>Pobiera s這wa kategorii i uzupe軟ia je danymi offline, gdy API jest niedost瘼ne.</summary>
    public async Task<WordCatalogResult> GetWordsAsync(WordCategory category, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<WordEntry> apiWords;
        var apiUnavailable = false;
        try
        {
            apiWords = await wordSource.GetWordsAsync(category, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception) when (exception is HttpRequestException or TaskCanceledException or JsonException or InvalidOperationException)
        {
            apiWords = [];
            apiUnavailable = true;
        }

        var fallbackTerms = category.OfflineWords
            .Select(word => word.Text)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        var fallbackImages = category.OfflineWords
            .Where(word => word.ImageUrl is not null)
            .ToDictionary(word => word.Text, word => word.ImageUrl, StringComparer.OrdinalIgnoreCase);
        var apiWordsByText = apiWords
            .GroupBy(word => word.Text, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.First(), StringComparer.OrdinalIgnoreCase);
        var enrichedOfflineWords = category.OfflineWords
            .Select(word => apiWordsByText.TryGetValue(word.Text, out var apiWord)
                ? word with
                {
                    ImageUrl = word.ImageUrl ?? apiWord.ImageUrl,
                    SourceUrl = word.SourceUrl ?? apiWord.SourceUrl
                }
                : word);
        var words = enrichedOfflineWords
            .Concat(apiWords.Where(word => !fallbackTerms.Contains(word.Text)))
            .DistinctBy(word => word.Text, StringComparer.OrdinalIgnoreCase)
            .ToArray();
        var apiImageCount = apiWords.Count(word =>
            word.ImageUrl is not null
            && (!fallbackImages.TryGetValue(word.Text, out var fallbackImage)
                || !string.Equals(word.ImageUrl, fallbackImage, StringComparison.OrdinalIgnoreCase)));

        return new WordCatalogResult(words, apiImageCount, apiUnavailable);
    }

    /// <summary>Zwraca zapisany obraz albo wyszukuje go na podstawie has豉 s這wa.</summary>
    public Task<WordImage?> GetImageAsync(WordEntry word, CancellationToken cancellationToken = default)
    {
        if (word.ImageUrl is not null)
        {
            return Task.FromResult<WordImage?>(new WordImage(word.ImageUrl, word.SourceUrl ?? word.ImageUrl));
        }

        return word.ImageSearchTerm is null
            ? Task.FromResult<WordImage?>(null)
            : imageSource.GetImageAsync(word.ImageSearchTerm, cancellationToken);
    }
}
