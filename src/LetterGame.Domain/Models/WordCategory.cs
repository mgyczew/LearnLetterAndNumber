namespace LetterGame.Domain.Models;

/// <summary>Opisuje kategoriê s³ów wraz z danymi do pobierania i lokalnym zestawem awaryjnym.</summary>
public sealed record WordCategory(
    string Name,
    string WikiCategory,
    string Emoji,
    string ImageSearchHint,
    IReadOnlyList<WordEntry> OfflineWords);
