using AIChatSample.Models;
using AIChatSample.Services;
using AIChatSample.ViewModel;
using AIChatSample.Views.AIChat;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls;

namespace AIChatSample.Controls;

/// <summary>
/// Centralized mapping from a navigation key produced by
/// <see cref="AIChatNavigationPanel"/> to a concrete page instance that
/// gets pushed onto the current <see cref="NavigationPage"/>.
/// </summary>
internal static class ChatNavigationService
{
    /// <summary>
    /// Handles a <see cref="AIChatNavigationPanel.NavigationRequested"/>
    /// event by pushing the matching page on the supplied page's
    /// navigation stack.
    /// </summary>
    public static async void Handle(Page currentPage, string key, object? parameter = null)
    {
        if (currentPage?.Navigation?.NavigationStack is null)
        {
            return;
        }

        switch (key)
        {
            case "NewChat":
                // "New chat" should always take the user back to a fresh
                // root, regardless of how deep the stack is, and reset the
                // AIAssistView so the previous conversation isn't shown.
                if (currentPage.Navigation.NavigationStack.Count > 1)
                {
                    await currentPage.Navigation.PopToRootAsync(animated: true);
                }

                // FR-0.4: route through the shared shell view-model so a
                // fresh ChatSession is created and ChatSessionReplaced
                // fires. The host page then re-binds the AIAssistView
                // via AIChatSuggestionViewModel.LoadSession.
                if (ServiceHelper.GetService<AIMainLayoutViewModel>() is { } newChatLayout)
                {
                    newChatLayout.NewChatCommand.Execute(null);
                }
                else if (currentPage.Navigation.NavigationStack.LastOrDefault() is NewChat newChatPage &&
                         newChatPage.BindingContext is AIChatSuggestionViewModel viewModel)
                {
                    viewModel.ResetConversation();
                }
                break;

            case "OpenChat":
                // FR-0.4: Tapping a recent chat in the drawer pops back
                // to the root NewChat page and binds the chosen session
                // into the AIAssistView via AIMainLayoutViewModel.
                if (currentPage.Navigation.NavigationStack.Count > 1)
                {
                    await currentPage.Navigation.PopToRootAsync(animated: true);
                }

                if (parameter is RecentChatItem recentItem &&
                    ServiceHelper.GetService<AIMainLayoutViewModel>() is { } layoutVm)
                {
                    layoutVm.OpenChatCommand.Execute(recentItem);
                }
                break;

            case "Profile":
                await PushIfNotCurrent(currentPage, typeof(ProfilePage));
                break;

            case "Library":
                await PushIfNotCurrent(currentPage, typeof(LibraryPage));
                break;

            case "Search":
                await PushIfNotCurrent(currentPage, typeof(SearchPage));
                break;

            case "NewAgent":
            case "CreateAgent":
                // "New agent" goes straight to the configure page where the
                // user can fill in the specialist-agent details. When a
                // template card (from the AIAssistView suggestion row) is
                // passed as the parameter, prefill the form from it.
                AgentConfigurePage? configurePage = null;
                if (currentPage.Handler?.MauiContext?.Services is IServiceProvider services &&
                    services.GetService<AgentConfigurePage>() is { } resolved)
                {
                    configurePage = resolved;
                }
                configurePage ??= new AgentConfigurePage();

                if (parameter is TemplateCardSuggestion template &&
                    configurePage.BindingContext is AgentConfigurePageViewModel vm)
                {
                    vm.AgentVM.InitializeFromTemplate(template);
                }

                await currentPage.Navigation.PushAsync(configurePage, animated: true);
                break;
        }
    }

    private static async Task PushIfNotCurrent(Page currentPage, Type pageType)
    {
        if (currentPage.Navigation.NavigationStack.LastOrDefault() is Page top &&
            top.GetType() == pageType)
        {
            return;
        }

        if (Activator.CreateInstance(pageType) is Page page)
        {
            await currentPage.Navigation.PushAsync(page, animated: true);
        }
    }
}
