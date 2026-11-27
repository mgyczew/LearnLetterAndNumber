namespace LetterGame.Domain.Models;

/// <summary>Przechowuje adres u¿ywanego obrazka oraz stronê jego Ÿród³a.</summary>
public sealed record WordImage(string ImageUrl, string SourceUrl);
