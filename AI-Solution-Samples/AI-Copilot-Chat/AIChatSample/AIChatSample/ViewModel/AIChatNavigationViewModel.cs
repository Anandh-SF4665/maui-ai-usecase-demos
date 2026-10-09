using System.Collections.ObjectModel;
using AIChatSample.Models;

namespace AIChatSample.ViewModel;

public sealed class AIChatNavigationViewModel
{
    public ObservableCollection<AIChatNavigationItem> PrimaryItems { get; } =
    [
        new AIChatNavigationItem { Key = "NewChat", Title = "New chat", Icon = "\ue70d", IconFontFamily = "MaterialAssets" },
        new AIChatNavigationItem { Key = "Search", Title = "Search", Icon = "\ue715", IconFontFamily = "MaterialAssets" },
        new AIChatNavigationItem { Key = "Library", Title = "Library", Icon = "\ue712", IconFontFamily = "MaterialAssets" }
    ];
}
