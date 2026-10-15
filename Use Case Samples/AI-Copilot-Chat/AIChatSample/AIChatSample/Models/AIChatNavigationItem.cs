using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.Maui.Graphics;

namespace AIChatSample.Models;

public sealed class AIChatNavigationItem : INotifyPropertyChanged
{
    public required string Key { get; init; }
    public required string Title { get; init; }
    public required string Icon { get; init; }
    public required string IconFontFamily { get; init; }

    private Color backgroundColor = Colors.Transparent;

    public Color BackgroundColor
    {
        get => backgroundColor;
        set
        {
            if (backgroundColor == value)
            {
                return;
            }

            backgroundColor = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(BackgroundColor)));
        }
    }

    public bool IsHovered { get; set; }

    public event PropertyChangedEventHandler? PropertyChanged;
}
