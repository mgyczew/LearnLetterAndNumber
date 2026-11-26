namespace LetterGame.Application.Contracts;

public interface IInstructionReader
{
    void Speak(string text);
    void Stop();
}
