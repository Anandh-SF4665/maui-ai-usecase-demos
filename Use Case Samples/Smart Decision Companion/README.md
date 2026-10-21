# SmartDailyDecisionCompanion

SmartDailyDecisionCompanion is a .NET MAUI sample app that helps users prioritize daily work with a simple AI-assisted decision experience. It includes mobile and desktop layouts, task categorization, and short AI-generated insights that support choosing what to do today.

## Features
- Mobile-first UI for Android and iOS
- Desktop layout for Windows and MacCatalyst
- Shared MVVM design with a single viewmodel across layouts
- Task lists for Do Today, Postpone, and Watch
- AI-powered insights for:
  - Mentorship guidance
  - Urgency
  - Importance
  - Positive momentum
  - Delay risk
  - Focus estimate
- Add Task popup with Topic, Description, Deadline, Project Type, and Priority
- Accept and Postpone recommendation actions
- Accessibility improvements and reduced XAML duplication
- More stable AI insight loading with stale-response protection

## Project Structure
- SmartDailyDecisionCompanion/MainPage.xaml — main UI for mobile and desktop layouts
- SmartDailyDecisionCompanion/ViewModels/MainViewModel.cs — task state, categorization, AI analysis, and commands
- SmartDailyDecisionCompanion/Models/TaskItem.cs — task model
- SmartDailyDecisionCompanion/Services/IAzureAIService.cs — AI service contract
- SmartDailyDecisionCompanion/Services/AzureBaseService.cs — Azure OpenAI transport and credential validation
- SmartDailyDecisionCompanion/Resources/Styles/Styles.xaml — shared styles and reusable icon styling

## Quick Start
Prerequisites:
- .NET SDK compatible with the solution
- .NET MAUI workload installed

Build:
- dotnet restore
- dotnet build SmartDailyDecisionCompanion.slnx

Run:
- Use the target platform that matches your environment, such as Android, iOS Simulator, Windows, or MacCatalyst.

## How to Use
1. Open the app.
2. Use the Today tab to review tasks categorized as Do Today, Postpone, or Watch.
3. Tap the floating + button to add a new task.
4. Select a task to open the Insights view and read the AI guidance.
5. Use Accept Recommendation to move the selected task into Do Today, or Postpone to defer it.

## Notes
- The sample uses an AI service abstraction, so the transport can be swapped or configured for a different backend.
- Project Type and Priority are freeform inputs in the current sample.
- If Azure credentials are invalid, the app now shows a user-facing alert more reliably.
- Mentorship and related insight fields now ignore stale async responses so rapid task selection is more stable.

## Key Highlights
- Clean responsive MAUI UI with separate mobile and desktop experiences
- Shared viewmodel keeps task state consistent across layouts
- AI-assisted recommendations help users decide what to do next
- Insight cards summarize urgency, importance, momentum, and delay risk
- Improved accessibility and reusable styling reduce UI duplication
- More reliable credential validation and async insight loading

## Troubleshooting
- If insights stay on Analyzing, verify the Azure OpenAI endpoint, key, and deployment name.
- If the wrong-credentials alert does not appear, make sure the app is running with a visible main window.
- If AI responses are empty, confirm the network connection and service availability.
- If a task selection seems stale, reselect the task to refresh the insight cards.
- If build errors appear, restore packages and ensure the correct .NET MAUI workload is installed.

## License
Syncfusion controls are subject to the [Syncfusion License](https://www.syncfusion.com/sales/licensing). A free community license is available for qualifying individuals and small businesses.
