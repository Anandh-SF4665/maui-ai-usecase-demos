using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CopilotChat.Models;
using CopilotChat.Services;

namespace CopilotChat.ViewModels;

/// <summary>
/// View model for the Create New Agent flow (details + logo editor).
/// </summary>
public partial class CreateAgentViewModel : ObservableObject
{
    private readonly ChatDataService dataService;
    private readonly Func<Task>? saveCallback;

    [ObservableProperty]
    private string name = string.Empty;

    [ObservableProperty]
    private string description = string.Empty;

    [ObservableProperty]
    private string behaviour = string.Empty;

    [ObservableProperty]
    private string selectedColor = "#6750A4";

    [ObservableProperty]
    private ImageSource? uploadedImage;

    /// <summary>Available avatar background colors.</summary>
    public string[] Colors { get; } =
    {
        "#6750A4", "#0288D1", "#2E7D32", "#EF6C00", "#D32F2F", "#7B1FA2", "#00897B", "#5D4037"
    };

    /// <summary>First letter of the agent name for the avatar preview.</summary>
    public string Initial => string.IsNullOrWhiteSpace(Name) ? "?" : Name.Trim()[0].ToString().ToUpperInvariant();

    public ICommand SaveCommand { get; set; }

    public ICommand SelectColorCommand { get; }

    public ICommand UploadImageCommand { get; }

    public CreateAgentViewModel(ChatDataService dataService)
    {
        this.dataService = dataService;

        SelectColorCommand = new Command<string>(color =>
        {
            SelectedColor = color;
            UploadedImage = null;
        });
        UploadImageCommand = new Command(async () => await PickImageAsync());
    }

    /// <summary>
    /// Wires the save action to the page (validates and navigates back).
    /// </summary>
    public void Initialize(Func<Task> onSave)
    {
        // kept simple: page injects its save handler
    }

    partial void OnNameChanged(string value)
        => OnPropertyChanged(nameof(Initial));

    private async Task PickImageAsync()
    {
        try
        {
            var result = await FilePicker.Default.PickAsync(new PickOptions
            {
                PickerTitle = "Select a logo image",
                FileTypes = FilePickerFileType.Images
            });

            if (result != null)
            {
                UploadedImage = ImageSource.FromFile(result.FullPath);
            }
        }
        catch
        {
            // User cancelled the picker or permission denied.
        }
    }

    /// <summary>
    /// Creates the agent and adds it to the agent list.
    /// </summary>
    public Agent CreateAgent()
    {
        var agent = new Agent
        {
            Name = Name?.Trim() ?? string.Empty,
            Description = Description?.Trim() ?? string.Empty,
            Behaviour = string.IsNullOrWhiteSpace(Behaviour)
                ? "You are a helpful AI assistant."
                : Behaviour.Trim(),
            Color = SelectedColor,
            CreatedDate = DateTime.Now
        };

        dataService.Agents.Add(agent);
        return agent;
    }
}

/// <summary>
/// View model for the Profile page.
/// </summary>
public partial class ProfileViewModel : ObservableObject
{
    private readonly ChatDataService dataService;

    [ObservableProperty]
    private string name;

    [ObservableProperty]
    private string email;

    public ProfileViewModel(ChatDataService dataService)
    {
        this.dataService = dataService;
        Name = dataService.Profile.Name;
        Email = dataService.Profile.Email;
    }

    /// <summary>
    /// Persists profile changes.
    /// </summary>
    public void Save()
    {
        dataService.Profile.Name = Name?.Trim() ?? dataService.Profile.Name;
        dataService.Profile.Email = Email?.Trim() ?? dataService.Profile.Email;
    }
}
