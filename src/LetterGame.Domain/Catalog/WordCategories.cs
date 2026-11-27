using LetterGame.Domain.Models;

namespace LetterGame.Domain.Catalog;

/// <summary>Udostêpnia sta³e kategorie i s³owa dostêpne bez po³¹czenia z internetem.</summary>
public static class WordCategories
{
    /// <summary>Wszystkie domyœlne kategorie wykorzystywane przez aplikacjê.</summary>
    public static IReadOnlyList<WordCategory> All { get; } =
    [
        Create("Owoce", "Kategoria:Owoce", "OWOCE", "fruit", "Owoc", "JAB£KO", "GRUSZKA", "ŒLIWKA", "WIŒNIA", "CZEREŒNIA", "MORELA", "BRZOSKWINIA", "BANAN", "POMARAÑCZA", "CYTRYNA", "LIMONKA", "MANDARYNKA", "ANANAS", "ARBUZ", "MELON", "WINOGRONA", "MALINA", "JE¯YNA", "BORÓWKA", "TRUSKAWKA", "PORZECZKA", "AGREST", "KIWI", "MANGO", "PAPAJA", "GRANAT", "FIGA", "DAKTYL", "KOKOS", "PIGWA"),
        Create("Warzywa", "Kategoria:Warzywa", "WARZYWA", "vegetable", "Warzywo", "MARCHEW", "ZIEMNIAK", "POMIDOR", "OGÓREK", "PAPRYKA", "CEBULA", "CZOSNEK", "SA£ATA", "KAPUSTA", "BURAK", "RZODKIEWKA", "FASOLA", "GROSZEK", "KUKURYDZA", "BROKU£", "KALAFIOR", "DYNIA", "CUKINIA", "BAK£A¯AN", "POR", "SZPINAK", "SELER", "PIETRUSZKA", "RZEPA", "SZPARAG", "KOPER", "KABACZEK", "BATAT", "SOCZEWICA"),
        Create("Zwierzêta", "Kategoria:Zwierzêta", "ZWIERZÊTA", "animal", "Zwierzê", "KOT", "PIES", "LIS", "WILK", "LEW", "TYGRYS", "ZEBRA", "KOÑ", "KURA", "KROWA", "ŒWINIA", "OWCA", "KOZA", "KACZKA", "GÊŒ", "RYBA", "¯ABA", "¯Ó£W", "JE¯", "SARNA", "£OŒ", "BÓBR", "WYDRA", "MA£PA", "S£OÑ", "¯YRAFA", "PINGWIN", "DELFIN", "REKIN", "MOTYL", "PSZCZO£A"),
        Create("Kolory", "Kategoria:Kolory", "KOLORY", "color swatch", "Kolor", "BIA£Y", "CZARNY", "CZERWONY", "NIEBIESKI", "ZIELONY", "¯Ó£TY", "POMARAÑCZOWY", "FIOLETOWY", "RÓ¯OWY", "BR¥ZOWY", "SZARY", "GRANATOWY", "TURKUSOWY", "Z£OTY", "SREBRNY", "BE¯OWY", "KREMOWY", "B£ÊKITNY", "RUDY", "MIÊTOWY"),
        Create("Kszta³ty", "Kategoria:Figury geometryczne", "KSZTA£TY", "geometric shape", "Kszta³t", "KO£O", "KWADRAT", "TRÓJK¥T", "PROSTOK¥T", "ROMB", "OWAL", "KULA", "SZEŒCIAN", "STO¯EK", "WALEC", "ELIPSA", "PIRAMIDA", "CYLINDER", "TRAPEZ", "PÓ£KOLE", "PIÊCIOK¥T", "SZEŒCIOK¥T", "LINIA", "PUNKT", "£UK"),
        Create("Jedzenie", "Kategoria:Potrawy", "JEDZENIE", "food", "Jedzenie", "CHLEB", "BU£KA", "MAS£O", "SER", "JAJKO", "MLEKO", "JOGURT", "ZUPA", "RY¯", "MAKARON", "PIZZA", "TORT", "CIASTO", "LÓD", "MIÓD", "D¯EM", "KANAPKA", "NALEŒNIK", "KLOPS", "KASZA", "P£ATKI", "PARÓWKA", "SA£ATKA", "CUKIER", "SÓL", "PIERÓG", "BUDYÑ", "SOK", "WODA", "LODY", "HERBATA", "KAKAO", "CUKIEREK"),
        Create("Dom", "Kategoria:Wyposa¿enie wnêtrz", "DOM", "house interior", "Rzecz w domu", "DOM", "DACH", "OKNO", "DRZWI", "STÓ£", "KRZES£O", "£Ó¯KO", "LAMPKA", "LUSTRO", "KUBEK", "TALERZ", "£Y¯KA", "WIDELEC", "GARNEK", "PATELNIA", "KANAPA", "FOTEL", "DYWAN", "ZEGAR", "SZAFKA", "£Ó¯KO", "PÓ£KA", "RÊCZNIK", "MYD£O", "KOC", "PODUSZKA", "LODÓWKA", "KUCHNIA", "MISKA", "SZKLANKA", "WIESZAK", "ZAS£ONA", "SZUFLADA"),
        Create("Przyroda", "Kategoria:Przyroda", "PRZYRODA", "nature", "Element przyrody", "S£OÑCE", "KSIÊ¯YC", "GWIAZDA", "NIEBO", "CHMURA", "DESZCZ", "ŒNIEG", "WIATR", "DRZEWO", "KWIAT", "LIŒÆ", "TRAWA", "LAS", "RZEKA", "MORZE", "GÓRA", "KAMIEÑ", "PIASEK", "ZIEMIA", "WODA", "OGIEÑ", "LÓD", "TÊCZA", "NOC", "DZIEÑ", "KRZEW", "KORA", "POLANA"),
        CreateFlags()
    ];

