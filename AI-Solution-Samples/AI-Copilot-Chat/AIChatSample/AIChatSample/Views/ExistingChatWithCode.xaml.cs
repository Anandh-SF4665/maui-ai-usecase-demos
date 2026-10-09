namespace AIChatSample.Views.AIChat;

using Syncfusion.Maui.AIAssistView;

public partial class ExistingChatWithCode : ContentPage
{
    public ExistingChatWithCode()
    {
        InitializeComponent();

        Color sfAIAssistViewBackground = Color.FromArgb("#FFFFFF");
        Color sfAIAssistViewRequestItemBackground = Color.FromArgb("#F7EDFF");
        if (Application.Current != null)
        {
            sfAIAssistViewBackground = Application.Current.UserAppTheme == AppTheme.Dark
            ? (Color?)Application.Current.Resources["BackgroundColorDark"] ?? Color.FromArgb("#1C1B1D")
            : (Color?)Application.Current.Resources["BackgroundColorLight"] ?? Color.FromArgb("#FFFFFF");

            sfAIAssistViewRequestItemBackground = Application.Current.UserAppTheme == AppTheme.Dark
            ? (Color?)Application.Current.Resources["ReceivedMessageColorDark"] ?? Color.FromArgb("#35303C")
            : (Color?)Application.Current.Resources["ReceivedMessageColorLight"] ?? Color.FromArgb("#F7EDFF");
        }

        ResourceDictionary dictionary = new ResourceDictionary();
        dictionary.Add("SfAIAssistViewTheme", "CustomTheme");
        dictionary.Add("SfAIAssistViewBackground", sfAIAssistViewBackground);

        dictionary.Add("SfAIAssistViewRequestItemBackground", sfAIAssistViewRequestItemBackground);
        dictionary.Add("SfAIAssistViewRequestItemFontFamily", "Roboto-Regular");
        dictionary.Add("SfAIAssistViewRequestItemFontSize", 14);
        dictionary.Add("SfAIAssistViewRequestItemAuthorFontFamily", "Roboto-Regular");
        dictionary.Add("SfAIAssistViewRequestItemAuthorFontSize", 14);

        dictionary.Add("SfAIAssistViewResponseItemBackground", sfAIAssistViewBackground);
        dictionary.Add("SfAIAssistViewResponseItemFontFamily", "Roboto-Regular");
        dictionary.Add("SfAIAssistViewResponseItemFontSize", 14);
        dictionary.Add("SfAIAssistViewResponseItemAuthorFontFamily", "Roboto-Regular");
        dictionary.Add("SfAIAssistViewResponseItemAuthorFontSize", 14);

        this.Resources.Add(dictionary);

    }

    private void AndroidMenuTapped(object? sender, EventArgs e)
    {
#if ANDROID
        AIChatNavigationPanel.ToggleCompact();
#endif
    }
}