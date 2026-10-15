using System.Diagnostics;
using AIRatingFeedback.Services;

namespace AIRatingFeedback;

public partial class MainPage : ContentPage
{
    private readonly OpenAIService _openAIService;
    private CancellationTokenSource? _typingCancellationTokenSource;

    public MainPage(OpenAIService openAIService)
    {
        InitializeComponent();

        _openAIService = openAIService;
    }

    private void FeedbackEditor_TextChanged(object sender, TextChangedEventArgs e)
    {
        // Cancel any pending AI evaluation for the previous text
        _typingCancellationTokenSource?.Cancel();
        _typingCancellationTokenSource?.Dispose();

        // Reset the rating while the user is still typing and hide the
        // loading indicator until we actually start the AI request.
        RatingControl.Value = 0;
        LoadingIndicator.IsVisible = false;
        LoadingIndicator.IsRunning = false;

        _typingCancellationTokenSource = new CancellationTokenSource();

        _ = ProcessFeedbackAfterDelayAsync(
            _typingCancellationTokenSource.Token);
    }

    private async Task ProcessFeedbackAfterDelayAsync(
        CancellationToken cancellationToken)
    {
        try
        {
            // Wait for user to stop typing
            await Task.Delay(500, cancellationToken);

            if (cancellationToken.IsCancellationRequested)
                return;

            string feedback =
                FeedbackEditor.Text?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(feedback))
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    RatingControl.Value = 0;
                    LoadingIndicator.IsRunning = false;
                    LoadingIndicator.IsVisible = false;
                });

                return;
            }

            // Show AI status only after typing stops
            MainThread.BeginInvokeOnMainThread(() =>
            {
                LoadingIndicator.IsVisible = true;
                LoadingIndicator.IsRunning = true;
            });

            double rating =
                await _openAIService.GetSuggestedRatingAsync(
                    feedback,
                    cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();

            MainThread.BeginInvokeOnMainThread(() =>
            {
                RatingControl.Value = rating;
                LoadingIndicator.IsRunning = false;
                LoadingIndicator.IsVisible = false;
            });
        }
        catch (TaskCanceledException)
        {
            // User typed again before the request completed - the next
            // ProcessFeedbackAfterDelayAsync call will take over.
        }
        catch (OperationCanceledException)
        {
            // Request cancelled for the same reason - leave UI alone so the
            // newer request can drive the indicator.
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"AI rating failed: {ex}");
            MainThread.BeginInvokeOnMainThread(() =>
            {
                LoadingIndicator.IsRunning = false;
                LoadingIndicator.IsVisible = false;
            });
        }
    }
}