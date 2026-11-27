using LetterGame.Domain.Models;

namespace LetterGame.Application.Models;

/// <summary>Pokazuje aktualny przedmiot, postêp wyprawy i aktywn¹ misjê specjaln¹.</summary>
public sealed record TreasureHuntGameState(
    int CurrentIndex,
    int FoundCount,
    int TotalWords,
    WordEntry? CurrentWord,
    string Mission,
    bool IsComplete);
