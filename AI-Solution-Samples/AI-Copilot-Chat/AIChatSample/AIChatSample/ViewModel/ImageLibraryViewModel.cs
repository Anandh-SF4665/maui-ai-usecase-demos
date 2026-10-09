using System.Collections.ObjectModel;
using System.Windows.Input;
using AIChatSample.Models;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AIChatSample.ViewModel
{
    public partial class ImageLibraryViewModel : ObservableObject
    {
        #region Fields

        [ObservableProperty] private string? searchText;
        [ObservableProperty] private bool isLoading;

        [ObservableProperty] private ImageItemModel? selectedImage;

        #endregion

        #region Collections

        public ObservableCollection<ImageItemModel> ImageLibraryItems { get; } = new();
        public ObservableCollection<ImageItemModel> FilteredImages { get; } = new();

        #endregion

        #region Derived

        public bool HasImages => ImageLibraryItems.Count > 0;

        #endregion

        #region Commands

        public ICommand SearchCommand { get; }
        public ICommand ImageSelectedCommand { get; }
        public ICommand OpenImageCommand { get; }
        public ICommand DeleteImageCommand { get; }
        public ICommand UploadImageCommand { get; }
        public ICommand RefreshLibraryCommand { get; }

        #endregion

        public ImageLibraryViewModel()
        {
            ImageLibraryItems.Add(new ImageItemModel
            {
                Id = "1",
                ImageName = "Mountain Lake",
                Category = "Landscape",
                ImagePath = "library1.png",
                ThumbnailPath = "background.png",
                Tags = "landscape, nature, outdoors",
                CreatedDate = "Mon",
                Description = "A wide landscape shot used as a background reference."
            });
            ImageLibraryItems.Add(new ImageItemModel
            {
                Id = "2",
                ImageName = "Abstract women Artwork",
                Category = "Artwork",
                ImagePath = "library2.png",
                ThumbnailPath = "review1.png",
                Tags = "abstract, portrait, art",
                CreatedDate = "Mon",
                Description = "Abstract portrait artwork sample card."
            });
            ImageLibraryItems.Add(new ImageItemModel
            {
                Id = "3",
                ImageName = "Portrait women",
                Category = "Portrait",
                ImagePath = "library3.png",
                ThumbnailPath = "review2.png",
                Tags = "portrait, photography, people",
                CreatedDate = "Mon",
                Description = "Portrait photography sample card."
            });
            ImageLibraryItems.Add(new ImageItemModel
            {
                Id = "4",
                ImageName = "Fox Photography",
                Category = "Animals",
                ImagePath = "library4.png",
                ThumbnailPath = "review3.png",
                Tags = "fox, wildlife, photography",
                CreatedDate = "Mon",
                Description = "Fox photography sample card."
            });
            ImageLibraryItems.Add(new ImageItemModel
            {
                Id = "5",
                ImageName = "Studio women",
                Category = "Portrait",
                ImagePath = "library5.png",
                ThumbnailPath = "product.png",
                Tags = "studio, portrait, people",
                CreatedDate = "Mon",
                Description = "Studio portrait sample card."
            });
            ImageLibraryItems.Add(new ImageItemModel
            {
                Id = "6",
                ImageName = "Food Bowl Photography",
                Category = "Food",
                ImagePath = "library6.png",
                ThumbnailPath = "map.png",
                Tags = "food, bowl, photography",
                CreatedDate = "Mon",
                Description = "Food bowl photography sample card."
            });
            ImageLibraryItems.Add(new ImageItemModel
            {
                Id = "7",
                ImageName = "Professional men",
                Category = "Portrait",
                ImagePath = "library7.png",
                ThumbnailPath = "connecticon.png",
                Tags = "professional, portrait, people",
                CreatedDate = "Mon",
                Description = "Professional portrait sample card."
            });
            ImageLibraryItems.Add(new ImageItemModel
            {
                Id = "8",
                ImageName = "Food Plate Photography",
                Category = "Food",
                ImagePath = "library8.png",
                ThumbnailPath = "standup.png",
                Tags = "food, plate, photography",
                CreatedDate = "Mon",
                Description = "Food plate photography sample card."
            });

            SearchCommand = new Command(ApplyFilter);
            ImageSelectedCommand = new Command<ImageItemModel>(OnImageSelected);
            OpenImageCommand = new Command<ImageItemModel>(OnOpenImage);
            DeleteImageCommand = new Command<ImageItemModel>(OnDeleteImage);
            UploadImageCommand = new Command(OnUploadImage);
            RefreshLibraryCommand = new Command(OnRefreshLibrary);

            ApplyFilter();
        }

        partial void OnSearchTextChanged(string? value) => ApplyFilter();

        private void ApplyFilter()
        {
            FilteredImages.Clear();

            var q = (SearchText ?? string.Empty).Trim();
            foreach (var img in ImageLibraryItems)
            {
                if (string.IsNullOrEmpty(q) ||
                    (img.ImageName?.Contains(q, StringComparison.OrdinalIgnoreCase) == true) ||
                    (img.Tags?.Contains(q, StringComparison.OrdinalIgnoreCase) == true) ||
                    (img.Category?.Contains(q, StringComparison.OrdinalIgnoreCase) == true) ||
                    (img.Description?.Contains(q, StringComparison.OrdinalIgnoreCase) == true))
                {
                    FilteredImages.Add(img);
                }
            }

            OnPropertyChanged(nameof(HasImages));
        }

        private void OnImageSelected(ImageItemModel? item)
        {
            if (item is null) return;

            foreach (var img in ImageLibraryItems)
                img.IsSelected = false;

            item.IsSelected = true;
            SelectedImage = item;
        }

        private void OnOpenImage(ImageItemModel i) {  }
        private void OnDeleteImage(ImageItemModel i)
        {
            ImageLibraryItems.Remove(i);
            ApplyFilter();
        }
        private void OnUploadImage() {  }
        private void OnRefreshLibrary() {  }
    }
}