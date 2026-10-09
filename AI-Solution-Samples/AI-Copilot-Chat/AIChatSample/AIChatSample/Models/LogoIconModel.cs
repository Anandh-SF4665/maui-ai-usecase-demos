using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace AIChatSample.Models
{
    /// <summary>One selectable icon in the Agent Logo popup (Create mode).</summary>
    public class LogoIconModel : INotifyPropertyChanged
    {
        public string? Id { get; set; }
        public string? IconName { get; set; }
        public string? IconGlyph { get; set; }
        public string? IconFontFamily { get; set; }

        private bool isSelected;
        public bool IsSelected
        {
            get => this.isSelected;
            set { if (this.isSelected == value) return; this.isSelected = value; this.OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? p = null) =>
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(p));
    }

    /// <summary>One selectable color swatch in the Agent Logo popup (Create mode).</summary>
    public class LogoColorModel : INotifyPropertyChanged
    {
        public string? Id { get; set; }
        public string? ColorHex { get; set; }
        public string? ColorName { get; set; }

        private bool isSelected;
        public bool IsSelected
        {
            get => this.isSelected;
            set { if (this.isSelected == value) return; this.isSelected = value; this.OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? p = null) =>
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(p));
    }
}