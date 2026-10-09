using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace AIChatSample.Models
{
    public class SidebarItem : INotifyPropertyChanged
    {
        private bool isSelected;

        public string? Title { get; set; }
        public string? IconGlyph { get; set; }
        public string? IconFontFamily { get; set; }
        public string? PageName { get; set; }
        private FontImageSource? iconImageSource;
        public FontImageSource? IconImageSource
        {
            get => this.iconImageSource;
            set
            {
                if (this.iconImageSource == value)
                {
                    return;
                }

                this.iconImageSource = value;
                this.OnPropertyChanged();
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
}