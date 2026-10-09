using AIChatSample.AIService;
using AIChatSample.Services;
using AIChatSample.ViewModel;
using AIChatSample.Views;
using AIChatSample.Views.AIChat;
using Syncfusion.Maui.Core.Hosting;
using Syncfusion.Maui.Toolkit.Hosting;
using Microsoft.Extensions.Logging;

namespace AIChatSample
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureSyncfusionCore()
                .ConfigureSyncfusionToolkit()
                .ConfigureFonts(fonts =>
                {
                    //fonts.AddFont("OpenSans-Regular.ttf", "RobotoRegular");
                    //fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                    fonts.AddFont("Roboto-Medium.ttf", "Roboto-Medium");
                    fonts.AddFont("Roboto-Regular.ttf", "Roboto-Regular");
                    fonts.AddFont("MauiMaterialAssets.ttf", "MaterialAssets");
                    fonts.AddFont("UIFontIcons.ttf", "FontIcons");
                });

            // Agent creation services (swap implementations for a real backend later).
            builder.Services.AddSingleton<IAgentStore, InMemoryAgentStore>();
            builder.Services.AddSingleton<IAgentService, DefaultAgentService>();

            // In-memory chat + user-profile store (NFR-3, FR-0.4).
            builder.Services.AddSingleton<IChatDataService, ChatDataService>();

            // Shared app shell view-model (FR-0.1, FR-0.4). Registered as
            // a singleton so the Recent-chats list, user-profile header
            // and currently-open session stay in sync across the
            // navigation panel and the chat host page.
            builder.Services.AddSingleton<AIMainLayoutViewModel>();

            // AI service: default to the deterministic mock so the sample
            // runs with zero configuration. Switch to AzureOpenAIService
            // when AZURE_OPENAI_ENDPOINT / _KEY / _DEPLOYMENT are present
            // (NFR-2, NFR-5).
            if (HasAzureOpenAIConfiguration())
            {
                builder.Services.AddSingleton<IAIService, AzureOpenAIService>();
            }
            else
            {
                builder.Services.AddSingleton<IAIService, MockAIService>();
            }

            // Legacy seam kept for backward compatibility with code that
            // still depends on the old interface.
            builder.Services.AddSingleton<IAzureAIService>(sp =>
                new ContextAwareAzureAIService(sp.GetRequiredService<IAIService>()));

#if DEBUG
            builder.Logging.AddDebug();
#endif

            var app = builder.Build();
            // Give XAML-constructed VMs (which cannot use constructor DI)
            // access to applications services.
            ServiceHelper.Initialize(app.Services);
            return app;
        }

        /// <summary>
        /// True when the environment provides the minimum Azure OpenAI
        /// configuration. All three variables must be set and
        /// non-empty (NFR-5).
        /// </summary>
        private static bool HasAzureOpenAIConfiguration()
        {
            var endpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT");
            var apiKey = Environment.GetEnvironmentVariable("AZURE_OPENAI_KEY");
            var deployment = Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT");

            return !string.IsNullOrWhiteSpace(endpoint)
                && !string.IsNullOrWhiteSpace(apiKey)
                && !string.IsNullOrWhiteSpace(deployment);
        }
    }
}
