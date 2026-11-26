namespace LetterGame.Domain.Models;

public sealed record WordEntry(
    string Text,
    string Clue,
    string Emoji,
    string? ImageUrl = null,
    string? SourceUrl = null,
    string? ImageSearchTerm = null);
