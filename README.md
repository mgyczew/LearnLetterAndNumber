# LearnLetterAndNumber

WPF-owa gra edukacyjna dla dzieci w wieku 6–7 lat. Zawiera uk³adanie s³ów, naukê zapisu liczb oraz æwiczenia z dzia³añ matematycznych.

## Struktura

- `LetterGame.csproj` — warstwa prezentacji WPF; widoki, obs³uga zdarzeñ i sk³adanie zale¿noœci w `App.xaml.cs`.
- `Architecture/LetterGame.Domain` — modele i regu³y domenowe, w tym generowanie pytañ matematycznych oraz zapis polskich nazw liczb.
- `Architecture/LetterGame.Application` — przypadki u¿ycia i stan sesji gier, katalog s³ów oraz kontrakty dla us³ug zewnêtrznych.
- `Architecture/LetterGame.Infrastructure` — integracje z Wikipedi¹/Wikimedia i synteza mowy.

Zale¿noœci kieruj¹ siê do œrodka: `Application` korzysta z `Domain`, a `Infrastructure` implementuje kontrakty `Application`. Projekt WPF pe³ni rolê composition root i ³¹czy warstwy.

## Budowanie

```powershell
dotnet build LetterGame.sln
```
