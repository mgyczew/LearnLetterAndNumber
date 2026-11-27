using LetterGame.Domain.Models;

namespace LetterGame.Application.Models;

public sealed record PictureMatchPuzzleState(
    WordEntry Word,
    IReadOnlyList<int> SelectedLetterIndices,
    IReadOnlyList<int> LetterBankIndices,
    bool IsSolved);
