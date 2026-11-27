using LetterGame.Domain.Models;

namespace LetterGame.Application.Models;

/// <summary>Zawiera wynik pobierania s³ów i informacjê o dostêpnoœci API oraz obrazów.</summary>
public sealed record WordCatalogResult(IReadOnlyList<WordEntry> Words, int ApiImageCount, bool ApiUnavailable = false);
