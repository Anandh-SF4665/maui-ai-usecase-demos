using Microsoft.Extensions.Logging;
using Microsoft.Maui.Hosting;
using Syncfusion.Maui.Core.Hosting;
using static AIFileSearchTreeView.ServiceHelper;

namespace AIFileSearchTreeView
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
                    fonts.AddFont("MaterialIcons-Regular.ttf", "MaterialIcons");
                });

            // Register Services
            builder.Services.AddSingleton<ISemanticSearchService, SemanticSearchService>();

            // Register ViewModels
            builder.Services.AddSingleton<SalesFileViewModel>();

            // Register Views
            builder.Services.AddTransient<MainPage>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            var app = builder.Build();

            // Initialize ServiceHelper for global service access
            ServiceHelper.CurrentServices = app.Services;

            // Validate Azure credentials on startup
            var semanticService = app.Services.GetRequiredService<ISemanticSearchService>();
            if (!semanticService.IsCredentialValid)
            {
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    var page = Application.Current?.Windows[0].Page;
                    if (page != null)
                    {
#if NET10_0
                        await page.DisplayAlertAsync(
#else
                        await page.DisplayAlert(
#endif
                            "Configuration Required",
                            "Azure OpenAI credentials not configured.\n\n" +
                            "Set these environment variables:\n" +
                            "• AZURE_OPENAI_ENDPOINT\n" +
                            "• AZURE_OPENAI_API_KEY\n" +
                            "• AZURE_OPENAI_DEPLOYMENT_NAME\n\n" +
                            "AI search will be unavailable until credentials are configured.",
                            "OK");
                    }
                });
            }

            return app;
        }
    }
}
