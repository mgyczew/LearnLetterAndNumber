using System.Net.Http;
using LetterGame.Application.Contracts;
using LetterGame.Application.Services;
using LetterGame.Domain.Models;

namespace LetterGame.Tests;

public sealed class WordCatalogServiceTests
{
    [Fact]
    public async Task Uses_offline_words_and_marks_api_unavailable_when_source_fails()
    {
        var service = CreateService(new FailingWordCatalogSource());
        var category = service.Categories[0];

        var result = await service.GetWordsAsync(category);

        Assert.True(result.ApiUnavailable);
        Assert.NotEmpty(result.Words);
        Assert.Contains(category.OfflineWords[0].Text, result.Words.Select(word => word.Text));
    }

    [Fact]
    public async Task Propagates_caller_cancellation_instead_of_using_offline_fallback()
    {
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        var service = CreateService(new CanceledWordCatalogSource());

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => service.GetWordsAsync(service.Categories[0], cancellation.Token));
    }

    [Fact]
    public void Prefers_enriched_words_when_combining_game_words()
    {
        var service = CreateService(new FailingWordCatalogSource());
        var enrichedWord = new WordEntry("KOT", "Zwierzê", "??", "https://example.test/cat.jpg", "https://example.test/article");

        var selectedWord = service.GetGameWords([enrichedWord])
            .Single(word => string.Equals(word.Text, "KOT", StringComparison.OrdinalIgnoreCase));

        Assert.Equal(enrichedWord, selectedWord);
    }

    [Fact]
    public async Task Keeps_an_assigned_image_when_its_source_url_is_missing()
    {
        var service = CreateService(new FailingWordCatalogSource());
        var word = new WordEntry("KOT", "Zwierzê", "??", "https://example.test/cat.jpg");

        var image = await service.GetImageAsync(word);

        Assert.NotNull(image);
        Assert.Equal(word.ImageUrl, image.ImageUrl);
    }

    [Fact]
    public async Task Merges_enriched_offline_words_and_additional_words_from_the_source()
    {
        var category = new WordCatalogService(new FailingWordCatalogSource(), new EmptyWordImageSource()).Categories[0];
        var offlineWord = category.OfflineWords.First(word => word.ImageUrl is null);
        var enrichedWord = offlineWord with
        {
            ImageUrl = "https://example.test/image.jpg",
            SourceUrl = "https://example.test/article"
        };
        var additionalWord = new WordEntry("NOWEHAS£O", "Dodatkowe s³owo", "?");
        var source = new StaticWordCatalogSource([enrichedWord, additionalWord]);
        var service = CreateService(source);

        var result = await service.GetWordsAsync(category);
        var mergedWord = result.Words.Single(word => word.Text == offlineWord.Text);

        Assert.False(result.ApiUnavailable);
        Assert.Equal(offlineWord.Clue, mergedWord.Clue);
        Assert.Equal(enrichedWord.ImageUrl, mergedWord.ImageUrl);
        Assert.Contains(result.Words, word => word.Text == additionalWord.Text);
    }

    [Fact]
    public async Task Delegates_image_search_terms_to_the_image_source()
    {
        var image = new WordImage("https://example.test/image.jpg", "https://example.test/source");
        var imageSource = new CapturingWordImageSource(image);
        var service = new WordCatalogService(new FailingWordCatalogSource(), imageSource);

        var result = await service.GetImageAsync(new WordEntry("KOT", "Zwierzê", "??", ImageSearchTerm: "kot"));

        Assert.Equal("kot", imageSource.SearchTerm);
        Assert.Equal(image, result);
    }

    private static WordCatalogService CreateService(IWordCatalogSource source) =>
        new(source, new EmptyWordImageSource());

    private sealed class FailingWordCatalogSource : IWordCatalogSource
    {
        public Task<IReadOnlyList<WordEntry>> GetWordsAsync(WordCategory category, CancellationToken cancellationToken) =>
            Task.FromException<IReadOnlyList<WordEntry>>(new HttpRequestException());
    }

    private sealed class StaticWordCatalogSource(IReadOnlyList<WordEntry> words) : IWordCatalogSource
    {
        public Task<IReadOnlyList<WordEntry>> GetWordsAsync(WordCategory category, CancellationToken cancellationToken) =>
            Task.FromResult(words);
    }

    private sealed class CanceledWordCatalogSource : IWordCatalogSource
    {
        public Task<IReadOnlyList<WordEntry>> GetWordsAsync(WordCategory category, CancellationToken cancellationToken) =>
            Task.FromCanceled<IReadOnlyList<WordEntry>>(cancellationToken);
    }

    private sealed class EmptyWordImageSource : IWordImageSource
    {
        public Task<WordImage?> GetImageAsync(string searchTerm, CancellationToken cancellationToken) =>
            Task.FromResult<WordImage?>(null);
    }

    private sealed class CapturingWordImageSource(WordImage image) : IWordImageSource
    {
        public string? SearchTerm { get; private set; }

        public Task<WordImage?> GetImageAsync(string searchTerm, CancellationToken cancellationToken)
        {
            SearchTerm = searchTerm;
            return Task.FromResult<WordImage?>(image);
        }
    }
}