    private static WordCategory Create(string name, string wikiCategory, string emoji, string imageSearchHint, string cluePrefix, params string[] words)
    {
        var categoryEmoji = name switch
        {
            "Owoce" => "??",
            "Warzywa" => "??",
            "Zwierzêta" => "??",
            "Kolory" => "??",
            "Kszta³ty" => "??",
            "Jedzenie" => "???",
            "Dom" => "??",
            "Przyroda" => "??",
            _ => emoji
        };
        var entries = words
            .Select(word => new WordEntry(word, $"{cluePrefix} z kategorii {name.ToLowerInvariant()}", categoryEmoji, ImageSearchTerm: $"{word} {imageSearchHint}", CategoryName: name))
            .ToArray();

        return new WordCategory(name, wikiCategory, categoryEmoji, imageSearchHint, entries);
    }

    private static WordCategory CreateFlags()
    {
        var countries = new (string Name, string Code)[]
        {
            ("POLSKA", "pl"), ("NIEMCY", "de"), ("FRANCJA", "fr"), ("W£OCHY", "it"), ("HISZPANIA", "es"),
            ("UKRAINA", "ua"), ("USA", "us"), ("JAPONIA", "jp"), ("CHINY", "cn"), ("BRAZYLIA", "br"),
            ("KANADA", "ca"), ("GRECJA", "gr"), ("NORWEGIA", "no"), ("SZWECJA", "se"), ("FINLANDIA", "fi"),
            ("CZECHY", "cz"), ("S£OWACJA", "sk"), ("AUSTRALIA", "au"), ("TURCJA", "tr"), ("MEKSYK", "mx"),
            ("IRLANDIA", "ie"), ("PORTUGALIA", "pt"), ("EGIPT", "eg"), ("INDIE", "in"), ("ARGENTYNA", "ar"),
            ("ISLANDIA", "is"), ("MAROKO", "ma"), ("AUSTRIA", "at"), ("BELGIA", "be"), ("DANIA", "dk")
        };

        var words = countries
            .Select(country => new WordEntry(
                country.Name,
                $"Flaga pañstwa {country.Name.ToLowerInvariant()}",
                "???",
                $"https://flagcdn.com/w640/{country.Code}.png",
                "https://flagcdn.com/",
                CategoryName: "Flagi"))
            .ToArray();

        return new WordCategory("Flagi", "Kategoria:Flagi pañstw œwiata", "???", "national flag", words);
    }
}
