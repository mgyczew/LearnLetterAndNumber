using LetterGame.Application.Models;
using LetterGame.Domain.Models;

namespace LetterGame.Application.Services;

/// <summary>Przechowuje stan uk³adania s³ów z kafelków liter na trzech poziomach.</summary>
public sealed class AlphabetGameSession(Random? random = null)
{
    private static readonly IReadOnlyDictionary<int, WordEntry[]> WordsByLevel = new Dictionary<int, WordEntry[]>
    {
        [1] =
        [
            Entry("KOT", "Zwierzê", "??", "Zwierzêta"), Entry("DOM", "Miejsce, w którym mieszkamy", "??", "Dom"),
            Entry("LAS", "Miejsce pe³ne drzew", "??", "Przyroda"), Entry("LIS", "Rudy mieszkaniec lasu", "??", "Zwierzêta"),
            Entry("SOK", "Napój z owoców", "??", "Jedzenie"), Entry("SER", "Produkt z mleka", "???", "Jedzenie"),
            Entry("NOS", "Czêœæ twarzy", "??", "Cia³o"), Entry("OKO", "Czêœæ twarzy, któr¹ patrzymy", "??", "Cia³o"),
            Entry("MAK", "Czerwony kwiat", "??", "Przyroda"), Entry("LODY", "Zimny deser", "???", "Jedzenie")
        ],
        [2] =
        [
            Entry("MLEKO", "Napój od krowy", "???", "Jedzenie"), Entry("BALON", "Mo¿na go nadmuchaæ", "??", "Zabawki"),
            Entry("KWIAT", "Roœnie w ogrodzie", "??", "Przyroda"), Entry("ROWER", "Ma dwa ko³a", "??", "Pojazdy"),
            Entry("¯ABA", "Zielone zwierzê, które skacze", "??", "Zwierzêta"), Entry("SZAFA", "Mebel na ubrania", "??", "Dom"),
            Entry("CZAPKA", "Zak³adamy j¹ na g³owê", "??", "Ubrania"), Entry("CHLEB", "Jemy go na kanapce", "???", "Jedzenie")
        ],
        [3] =
        [
            Entry("KSI¥¯KA", "Czytamy j¹", "??", "Rzeczy"), Entry("SZKO£A", "Miejsce, w którym siê uczymy", "??", "Miejsca"),
            Entry("WIEWIÓRKA", "Rude zwierzê, które lubi orzechy", "???", "Zwierzêta"), Entry("PARASOL", "Chroni przed deszczem", "??", "Rzeczy"),
            Entry("SAMOCHÓD", "Pojazd, którym jeŸdzimy", "??", "Pojazdy"), Entry("JAB£KO", "Okr¹g³y owoc", "??", "Owoce")
        ]
    };

    private static readonly string[] Digraphs = ["SZ", "CZ", "RZ", "CH"];
    private readonly Random _random = random ?? new Random();
    private readonly List<int> _selectedTileIndices = [];
    private int _level = 1;
    private WordEntry? _word;
    private string[] _letterTiles = [];
    private int _score;
    private bool _isSolved;
    private string? _previousWord;

    public AlphabetGameState State => new(
        _level,
        _word,
        _letterTiles.ToArray(),
        _selectedTileIndices.ToArray(),
        _score,
        _isSolved);

    /// <summary>Rozpoczyna now¹ grê na wybranym poziomie i zeruje wynik.</summary>
    public void Start(int level)
    {
        _score = 0;
        _previousWord = null;
        SetLevel(level);
    }

    /// <summary>Zmienia poziom, losuje s³owo i zachowuje dotychczasowy wynik.</summary>
    public void SetLevel(int level)
    {
        _level = WordsByLevel.ContainsKey(level) ? level : 1;
        ChooseWord();
    }

    /// <summary>Losuje kolejne s³owo z aktualnego poziomu.</summary>
    public void NextWord() => ChooseWord();

    /// <summary>Dodaje do odpowiedzi wskazany, jeszcze niewybrany kafelek.</summary>
    public bool SelectTile(int index)
    {
        if (_isSolved
            || index < 0
            || index >= _letterTiles.Length
            || _selectedTileIndices.Contains(index)
            || _selectedTileIndices.Count >= _letterTiles.Length)
        {
            return false;
        }

        _selectedTileIndices.Add(index);
        return true;
    }

    /// <summary>Usuwa ostatnio wybrany kafelek z odpowiedzi.</summary>
    public bool UndoSelection()
    {
        if (_isSolved || _selectedTileIndices.Count == 0)
        {
            return false;
        }

        _selectedTileIndices.RemoveAt(_selectedTileIndices.Count - 1);
        return true;
    }

    /// <summary>Usuwa wszystkie wybrane kafelki z bie¿¹cej odpowiedzi.</summary>
    public bool ClearSelection()
    {
        if (_isSolved || _selectedTileIndices.Count == 0)
        {
            return false;
        }

        _selectedTileIndices.Clear();
        return true;
    }

    /// <summary>Sprawdza u³o¿one s³owo i nalicza punkt za poprawn¹ odpowiedŸ.</summary>
    public WordAnswerResult CheckAnswer()
    {
        if (_word is null || _selectedTileIndices.Count != _letterTiles.Length)
        {
            return WordAnswerResult.Incomplete;
        }

        var answer = string.Concat(_selectedTileIndices.Select(index => _letterTiles[index]));
        if (!string.Equals(answer, _word.Text, StringComparison.OrdinalIgnoreCase))
        {
            return WordAnswerResult.Incorrect;
        }

        if (!_isSolved)
        {
            _score++;
            _isSolved = true;
        }

        return WordAnswerResult.Correct;
    }

    private void ChooseWord()
    {
        var candidates = WordsByLevel[_level];
        var nextCandidates = candidates
            .Where(word => !string.Equals(word.Text, _previousWord, StringComparison.OrdinalIgnoreCase))
            .ToArray();
        if (nextCandidates.Length > 0)
        {
            candidates = nextCandidates;
        }

        _word = candidates[_random.Next(candidates.Length)];
        _previousWord = _word.Text;
        _letterTiles = Tokenize(_word.Text)
            .OrderBy(_ => _random.Next())
            .ToArray();
        _selectedTileIndices.Clear();
        _isSolved = false;
    }

    private static IReadOnlyList<string> Tokenize(string word)
    {
        var tokens = new List<string>();
        for (var index = 0; index < word.Length;)
        {
            var digraph = Digraphs.FirstOrDefault(candidate =>
                index + candidate.Length <= word.Length
                && word.AsSpan(index, candidate.Length).Equals(candidate, StringComparison.OrdinalIgnoreCase));
            if (digraph is not null)
            {
                tokens.Add(digraph);
                index += digraph.Length;
            }
            else
            {
                tokens.Add(word[index].ToString());
                index++;
            }
        }

        return tokens;
    }

    private static WordEntry Entry(string text, string clue, string emoji, string category) =>
        new(text, clue, emoji, CategoryName: category);
}
