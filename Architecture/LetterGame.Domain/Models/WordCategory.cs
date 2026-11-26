namespace LetterGame.Domain.Models;

public sealed record WordCategory(
    string Name,
    string WikiCategory,
    string Emoji,
    string ImageSearchHint,
    IReadOnlyList<WordEntry> OfflineWords);
