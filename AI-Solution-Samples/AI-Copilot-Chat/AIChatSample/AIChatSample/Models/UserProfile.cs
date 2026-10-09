using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace AIChatSample.Models;

/// <summary>
/// App-wide shared user identity. Mutating this object notifies every
/// observer so profile edits propagate live (FR-3.3 / NFR-3).
/// </summary>
public sealed class UserProfile : INotifyPropertyChanged
{
    private string name = "Alexa John";
    private string email = string.Empty;
    private string color = "#6C4EC2";

    public string Name
    {
        get => name;
        set
        {
            if (name == value) return;
            name = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(Initial));
        }
    }

    public string Email
    {
        get => email;
        set
        {
            if (email == value) return;
            email = value;
            OnPropertyChanged();
        }
    }

    public string Color
    {
        get => color;
        set
        {
            if (color == value) return;
            color = value;
            OnPropertyChanged();
        }
    }

    /// <summary>First non-whitespace character of <see cref="Name"/>, upper-cased.</summary>
    public string Initial
    {
        get
        {
            var trimmed = (name ?? string.Empty).TrimStart();
            return trimmed.Length == 0 ? "?" : trimmed.Substring(0, 1).ToUpperInvariant();
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
