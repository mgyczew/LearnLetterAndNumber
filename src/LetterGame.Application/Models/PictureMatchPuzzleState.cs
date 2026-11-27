using LetterGame.Domain.Models;

namespace LetterGame.Application.Models;

/// <summary>Opisuje s³owo, dostêpne litery i postêp pojedynczej zagadki obrazkowej.</summary>
public sealed record PictureMatchPuzzleState(
    WordEntry Word,
    IReadOnlyList<int> SelectedLetterIndices,
    IReadOnlyList<int> LetterBankIndices,
    bool IsSolved);
