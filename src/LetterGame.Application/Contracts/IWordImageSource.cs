using LetterGame.Domain.Models;

namespace LetterGame.Application.Contracts;

/// <summary>Abstrakcja wyszukiwania obrazów u¿ywanych jako wskazówki w grach.</summary>
public interface IWordImageSource
{
    /// <summary>Wyszukuje obraz i zwraca adres pliku oraz Ÿród³a lub null, gdy go nie znaleziono.</summary>
    Task<WordImage?> GetImageAsync(string searchTerm, CancellationToken cancellationToken);
}
