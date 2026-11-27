namespace LetterGame.Application.Contracts;

/// <summary>Abstrakcja odczytywania instrukcji gry na g³os.</summary>
public interface IInstructionReader
{
    /// <summary>Odczytuje przekazany tekst, jeœli urz¹dzenie g³osowe jest dostêpne.</summary>
    void Speak(string text);

    /// <summary>Przerywa aktualnie odczytywan¹ instrukcjê.</summary>
    void Stop();
}
