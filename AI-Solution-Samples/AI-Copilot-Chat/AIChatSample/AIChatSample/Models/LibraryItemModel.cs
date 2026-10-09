using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace AIChatSample.Models
{
    /// <summary>One row in the Library / Knowledge Hub list.</summary>
    public enum LibraryItemType
    {
        Chat,
        Agent
    }

    public class LibraryItemModel : INotifyPropertyChanged
    {
        public string? Id { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? CreatedDate { get; set; }
        public string? ModifiedDate { get; set; }
        public LibraryItemType Type { get; set; }

        /// <summary>Font-glyph used for the row's left icon.</summary>
        public string? IconGlyph { get; set; }
        private string? image;

        public string? Image
        {
            get => this.image;
            set
            {
                if (this.image == value)
                {
                    return;
                }

                this.image = value;
                this.OnPropertyChanged();
            }
        }
        public string? IconFontFamily { get; set; }

        public bool IsAgent => Type == LibraryItemType.Agent;
        public bool IsChat => Type == LibraryItemType.Chat;

        public bool CanShare { get; set; }
        public bool CanEdit { get; set; }

        private bool isSelected;
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

        public string Subtitle =>
            string.IsNullOrEmpty(CreatedDate) || string.IsNullOrEmpty(ModifiedDate)
                ? string.Empty
                : $"{CreatedDate} • {ModifiedDate}";

        /// <summary>Text shown on the type badge ("Chat" / "Agent").</summary>
        public string BadgeText => Type == LibraryItemType.Agent ? "Agent" : "Chat";

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? p = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(p));
    }

    /// <summary>Display model for the filter chip group.</summary>
    public class LibraryFilterTab
    {
        public string? Text { get; set; }
        public LibraryFilter Filter { get; set; }
    }

    public enum LibraryFilter
    {
        All,
        Chats,
        Agents
    }
}