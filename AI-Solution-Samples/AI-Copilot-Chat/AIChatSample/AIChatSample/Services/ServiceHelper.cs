using System;

namespace AIChatSample.Services;

/// <summary>
/// Minimal service locator giving non-DI-constructed view-models (XAML
/// BindingContexts, Activator-created pages) access to app services.
/// Initialized once in <see cref="MauiProgram"/>; kept deliberately small
/// so DI constructors remain the preferred pattern.
/// </summary>
public static class ServiceHelper
{
    /// <summary>Root service provider of the running MAUI app.</summary>
    public static IServiceProvider? Services { get; private set; }

    public static void Initialize(IServiceProvider services) => Services = services;

    public static T? GetService<T>() where T : class =>
        Services?.GetService(typeof(T)) is T service ? service : null;

    public static T GetRequiredService<T>() where T : class
    {
        var service = Services?.GetService(typeof(T)) as T;
        return service ?? throw new InvalidOperationException(
            $"{typeof(T).Name} is not registered. Call ServiceHelper.Initialize(app.Services) at startup.");
    }
}