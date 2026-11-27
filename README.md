# LearnLetterAndNumber

WPF-owa gra edukacyjna dla dzieci w wieku 6–7 lat. Zawiera uk³adanie s³ów, naukê zapisu liczb oraz æwiczenia z dzia³añ matematycznych.

## Struktura

- `src/LetterGame` — prezentacja WPF: widoki w XAML oraz obs³uga interakcji w klasach `MainWindow`. `App.xaml.cs` jest composition root: tworzy konkretne us³ugi i przekazuje je do okna.
- `src/LetterGame.Domain` — modele i regu³y niezale¿ne od WPF i integracji, w tym kategorie s³ów, generowanie pytañ matematycznych oraz zapis polskich nazw liczb.
- `src/LetterGame.Application` — przypadki u¿ycia, stan i regu³y sesji gier oraz kontrakty dla katalogu s³ów, obrazów i mowy. Zale¿y wy³¹cznie od `Domain`.
- `src/LetterGame.Infrastructure` — implementacje kontraktów Application: pobieranie treœci i obrazów z Wikipedii/Wikimedia oraz synteza mowy Windows.
- `tests/LetterGame.Tests` — testy regu³ i sesji bez uruchamiania WPF ani kontaktu z us³ugami zewnêtrznymi.

Lista s³ów jest kuratorowana lokalnie. Obecny adapter Wikipedii uzupe³nia te s³owa o miniatury i linki do Ÿróde³; nie pobiera nieprzefiltrowanych hase³ z kategorii, co pozwala kontrolowaæ s³ownictwo przeznaczone dla dzieci.

Zale¿noœci kieruj¹ siê do œrodka:

```text
Domain <- Application <- Infrastructure
             ^               ^
             ??? WPF app ?????
```

`Domain` nie powinien odwo³ywaæ siê do pozosta³ych projektów. `Application` nie powinien znaæ `Infrastructure` ani WPF. Integracje zewnêtrzne dodawaj przez kontrakt w `Application` i implementacjê w `Infrastructure`. Projekt WPF mo¿e znaæ te warstwy, poniewa¿ sk³ada aplikacjê i wyœwietla jej stan.

## Co robi¹ gry

- **S³owo po s³owie** — uk³adanie jednego s³owa z rozsypanych liter; obraz, kategoria i opis pomagaj¹ znaleŸæ odpowiedŸ.
- **Wê¿yk literowy** — zbieranie liter we w³aœciwej kolejnoœci, aby u³o¿yæ s³owo.
- **Dopasuj obrazki** — uk³adanie nazw kilku obrazków z liter.
- **Ruchomy alfabet (gra 4)** — uk³adanie s³ów na trzech poziomach; `sz`, `cz`, `rz` i `ch` s¹ osobnymi kafelkami.
- **Poszukiwanie skarbów (gra 5)** — dziecko czyta s³owo, szuka pasuj¹cego przedmiotu w domu i oznacza go jako znaleziony; misje specjalne dodaj¹ proste zadania ruchowe.
- **S³owne bingo (gra 6)** — jedna z czterech plansz 3×3, losowane i odczytywane s³owa oraz zaznaczanie pól do uzyskania linii.
- **Gry liczbowe** — liczenie obiektów, wskazywanie cyfry ze s³uchu oraz uk³adanie liczby.
- **Æwiczenia matematyczne** — rozpoznawanie liczb i proste dzia³ania.

Karty i ekrany gier powinny krótko wyjaœniaæ cel oraz g³ówne dzia³anie. Regu³y i zmiany stanu trzymaj w sesjach Application; kod WPF powinien przede wszystkim prezentowaæ stan i przekazywaæ akcje u¿ytkownika. Jeœli `MainWindow` bêdzie dalej rozrastaæ siê, kolejnym krokiem jest wydzielenie ViewModels dla ekranów.

## Budowanie

```powershell
dotnet build LetterGame.sln
```

## Testy

```powershell
dotnet test LetterGame.sln
```
