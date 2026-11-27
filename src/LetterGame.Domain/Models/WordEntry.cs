namespace LetterGame.Domain.Models;

/// <summary>Reprezentuje s³owo wraz z podpowiedzi¹, kategori¹ i opcjonalnymi danymi obrazka.</summary>
public sealed record WordEntry(
    string Text,
    string Clue,
    string Emoji,
    string? ImageUrl = null,
    string? SourceUrl = null,
    string? ImageSearchTerm = null,
    string? CategoryName = null);
