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

    private static WordCatalogService CreateService(IWordCatalogSource source) =>
        new(source, new EmptyWordImageSource());

    private sealed class FailingWordCatalogSource : IWordCatalogSource
    {
        public Task<IReadOnlyList<WordEntry>> GetWordsAsync(WordCategory category, CancellationToken cancellationToken) =>
            Task.FromException<IReadOnlyList<WordEntry>>(new HttpRequestException());
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
}
