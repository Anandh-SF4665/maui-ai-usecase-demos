# AI Chat Application — Architecture Specification

> **Harness ID:** `HC-AICHAT-001` · **Version:** 1.1 · **Companion docs:** [PLAN.md](PLAN.md) · [SPEC.md](SPEC.md) · [TASKS.md](TASKS.md)

## 1. Project Structure

```
CopilotChat/
├── App.xaml / AppShell            # boot + resource dictionary (theme)
├── MauiProgram.cs                 # DI registrations
├── MainPage(.xaml/.cs)            # shell: nav + AI AssistView host
├── Models/Models.cs               # Agent, ChatSession, UserProfile
├── Services/
│   ├── ChatDataService.cs         # in-memory state: agents, chats, profile
│   └── AIService/IAIService       # + AzureOpenAIService + MockAIService
├── ViewModels/
│   ├── MainViewModel.cs           # nav, chat, agents, search
│   ├── CreateAgentViewModel.cs
│   ├── AgentLogoEditorViewModel.cs  (to add if missing)
│   └── ProfileViewModel.cs          (to add if missing)
└── Views/
    ├── CreateAgentPage.xaml       # Feature 1
    ├── AgentLogoEditor.xaml       # Feature 2
    ├── ProfilePage.xaml           # Feature 3
    ├── SearchView.xaml            # supporting
    └── LibraryView.xaml           # supporting
```

## 2. Key Contracts (already scaffolded — preserve signatures)

- `Agent { Id, Name, Description, Behaviour, Color, Initial, CreatedDate }`
- `ChatSession { Id, Title, CreatedDate, LastActiveDate, AgentId?, Messages: ObservableCollection<IAssistItem> }`
- `UserProfile { Name, Email, Color }`
- `MainViewModel` exposes `RequestCommand` bound to `SfAIAssistView`, plus `NewChat/OpenChat/OpenAgent/ShowSearch/ShowLibrary/ToggleNav/EditProfile/CreateAgent` commands.

## 3. Data Flow (chat)

1. User submits prompt via AI AssistView → `RequestCommand`.
2. Prompt + current agent `Behaviour` (system context) sent to `IAIService`.
3. Response (+ optional follow-up suggestions) appended to `ChatSession.Messages` / `Suggestions`.
4. Session `LastActiveDate` updated; history re-sorted.

## 4. AI Service Abstraction

- `IAIService` is the single seam between the UI and any AI provider (NFR-2).
- `AzureOpenAIService` — real provider, reads endpoint/key from environment/user-secrets (NFR-5), never committed.
- `MockAIService` — deterministic offline responses so the sample runs with zero configuration.
- DI default binding: Mock unless Azure env config is present.

## 5. State Management

- `ChatDataService` is the single in-memory store for agents, chat sessions, and the shared `UserProfile` (NFR-3).
- ViewModels mutate state only through `ChatDataService`, never against each other's private copies.
- `UserProfile` must notify UI (`ObservableObject` or wrapped) so profile edits propagate live (FR-3.3).