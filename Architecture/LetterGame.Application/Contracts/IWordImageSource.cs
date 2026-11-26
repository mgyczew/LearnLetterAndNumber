using LetterGame.Domain.Models;

namespace LetterGame.Application.Contracts;

public interface IWordImageSource
{
    Task<WordImage?> GetImageAsync(string searchTerm, CancellationToken cancellationToken);
}
