using LetterGame.Application.Models;
using LetterGame.Domain.Models;

namespace LetterGame.Application.Services;

/// <summary>Obs³uguje postêp domowego polowania na przedmioty i rotacjê misji specjalnych.</summary>
public sealed class TreasureHuntGameSession
{
    private static readonly WordEntry[] Words =
    [
        new("STÓ£", "ZnajdŸ stó³", "??", CategoryName: "Dom"), new("OKNO", "ZnajdŸ okno", "??", CategoryName: "Dom"),
        new("DRZWI", "ZnajdŸ drzwi", "??", CategoryName: "Dom"), new("LAMPA", "ZnajdŸ lampê", "??", CategoryName: "Dom"),
        new("KRZES£O", "ZnajdŸ krzes³o", "??", CategoryName: "Dom"), new("£Ó¯KO", "ZnajdŸ ³ó¿ko", "???", CategoryName: "Dom"),
        new("LODÓWKA", "ZnajdŸ lodówkê", "??", CategoryName: "Dom"),
        new("ZEGAR", "ZnajdŸ zegar", "??", CategoryName: "Dom"), new("LUSTRO", "ZnajdŸ lustro", "??", CategoryName: "Dom"),
        new("DYWAN", "ZnajdŸ dywan", "??", CategoryName: "Dom"), new("KUBEK", "ZnajdŸ kubek", "?", CategoryName: "Dom"),
        new("SZAFA", "ZnajdŸ szafê", "??", CategoryName: "Dom")
    ];

    private static readonly string[] Missions =
    [
        "WeŸ kubek i po³ó¿ go na stole.",
        "Dotknij krzes³a, a potem wska¿ okno.",
        "ZnajdŸ lampê i powiedz, do czego s³u¿y.",
        "PodejdŸ do drzwi i wróæ na miejsce."
    ];

    private int _currentIndex;
    private int _foundCount;
    private int _missionIndex;

    public TreasureHuntGameState State => new(
        _currentIndex,
        _foundCount,
        Words.Length,
        _currentIndex < Words.Length ? Words[_currentIndex] : null,
        Missions[_missionIndex],
        _currentIndex >= Words.Length);

    /// <summary>Rozpoczyna wyprawê od pierwszego s³owa i pierwszej misji.</summary>
    public void Start()
    {
        _currentIndex = 0;
        _foundCount = 0;
        _missionIndex = 0;
    }

    /// <summary>Oznacza bie¿¹cy przedmiot jako znaleziony i przechodzi do nastêpnego.</summary>
    public bool MarkFound()
    {
        if (_currentIndex >= Words.Length)
        {
            return false;
        }

        _foundCount++;
        _currentIndex++;
        return true;
    }

    /// <summary>Pomija bie¿¹cy przedmiot bez zwiêkszania liczby znalezionych skarbów.</summary>
    public bool SkipWord()
    {
        if (_currentIndex >= Words.Length)
        {
            return false;
        }

        _currentIndex++;
        return true;
    }

    /// <summary>Wyœwietla nastêpn¹ misjê specjaln¹, wracaj¹c do pierwszej po ostatniej.</summary>
    public void NextMission() => _missionIndex = (_missionIndex + 1) % Missions.Length;
}
