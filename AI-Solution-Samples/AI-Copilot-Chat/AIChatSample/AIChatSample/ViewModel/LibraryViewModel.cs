using System.Collections.ObjectModel;
using System.Windows.Input;
using AIChatSample.Models;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AIChatSample.ViewModel
{
    public partial class LibraryViewModel : ObservableObject
    {
        #region Fields

        [ObservableProperty] private string? searchText;
        [ObservableProperty] private bool isLoading;
        [ObservableProperty] private LibraryFilterTab? selectedFilterTab;

        // Two-way bound to SfListView.SelectedItem.
        // Tapping a row updates this; the partial method below reacts.
        [ObservableProperty] private LibraryItemModel? selectedLibraryItem;

        #endregion

        #region Collections

        public ObservableCollection<LibraryItemModel> LibraryItems { get; } = new();
        public ObservableCollection<LibraryItemModel> FilteredItems { get; } = new();
        public ObservableCollection<LibraryFilterTab> Filters { get; } = new();

        #endregion

        #region Commands

        public ICommand SearchCommand { get; }
        public ICommand FilterChangedCommand { get; }
        public ICommand OpenFilterCommand { get; }
        public ICommand OpenItemCommand { get; }
        public ICommand EditItemCommand { get; }
        public ICommand DeleteItemCommand { get; }
        public ICommand ShareItemCommand { get; }
        public ICommand OpenMoreMenuCommand { get; }

        #endregion

        public LibraryViewModel()
        {
            Filters.Add(new LibraryFilterTab { Text = "All", Filter = LibraryFilter.All });
            Filters.Add(new LibraryFilterTab { Text = "Chats", Filter = LibraryFilter.Chats });
            Filters.Add(new LibraryFilterTab { Text = "Agents", Filter = LibraryFilter.Agents });
            SelectedFilterTab = Filters[0];

            LibraryItems.Add(new LibraryItemModel
            {
                Id = "1",
                Title = "Banner images references for website home page",
                CreatedDate = "Mon",
                Image = Application.Current?.RequestedTheme != AppTheme.Dark ? "messageicon.png": "messageicondark.png",
                ModifiedDate = "Mon",
                Type = LibraryItemType.Chat,
                IconGlyph = "\ue73b",
                IconFontFamily = "UIKitIcons",
                CanShare = true,
                CanEdit = true
            });
            LibraryItems.Add(new LibraryItemModel
            {
                Id = "2",
                Title = "Banner images",
                CreatedDate = "Mon",
                ModifiedDate = "Mon",
                Image = Application.Current?.RequestedTheme != AppTheme.Dark ?  "messageicon.png": "messageicondark.png",
                Type = LibraryItemType.Chat,
                IconGlyph = "\ue73b",
                IconFontFamily = "UIKitIcons",
                CanShare = true,
                CanEdit = true
            });
            LibraryItems.Add(new LibraryItemModel
            {
                Id = "3",
                Title = "Data Analyst",
                CreatedDate = "Mon",
                ModifiedDate = "Mon",
                Image = Application.Current?.RequestedTheme != AppTheme.Dark ? "trendicon.png": "trendicondark.png",
                Type = LibraryItemType.Agent,
                IconGlyph = "\ue736",
                IconFontFamily = "MaterialAssets",
                CanShare = false,
                CanEdit = false
            });
            LibraryItems.Add(new LibraryItemModel
            {
                Id = "4",
                Title = "Idea Coach",
                CreatedDate = "Mon",
                ModifiedDate = "Mon",
                Image = Application.Current?.RequestedTheme != AppTheme.Dark ?  "bulbicon.png" : "bulbicondark.png",
                Type = LibraryItemType.Agent,
                IconGlyph = "\ue736",
                IconFontFamily = "MaterialAssets",
                CanShare = false,
                CanEdit = false
            });
            Application.Current!.RequestedThemeChanged += OnRequestedThemeChanged;
            SearchCommand = new Command(ApplyFilter);
            FilterChangedCommand = new Command(ApplyFilter);
            OpenFilterCommand = new Command(OnOpenFilter);
            OpenItemCommand = new Command<LibraryItemModel>(OnOpenItem);
            EditItemCommand = new Command<LibraryItemModel>(OnEditItem);
            DeleteItemCommand = new Command<LibraryItemModel>(OnDeleteItem);
            ShareItemCommand = new Command<LibraryItemModel>(OnShareItem);
            OpenMoreMenuCommand = new Command<LibraryItemModel>(OnOpenMoreMenu);

            ApplyFilter();
        }
        private void OnRequestedThemeChanged(object? sender, AppThemeChangedEventArgs e)
        {
            bool isDark = e.RequestedTheme == AppTheme.Dark;

            foreach (var item in LibraryItems)
            {
                switch (item.Id)
                {
                    case "1":
                    case "2":
                        item.Image = isDark
                            ? "messageicondark.png"
                            : "messageicon.png";
                        break;

                    case "3":
                        item.Image = isDark
                            ? "trendicondark.png"
                            : "trendicon.png";
                        break;

                    case "4":
                        item.Image = isDark
                            ? "bulbicondark.png"
                            : "bulbicon.png";
                        break;
                }
            }
        }

        partial void OnSearchTextChanged(string? value) => ApplyFilter();

        partial void OnSelectedFilterTabChanged(LibraryFilterTab? value) => ApplyFilter();

        partial void OnSelectedLibraryItemChanged(LibraryItemModel? value)
        {
            foreach (var item in LibraryItems)
                item.IsSelected = false;

            if (value is not null)
                value.IsSelected = true;
        }

        private void ApplyFilter()
        {
            SelectedLibraryItem = null;

            FilteredItems.Clear();

            var filter = SelectedFilterTab?.Filter ?? LibraryFilter.All;
            var q = (SearchText ?? string.Empty).Trim();

            foreach (var item in LibraryItems)
            {
                if (filter == LibraryFilter.Chats && !item.IsChat) continue;
                if (filter == LibraryFilter.Agents && !item.IsAgent) continue;

                if (!string.IsNullOrEmpty(q) &&
                    (item.Title?.Contains(q, StringComparison.OrdinalIgnoreCase) != true))
                    continue;

                FilteredItems.Add(item);
            }

            if (FilteredItems.Count > 0)
                SelectedLibraryItem = FilteredItems[0];
        }

        private void OnOpenFilter() {  }
        private void OnOpenItem(LibraryItemModel i) {  }
        private void OnEditItem(LibraryItemModel i) {  }
        private void OnDeleteItem(LibraryItemModel i) { LibraryItems.Remove(i); ApplyFilter(); }
        private void OnShareItem(LibraryItemModel i) {  }
        private void OnOpenMoreMenu(LibraryItemModel i) {  }
    }
}