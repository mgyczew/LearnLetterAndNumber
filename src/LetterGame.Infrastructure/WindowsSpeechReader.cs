using System.Reflection;
using System.Runtime.InteropServices;
using LetterGame.Application.Contracts;

namespace LetterGame.Infrastructure;

/// <summary>Odczytuje tekst na g³os przy u¿yciu Windows SAPI, jeœli synteza jest dostêpna.</summary>
public sealed class WindowsSpeechReader : IInstructionReader, IDisposable
{
    private object? _voice;

    public WindowsSpeechReader()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        try
        {
            var voiceType = Type.GetTypeFromProgID("SAPI.SpVoice");
            if (voiceType is not null)
            {
                _voice = Activator.CreateInstance(voiceType);
                SelectPolishVoice(voiceType);
            }
        }
        catch (COMException)
        {
        }
        catch (PlatformNotSupportedException)
        {
        }
    }

    public void Speak(string text)
    {
        if (_voice is null)
        {
            return;
        }

        try
        {
            InvokeSpeak(text, 1);
        }
        catch (COMException)
        {
        }
        catch (TargetInvocationException)
        {
        }
    }

    public void Stop()
    {
        if (_voice is null)
        {
            return;
        }

        try
        {
            InvokeSpeak(string.Empty, 2);
        }
        catch (COMException)
        {
        }
        catch (TargetInvocationException)
        {
        }
    }

    /// <summary>Zatrzymuje odczyt i zwalnia obiekt COM u¿ywany przez SAPI.</summary>
    public void Dispose()
    {
        if (_voice is null)
        {
            return;
        }

        Stop();
        try
        {
            if (OperatingSystem.IsWindows() && Marshal.IsComObject(_voice))
            {
                Marshal.FinalReleaseComObject(_voice);
            }
        }
        catch (COMException)
        {
        }
        finally
        {
            _voice = null;
        }
    }

    private void InvokeSpeak(string text, int flags)
    {
        _voice!.GetType().InvokeMember(
            "Speak",
            BindingFlags.InvokeMethod,
            binder: null,
            target: _voice,
            args: [text, flags]);
    }

    private void SelectPolishVoice(Type voiceType)
    {
        try
        {
            object? voices = voiceType.InvokeMember("GetVoices", BindingFlags.InvokeMethod, null, _voice, ["Language=0415", ""]);
            var count = GetVoiceCount(voices);
            if (count == 0)
            {
                voices = voiceType.InvokeMember("GetVoices", BindingFlags.InvokeMethod, null, _voice, ["Language=415", ""]);
                count = GetVoiceCount(voices);
            }

            if (voices is null)
            {
                return;
            }

            if (count > 0)
            {
                var polishVoice = voices.GetType().InvokeMember("Item", BindingFlags.InvokeMethod, null, voices, [0]);
                voiceType.InvokeMember("Voice", BindingFlags.SetProperty, null, _voice, [polishVoice]);
            }
        }
        catch (COMException)
        {
        }
        catch (TargetInvocationException)
        {
        }
    }

    private static int GetVoiceCount(object? voices)
    {
        return voices is null
            ? 0
            : (int)voices.GetType().InvokeMember("Count", BindingFlags.GetProperty, null, voices, null)!;
    }
}
