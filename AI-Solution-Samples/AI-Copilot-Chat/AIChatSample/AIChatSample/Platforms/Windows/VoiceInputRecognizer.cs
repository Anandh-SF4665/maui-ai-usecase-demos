#if WINDOWS
using Windows.Media.SpeechRecognition;

namespace AIChatSample.Services;

/// <summary>
/// Windows-specific voice input backed by
/// <c>Windows.Media.SpeechRecognition.SpeechRecognizer</c> (the WinRT
/// runtime API available in WinUI 3 / MAUI on Windows). Falls back to
/// the typed-prompt default when no recognizer is installed on the
/// device or the user denies microphone access.
/// </summary>
public partial class VoiceInputRecognizer
{
    public override bool IsNativeRecognitionSupported => true;

    public override async Task<string?> RecognizeAsync(string? existingText, CancellationToken ct = default)
    {
        try
        {
            using var recognizer = new SpeechRecognizer();
            var constraint = new SpeechRecognitionTopicConstraint(
                SpeechRecognitionScenario.Dictation, "dictation");
            recognizer.Constraints.Add(constraint);
            await recognizer.CompileConstraintsAsync();

            // Fire a one-shot recognition so the user can dictate one
            // utterance. The UI's IsListening state is shown while we
            // wait, and the recognized text is appended to the editor.
            var session = recognizer.RecognizeAsync();
            using var reg = ct.Register(() =>
            {
                try { recognizer.Cancel(); } catch { /* ignore */ }
            });
            var result = await session;
            if (result?.Status != SpeechRecognitionResultStatus.Success) return existingText;
            var recognized = result.Text;
            if (string.IsNullOrWhiteSpace(recognized)) return existingText;
            return string.IsNullOrEmpty(existingText)
                ? recognized
                : (existingText + " " + recognized).Trim();
        }
        catch
        {
            return await base.RecognizeAsync(existingText, ct);
        }
    }
}
#endif
