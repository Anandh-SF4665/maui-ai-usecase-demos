using CopilotChat.Services;

namespace CopilotChat.Views;

public partial class LibraryView : ContentView
{
    public LibraryView()
    {
        InitializeComponent();

        var dataService = IPlatformApplication.Current?.Services.GetService<ChatDataService>();
        BindingContext = new LibraryViewModel(dataService!);
    }
}

/// <summary>
/// Exposes library data for the Library page.
/// </summary>
public class LibraryViewModel
{
    public LibraryViewModel(ChatDataService dataService)
    {
        DataStore = dataService;
    }

    public ChatDataService DataStore { get; }
}
