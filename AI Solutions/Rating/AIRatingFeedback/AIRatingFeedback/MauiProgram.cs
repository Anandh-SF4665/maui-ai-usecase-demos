using AIRatingFeedback.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Handlers;
using Syncfusion.Maui.Core.Hosting;

#if WINDOWS
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
#endif

namespace AIRatingFeedback
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();

            builder
                .UseMauiApp<App>()
                .ConfigureSyncfusionCore()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            builder.Services.AddSingleton<OpenAIService>();
            builder.Services.AddSingleton<MainPage>();

#if WINDOWS
EditorHandler.Mapper.AppendToMapping("RemoveBorder", (handler, view) =>
{
    handler.PlatformView.BorderThickness =
        new Microsoft.UI.Xaml.Thickness(0);

    handler.PlatformView.Padding =
        new Microsoft.UI.Xaml.Thickness(0);

    handler.PlatformView.Background =
        new Microsoft.UI.Xaml.Media.SolidColorBrush(
            Microsoft.UI.Colors.Transparent);

    handler.PlatformView.Resources["TextControlBorderThemeThickness"] =
        new Microsoft.UI.Xaml.Thickness(0);

    handler.PlatformView.Resources["TextControlBorderThemeThicknessFocused"] =
        new Microsoft.UI.Xaml.Thickness(0);
});
#endif

#if ANDROID
            EditorHandler.Mapper.AppendToMapping("RemoveUnderline", (handler, view) =>
            {
                handler.PlatformView.Background = null;
            });
#endif

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}