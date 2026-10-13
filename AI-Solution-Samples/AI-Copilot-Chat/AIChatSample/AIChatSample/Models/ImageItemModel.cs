using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace AIChatSample.Models
{
    /// <summary>One image card in the Image Library / Media Gallery grid.</summary>
    public class ImageItemModel : INotifyPropertyChanged
    {
        public string? Id { get; set; }
        public string? ImagePath { get; set; }
        public string? ImageName { get; set; }
        public string? Tags { get; set; }
        public string? Category { get; set; }
        public string? Description { get; set; }
        public string? CreatedDate { get; set; }
        public string? ThumbnailPath { get; set; }

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

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? p = null) =>
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(p));
    }
}