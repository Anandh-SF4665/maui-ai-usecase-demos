using Syncfusion.Maui.AIAssistView;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace AIChatSample.Models
{
    public class AgentItem : INotifyPropertyChanged
    {
        private bool isSelected;
        private string? initial;
        private string? color;
        private string? id;
        private string? avatarSource;
        private string? avatarGlyph;

        public string? Name { get; set; }
        public string? Glyph { get; set; }

        /// <summary>Stable id copied from <see cref="Models.AgentConfiguration.Id"/>; lets chat sessions / nav rows bind back to the same record (FR-1.3).</summary>
        public string? Id
        {
            get => this.id;
            set
            {
                if (this.id == value) return;
                this.id = value;
                this.OnPropertyChanged(nameof(this.Id));
            }
        }

        /// <summary>Single-character avatar initial derived from the agent name (FR-1.3).</summary>
        public string? Initial
        {
            get => this.initial;
            set
            {
                if (this.initial == value) return;
                this.initial = value;
                this.OnPropertyChanged(nameof(this.Initial));
            }
        }

        /// <summary>Avatar background color (hex) lifted from <see cref="Models.AgentConfiguration.Color"/> (FR-1.3).</summary>
        public string? Color
        {
            get => this.color;
            set
            {
                if (this.color == value) return;
                this.color = value;
                this.OnPropertyChanged(nameof(this.Color));
            }
        }

        public string? AvatarSource
        {
            get => this.avatarSource;
            set
            {
                if (this.avatarSource == value) return;
                this.avatarSource = value;
                this.OnPropertyChanged(nameof(this.AvatarSource));
            }
        }

        public string? AvatarGlyph
        {
            get => this.avatarGlyph;
            set
            {
                if (this.avatarGlyph == value) return;
                this.avatarGlyph = value;
                this.OnPropertyChanged(nameof(this.AvatarGlyph));
            }
        }

        public bool IsSelected
        {
            get => this.isSelected;
            set
            {
                if (this.isSelected == value) return;
                this.isSelected = value;
                this.OnPropertyChanged(nameof(this.IsSelected));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? p = null) =>
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(p));
    }
    public sealed class PulseScaleConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            var tick = value is int i ? i : 0;
            return (tick % 2) == 1 ? 1.12 : 1.0;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
    public sealed class LoadedScaleConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
            => value is bool b && b ? 1.0 : 0.85;

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
    public sealed class LoadedOpacityConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
            => value is bool b && b ? 1.0 : 0.0;

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
    public sealed class AgentItemTemplateSelector : DataTemplateSelector
    {
        public DataTemplate? RequestTemplate { get; set; }
        public DataTemplate? ResponseTemplate { get; set; }

        protected override DataTemplate? OnSelectTemplate(object item, BindableObject container)
        {
            if (item is AssistItem ai)
            {
                return ai.IsRequested ? RequestTemplate : ResponseTemplate;
            }
            return ResponseTemplate;
        }
    }
    public class RecentChatItem
    {
        public string? Title { get; set; }
        public string? Time { get; set; }
        public string? Glyph { get; set; }

        /// <summary>
        /// Carries the underlying <see cref="ChatSession"/> (or its id as a
        /// string) so the host view-model can resolve the row back to a
        /// stored session. Set by <c>AIMainLayoutViewModel</c> when
        /// materialising the Recent chats list (FR-0.4).
        /// </summary>
        public object? Tag { get; set; }
    }
}