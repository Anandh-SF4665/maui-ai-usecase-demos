namespace AIChatSample.Services;

/// <summary>
/// Cross-platform voice-to-text input. The default implementation
/// provides a text-prompt fallback so the mic icon does something
/// meaningful on every target platform, and platform-specific overrides
/// in <c>Platforms/</c> can replace it with native speech-to-text
/// (Windows.Media.SpeechRecognition, Android SpeechRecognizer, iOS
/// AVSpeechRecognizer, etc.) when the corresponding runtime is available.
/// </summary>
public partial class VoiceInputRecognizer
{
    /// <summary>
    /// True when the platform's native speech-to-text is wired up. False
    /// means the recognizer will fall back to a typed-input prompt so the
    /// feature remains usable everywhere.
    /// </summary>
    public virtual bool IsNativeRecognitionSupported => false;

    /// <summary>
    /// Captures voice input and returns the recognized text. The optional
    /// <paramref name="existingText"/> is concatenated with the result so
    /// successive recordings append to the editor instead of replacing it.
    /// Implementations should respect cancellation via the caller passing
    /// a CancellationToken where applicable.
    /// </summary>
    public virtual async Task<string?> RecognizeAsync(string? existingText, CancellationToken ct = default)
    {
        // Default fallback: prompt the user for the text the mic would
        // have produced. This keeps the feature working on every platform
        // and during automated UI tests where no microphone is available.
        var page = Microsoft.Maui.Controls.Application.Current?.Windows
            .FirstOrDefault()?.Page;
        if (page is null) return existingText;
        var prompt = await page.DisplayPromptAsync(
            title: "Voice input",
            message: "Speak or type your message. Recognized text will be added to the editor.",
            accept: "Use",
            cancel: "Cancel",
            placeholder: "Type the message…",
            initialValue: existingText ?? string.Empty,
            keyboard: Microsoft.Maui.Keyboard.Text);
        return prompt;
    }
}
