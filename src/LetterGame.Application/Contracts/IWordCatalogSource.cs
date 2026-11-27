using LetterGame.Domain.Models;

namespace LetterGame.Application.Contracts;

/// <summary>Abstrakcja Ÿród³a s³ów pobieranych dla wskazanej kategorii.</summary>
public interface IWordCatalogSource
{
    /// <summary>Pobiera s³owa kategorii; anulowanie jest przekazywane do Ÿród³a.</summary>
    Task<IReadOnlyList<WordEntry>> GetWordsAsync(WordCategory category, CancellationToken cancellationToken);
}
