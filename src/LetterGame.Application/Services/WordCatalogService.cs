using System.Net.Http;
using System.Text.Json;
using LetterGame.Application.Contracts;
using LetterGame.Application.Models;
using LetterGame.Domain.Catalog;
using LetterGame.Domain.Models;

namespace LetterGame.Application.Services;

public sealed class WordCatalogService(IWordCatalogSource wordSource, IWordImageSource imageSource)
{
    public IReadOnlyList<WordCategory> Categories => WordCategories.All;

    public IReadOnlyList<WordEntry> GetGameWords(IEnumerable<WordEntry> additionalWords) => Categories
        .SelectMany(category => category.OfflineWords)
        .Concat(additionalWords)
        .Where(word => word.Text.Length is >= 3 and <= 14 && word.Text.All(char.IsLetter))
        .DistinctBy(word => word.Text, StringComparer.OrdinalIgnoreCase)
        .ToArray();

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

    public Task<WordImage?> GetImageAsync(WordEntry word, CancellationToken cancellationToken = default)
    {
        if (word.ImageUrl is not null && word.SourceUrl is not null)
        {
            return Task.FromResult<WordImage?>(new WordImage(word.ImageUrl, word.SourceUrl));
        }

        return word.ImageSearchTerm is null
            ? Task.FromResult<WordImage?>(null)
            : imageSource.GetImageAsync(word.ImageSearchTerm, cancellationToken);
    }
}
