namespace AIChatSample.Controls;

public partial class AITempChat : ContentView
{
    private bool spanUpdatePending;

    public AITempChat()
    {
        InitializeComponent();

        Color sfAIAssistViewBackground = Color.FromArgb("#FCF8FA");
        if (Application.Current != null)
        {
            sfAIAssistViewBackground = Application.Current.UserAppTheme == AppTheme.Dark
                ? (Color?)Application.Current.Resources["SentMessageColorDark"] ?? Color.FromArgb("#313032")
                : (Color?)Application.Current.Resources["SentMessageColorLight"] ?? Color.FromArgb("#FCF8FA");
        }

        ResourceDictionary dictionary = new ResourceDictionary
        {
            { "SfAIAssistViewTheme", "CustomTheme" },
            { "SfAIAssistViewBackground", sfAIAssistViewBackground },
            { "SfAIAssistViewHeaderSuggestionBackground", Color.FromArgb("#FCF8FA") }
        };
        Resources.Add(dictionary);
    }

    private void AndroidMenuTapped(object? sender, EventArgs e)
    {
#if ANDROID
        AIChatNavigationPanel.ToggleCompact();
#endif
    }

    private void SuggestionsLayoutSizeChanged(object? sender, EventArgs e)
    {
        if (sender is not CollectionView suggestionsLayout ||
            suggestionsLayout.ItemsLayout is not GridItemsLayout gridLayout)
        {
            return;
        }

        var targetSpan = suggestionsLayout.Width < 560 ? 1 : 2;
        if (gridLayout.Span == targetSpan || spanUpdatePending)
        {
            return;
        }

        spanUpdatePending = true;
        Dispatcher.Dispatch(() =>
        {
            spanUpdatePending = false;
            if (suggestionsLayout.ItemsLayout is GridItemsLayout currentLayout &&
                currentLayout.Span != targetSpan)
            {
                currentLayout.Span = targetSpan;
            }
        });
    }
}