using CopilotChat.Services;
using CopilotChat.ViewModels;

namespace CopilotChat.Views;

public partial class CreateAgentPage : ContentPage
{
    private readonly CreateAgentViewModel viewModel;

    public CreateAgentPage(CreateAgentViewModel viewModel, ChatDataService dataService)
    {
        InitializeComponent();
        this.viewModel = viewModel;
        BindingContext = viewModel;

        viewModel.SaveCommand = new Command(async () => await SaveAndReturnAsync());
    }

    /// <summary>
    /// Persist the new agent and return to the main page.
    /// </summary>
    public async Task SaveAndReturnAsync()
    {
        if (string.IsNullOrWhiteSpace(viewModel.Name))
        {
            await DisplayAlert("Agent", "Please enter an agent name.", "OK");
            return;
        }

        viewModel.CreateAgent();
        await Shell.Current.GoToAsync("..");
    }
}
