using LetterGame.Domain.Models;

namespace LetterGame.Application.Contracts;

public interface IWordCatalogSource
{
    Task<IReadOnlyList<WordEntry>> GetWordsAsync(WordCategory category, CancellationToken cancellationToken);
}
