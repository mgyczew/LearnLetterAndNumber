using LetterGame.Domain.Models;

namespace LetterGame.Application.Models;

public sealed record WordCatalogResult(IReadOnlyList<WordEntry> Words, int ApiImageCount, bool ApiUnavailable = false);
