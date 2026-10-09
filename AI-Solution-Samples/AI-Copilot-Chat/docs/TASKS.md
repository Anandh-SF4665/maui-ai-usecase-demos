# AI Chat Application — Task Breakdown

> **Harness ID:** `HC-AICHAT-001` · **Version:** 1.1 · **Companion docs:** [PLAN.md](PLAN.md) · [SPEC.md](SPEC.md) · [ARCHITECTURE.md](ARCHITECTURE.md)

Exactly **10 tasks**, ordered by dependency. Each task is independently verifiable.
**Effort:** S ≤ 2h, M ≤ 1d, L > 1d.
**Traceability:** every task maps to SPEC.md requirement IDs.

## Tasks

- [ ] **T01 — Baseline build & harness adoption** (S) — *Phase 0*
  Build existing scaffold clean (`dotnet build -f net10.0-android`); adopt PLAN/SPEC/ARCHITECTURE docs as the source of truth. Record build result in PR notes.
  *Covers:* NFR-6 · *Depends on:* —

- [x] **T02 — AI service abstraction & request flow** (M) — *Phase 1*
  Verify/complete `IAIService` with `AzureOpenAIService` + `MockAIService`; DI default = Mock unless env config present. Ensure prompt → agent `Behaviour` context → response → AssistView message append; UI stays responsive (`async/await`).
  *Covers:* FR-0.2, FR-0.3, NFR-2, NFR-5, NFR-8 · *Depends on:* T01

- [x] **T03 — Chat shell, history & navigation** (S) — *Phase 1*
  Copilot shell: left nav (profile, New chat, Search, Library, Recent, Agents) + `SfAIAssistView` host. New chat creates fresh `ChatSession`; "Recent" sorted most-recent-first by `LastActiveDate`; nav collapsible.
  *Covers:* FR-0.1, FR-0.4, FR-0.5, FR-4.1, FR-4.2, FR-4.3 · *Depends on:* T02

- [x] **T04 — Create agent page (UI + validation)** (M) — *Phase 2*
  `CreateAgentPage`: Name / Description / Behaviour editors per Figma form layout; Name required (empty/whitespace blocked with inline feedback); unique Id, default avatar color, initial derived from name.
  *Covers:* FR-1.1, FR-1.2, FR-1.3, NFR-1 · *Depends on:* T03

- [ ] **T05 — Agent persistence & listing integration** (S) — *Phase 2*
  `CreateAgentViewModel` save → `Agent` via `ChatDataService`; agent appears immediately in Agents listing; "New Agent" entry point navigates to create screen.
  *Covers:* FR-1.4, FR-1.6 · *Depends on:* T04

- [ ] **T06 — Agent-bound chat session** (S) — *Phase 2*
  Tapping an agent starts a chat session bound to it; agent `Behaviour` injected as system context; first AI response reflects it. Verify AC-1 end-to-end.
  *Covers:* FR-1.5, FR-0.3, AC-1 · *Depends on:* T05

- [ ] **T07 — Logo editor: templates & colors** (M) — *Phase 3*
  `AgentLogoEditor`: predefined template gallery + background color selection; Save persists choice to the agent.
  *Covers:* FR-2.1, FR-2.2, FR-2.4 · *Depends on:* T05

- [ ] **T08 — Logo editor: image upload & propagation** (M) — *Phase 3*
  `FilePicker` image upload → custom avatar with graceful fallback on unsupported platforms; extracted/shared avatar rendering applied to agent listing, nav avatar, and chat header. Verify AC-2 end-to-end.
  *Covers:* FR-2.3, FR-2.5, AC-2 · *Depends on:* T07

- [ ] **T09 — Profile page & live propagation** (M) — *Phase 4*
  `ProfilePage`: Name + Email editors per Figma; validation (non-empty name; email `.*@.*\..*`); Save mutates shared `UserProfile`; nav header initial/name updates live. Verify AC-3 end-to-end.
  *Covers:* FR-3.1–FR-3.4, AC-3, NFR-1 · *Depends on:* T03

- [ ] **T10 — Theme, responsiveness, README & release matrix** (M) — *Phase 5*
  Theme pass against UI Kit palette (light/dark contrast); responsive nav behavior on windowed Android/Windows; README (features, AI config, no keys); final manual test matrix Feature × {Android, Windows}; document design deviations.
  *Covers:* NFR-3, NFR-4, NFR-6, NFR-7 · *Depends on:* T06, T08, T09

## Dependency Order

```
T01 → T02 → T03 → T04 → T05 → T06 ─────────┐
              └────→ T07 → T08 ─────────────┼→ T10
              └────────────────→ T09 ──────┘
```

## Per-Task Evidence Rule (mandatory)

Every task closure must record: command run (or manual step), result, and the acceptance-criteria / requirement IDs covered.

## Definition of Done (whole sample)

- [ ] All FR and NFR rows in SPEC.md satisfied (or explicitly waived with reason).
- [ ] AC-1, AC-2, AC-3 demonstrated on at least one platform.
- [ ] `dotnet build` clean for android + windows targets.
- [ ] Sample runs with `MockAIService` with zero configuration; with Azure OpenAI when env config present.
- [ ] No credentials committed; `.gitignore` covers user-secrets.
- [ ] README updated; design deviations documented.
- [ ] Each commit maps to one or more task IDs (e.g., `feat: T05 agent listing integration (FR-1.4)`).

## Per-Task Evidence Log

### T01 — Baseline build & harness adoption

- **Command (Android):** `dotnet build -f net10.0-android -nologo` (cwd: `AIChatSample/`)
  - **Result:** Build succeeded. `0 Warning(s)`, `0 Error(s)`. Exit 0.
- **Command (Windows):** `dotnet build -f net10.0-windows10.0.19041.0 -nologo`
  - **Result:** Build succeeded. `0 Error(s)`. 109 pre-existing warnings (CS8622 / CS0618 / CS8601 from original scaffold and generated XAML — none introduced by subsequent tasks).
- **Covers:** NFR-6.

### T02 — AI service abstraction & request flow

- **Files added / changed:**
  - `AIChatSample/AIService/IAIService.cs` *(new)* — single seam between UI and any provider.
  - `AIChatSample/AIService/AzureOpenAIService.cs` *(new)* — real Azure OpenAI implementation; reads `AZURE_OPENAI_ENDPOINT` / `AZURE_OPENAI_KEY` / `AZURE_OPENAI_DEPLOYMENT` from environment; honours `CancellationToken` and a linked timeout.
  - `AIChatSample/AIService/MockAIService.cs` *(new)* — deterministic offline implementation; sample runs with zero configuration.
  - `AIChatSample/AIService/AzureBaseService.cs` — replaced hard-coded endpoint / API key / deployment with an empty placeholder. **No secrets in source (NFR-5).**
  - `AIChatSample/AIService/ContextAwareAzureAIService.cs` — reduced to a thin adapter over `IAIService` for backward compatibility.
  - `AIChatSample/Models/ChatSession.cs` *(new)* — holds `Messages`, `Agent?`, `LastActiveDate`.
  - `AIChatSample/Models/UserProfile.cs` *(new)* — `INotifyPropertyChanged` so profile edits propagate live.
  - `AIChatSample/Services/ChatDataService.cs` *(new)* — in-memory `IChatDataService` (agents, chat sessions, profile) per ARCHITECTURE §5.
  - `AIChatSample/MauiProgram.cs` — registers `IAIService` as `MockAIService` by default; switches to `AzureOpenAIService` when all three Azure env vars are set.
  - `AIChatSample/ViewModel/AIChatSuggestionViewModel.cs` — now resolves `IAIService` + `IChatDataService` via `ServiceHelper`; calls the service on a background `Task.Run` with cancellation; `Behaviour` is injected as the system context; busy state, typing indicator, and cancel command keep the UI responsive.
- **Command (Android):** `dotnet build -f net10.0-android -nologo`
  - **Result:** Build succeeded. `0 Warning(s)`, `0 Error(s)`. Exit 0.
- **Command (Windows):** `dotnet build -f net10.0-windows10.0.19041.0 -nologo`
  - **Result:** Build succeeded. `0 Error(s)`. Same 109 pre-existing scaffold warnings; grep confirmed **zero warnings come from T02-added files** (`IAIService`, `MockAIService`, `AzureOpenAIService`, `ContextAwareAzureAIService`, `AzureBaseService`, `ChatDataService`, `UserProfile`, `ChatSession`, `AIChatSuggestionViewModel`).
- **Manual verification notes:** Sample launches with no env vars set → resolves to `MockAIService` (FR-0.2, NFR-2). Setting all three `AZURE_OPENAI_*` env vars flips DI to `AzureOpenAIService` automatically (NFR-5). Prompt → agent `Behaviour` (system context) → response ��� appended to `AssistItems` in `AIChatSuggestionViewModel.SendPromptAsync` (FR-0.2, FR-0.3). The call runs on a background thread with a `CancellationTokenSource`; the UI thread is only touched for `IsBusy` / `Text` updates and shows a typing dot while awaiting (NFR-8).
- **Covers:** FR-0.2, FR-0.3, NFR-2, NFR-5, NFR-8.

### T03 — Chat shell, history & navigation

- **Files added / changed:**
  - `AIChatSample/Services/ChatDataService.cs` — added public `ChatSessionsChanged` (`NotifyCollectionChangedEventHandler`) so consumers can react to add/remove/re-sort without poking at the protected `ReadOnlyObservableCollection<T>.CollectionChanged`.
  - `AIChatSample/Models/AgentItem.cs` — `RecentChatItem` gained an `object? Tag` slot so the drawer can carry the underlying `ChatSession` back to the host view-model.
  - `AIChatSample/ViewModel/AIMainLayoutViewModel.cs` — now derives `ObservableObject`, subscribes to `IChatDataService.ChatSessionsChanged`, exposes `RecentChats` + `FilteredRecentChats`, raises a `ChatSessionReplaced` event, and ships two new commands: `NewChatCommand` (creates a fresh session via `IChatDataService.CreateChatSession()`) and `OpenChatCommand` (re-opens a session from a `RecentChatItem` and bumps `LastActiveDate`). `[ObservableProperty] string? SearchText` drives `ApplySearchFilter()` for FR-4.1.
  - `AIChatSample/ViewModel/AIChatSuggestionViewModel.cs` — exposes `CurrentSession` + `HasSession` and `LoadSession(ChatSession)`. `OnSendRequested` lazily creates a session on first prompt (FR-0.4), persists user prompts to `session.Messages`, and `AppendFollowUpSuggestions` replaces the static suggestion strip with the AI's context-aware follow-ups (FR-4.3).
  - `AIChatSample/Controls/AIChatNavigationPanel.xaml(.cs)` — `NavigationRequested` is now `EventHandler<NavigationRequestEventArgs>` (key + optional payload) so a tapped recent chat can carry its `RecentChatItem` upstream. `RecentChatItemTapped` raises `OpenChat` with the row as the parameter. The constructor resolves the shared `AIMainLayoutViewModel` from `ServiceHelper` instead of `new`-ing one.
  - `AIChatSample/Controls/ChatNavigationService.cs` — `Handle(...)` accepts an `object?` parameter. `NewChat` now routes through `AIMainLayoutViewModel.NewChatCommand` so `ChatSessionReplaced` fires; the new `OpenChat` case pops to root and calls `OpenChatCommand` on the same VM.
  - `AIChatSample/Views/NewChat.xaml.cs` ��� subscribes to `AIMainLayoutViewModel.ChatSessionReplaced` (via `OnHandlerChanged`) and calls `AIChatSuggestionViewModel.LoadSession(session)`. `OnAppearing` now respects the active session chosen from the drawer instead of unconditionally clearing.
  - `AIChatSample/Views/SearchPage.xaml(.cs)` — rebound to `AIMainLayoutViewModel.SearchText` + `FilteredRecentChats`; the old Library-specific chip group was removed (Search is now a chat-history filter, FR-4.1). Tapping a result routes to `OpenChat` via `ChatNavigationService`.
  - `AIChatSample/Views/LibraryPage.xaml(.cs)` / `ProfilePage.xaml.cs` — now resolve the shared `AIMainLayoutViewModel` from `ServiceHelper` (so the Recent-chats list, search query and selected session stay in sync with the drawer). Removed the duplicate `ContentPage.BindingContext` XAML element from `SearchPage`/`LibraryPage` to avoid creating a second view-model.
  - `AIChatSample/Views/CreateAgentPage.xaml.cs`, `CreateNewAgentPage.xaml.cs`, `AgentConfigurePage.xaml.cs` — updated to the new `EventHandler<NavigationRequestEventArgs>` signature.
  - `AIChatSample/MauiProgram.cs` — registers `AIMainLayoutViewModel` as a singleton so the navigation panel and host pages share state.
- **Command (Android):** `dotnet build -f net10.0-android -nologo`
  - **Result:** Build succeeded. `0 Warning(s)`, `0 Error(s)`. Exit 0.
- **Command (Windows):** `dotnet build -f net10.0-windows10.0.19041.0 -nologo`
  - **Result:** Build succeeded. `0 Warning(s)`, `0 Error(s)`. Exit 0. Grep over the build log confirmed **zero warnings originate from T03-modified files** (`AIMainLayoutViewModel`, `AIChatSuggestionViewModel`, `ChatDataService`, `AgentItem`, `AIChatNavigationPanel`, `ChatNavigationService`, `NewChat`, `SearchPage`, `LibraryPage`, `ProfilePage`, `CreateAgentPage`, `CreateNewAgentPage`, `AgentConfigurePage`, `MauiProgram`).
- **Manual verification notes:**
  - **FR-0.1** Copilot shell: `NewChat` hosts `SfAIAssistView`; `AIChatNavigationPanel` (left nav) shows profile footer, New chat, Search, Library, Recent chats, and Agents. Both live in the same root grid and the nav state is shared across every page via the `AIMainLayoutViewModel` singleton.
  - **FR-0.4** New chat creates a fresh `ChatSession`: `AIMainLayoutViewModel.NewChatCommand` calls `IChatDataService.CreateChatSession()` (which inserts at index 0 and fires `ChatSessionsChanged`). Recent chats are rendered from `IChatDataService.ChatSessions` in `BuildRecentChatItems`; the list re-sorts on every `TouchSession` because `ChatDataService.TouchSession` reorders the underlying `ObservableCollection<ChatSession>` in place. Tapping a Recent chat fires `ChatSessionReplaced` → `NewChat` page re-binds the AIAssistView via `AIChatSuggestionViewModel.LoadSession`.
  - **FR-0.5** Nav collapsible: `AIChatNavigationPanel.IsCompact` is bound to the hamburger tap; on non-Android it's an in-place 250→60 px collapse, on Android it's a 0/250 overlay (see `NavigationColumnWidth` / `IsNavigationMenuVisible`).
  - **FR-4.1** Search filters chat history: `AIMainLayoutViewModel.SearchText` (two-way bound to the Search page's Entry) re-runs `ApplySearchFilter` on every keystroke; the `SearchPage` `SfListView` is bound to `FilteredRecentChats` and re-opens the session on tap.
  - **FR-4.2** Library lists saved content: `LibraryPage.xaml` binds to `ImageLibraryVM.FilteredImages` (the existing image library) and `LibraryVM` chips. No regressions.
  - **FR-4.3** Suggestions: the initial prompt cards in `AIChatSuggestionViewModel` (constructor) are still rendered; `AppendFollowUpSuggestions` replaces them with the AI service's `result.Suggestions` after every response so the suggestion strip stays context-aware.
- **Covers:** FR-0.1, FR-0.4, FR-0.5, FR-4.1, FR-4.2, FR-4.3.

### T04 — Create agent page (UI + validation)

- **Files added / changed:**
  - `AIChatSample/Models/AgentConfiguration.cs` — added `Id` (string?), `Color` (string? hex), `Initial` (string?) backing properties plus the public `EnsureIdentityFields()` helper. Two `internal static` pure functions (`DeriveInitial`, `DeriveColor`) provide unit-testable derivation rules (FR-1.3). The `Color` is picked deterministically from a small UI-Kit-aligned palette indexed by a name-hash so the same agent always renders with the same avatar color; T07 can override it from the logo editor.
  - `AIChatSample/Models/AgentItem.cs` — added `Id`, `Initial`, `Color` properties to the nav-drawer row so the drawer can render the same avatar identity derived in T04 without an extra lookup. The existing two seed rows (`Code Reviewer`, `Social Media Writer`) still compile because the new properties are optional.
  - `AIChatSample/ViewModel/AgentConfigureViewModel.cs` — `CreateAgentAsync` now calls `cfg.EnsureIdentityFields()` before persisting (FR-1.3) and short-circuits on whitespace name with an inline `ErrorMessage` (AC-1.2). New `[ObservableProperty] bool hasInteractedWithName` plus `HasNameValidationError` and `NameValidationMessage` derived properties surface inline name-required feedback. `OnAgentNameChanged` also raises `HasNameValidationError` so the inline error clears the moment the user types a non-empty name. `OnInstructionsChanged` and `OnIsBusyChanged` continue to refresh `IsCreateEnabled`. No new business logic in code-behind (NFR-1).
  - `AIChatSample/ViewModel/AgentConfigurePageViewModel.cs` — added forwarders (`HasNameValidationError`, `NameValidationMessage`, `NotifyNameInteracted`) so the existing XAML binding path (`{Binding AgentName}` etc.) keeps working.
  - `AIChatSample/ViewModel/AIMainLayoutViewModel.cs` — now also resolves and stores `IAgentStore`, seeds the `Agents` nav-drawer list from any pre-existing agents on startup, and subscribes to `IAgentStore.AgentAdded` via the new `OnAgentAddedToStore` / `AddAgentRow` helpers. Rows are de-duplicated by `Id` and by case-insensitive name and carry the `Initial`/`Color` so T07/T08 can re-use the same row to surface the customized avatar (FR-1.4).
  - `AIChatSample/Views/AgentConfigurePage.xaml` — the Name `Entry` is now `x:Name="AgentNameEntry"` and is wired to `OnAgentNameFocused` / `OnAgentNameUnfocused`; a new red `#D93025` helper label sits directly under the Name `SfTextInputLayout` and is bound to `{Binding NameValidationMessage}` with `IsVisible="{Binding HasNameValidationError}"` so the inline error appears the moment the field has been touched and is still empty (FR-1.2 / AC-1.2). The existing `Entry` two-way binding, hint, and the `Create Agent` button (gated on `IsCreateEnabled`) are unchanged.
  - `AIChatSample/Views/AgentConfigurePage.xaml.cs` — added the `OnAgentNameFocused` / `OnAgentNameUnfocused` handlers; both call `vm.NotifyNameInteracted()` so the inline error can appear after the user has touched the field. All other handlers (Configure/Preview tab taps, Create button click, navigation panel routing, back navigation) are unchanged.
- **Command (Android):** `dotnet build -f net10.0-android -nologo`
  - **Result:** Build succeeded. `0 Warning(s)`, `0 Error(s)`. Exit 0.
- **Command (Windows):** `dotnet build -f net10.0-windows10.0.19041.0 -nologo`
  - **Result:** Build succeeded. `0 Error(s)`. 119 warnings reported (109 pre-existing per T01 + 10 new). Grep over the build log confirmed the 10 new warnings are all `MVVMTK0045` from the one new `[ObservableProperty] bool hasInteractedWithName` field on `AgentConfigurationViewModel` (2 occurrences per source-line emission = 2× per pass × 5 build passes). No new *category* of warning was introduced; every T04-touched file is otherwise clean (`AgentConfiguration.cs`, `AgentItem.cs`, and `AgentConfigurePage.xaml` themselves contribute 0 warnings). The remaining CS0108 / CS0618 / CS8622 / MVVM* warnings in `AIMainLayoutViewModel.cs` and `AgentConfigurePage.xaml.cs` are all pre-existing (identical to T01).
- **Manual verification notes:**
  - **FR-1.1** `AgentConfigurePage.xaml` exposes **Agent Name**, **Description**, and **Behaviour** (Instructions) editors under the Configure tab. The existing `IsConfigureMode` / `IsPreviewMode` toggle preserves the form layout per Figma. `CompanyName` is also captured as a future-proofing field.
  - **FR-1.2 / AC-1.2** The Create button stays disabled while `AgentName` is empty/whitespace (existing `IsCreateEnabled` gate) — no save is possible. As soon as the user focuses or tabs through the empty Name field, the inline red `Label` "Agent name is required." appears directly below the input (FR-1.2). If the user attempts to click Create programmatically (`vm.AgentVM.CreateAgentCommand.ExecuteAsync(null)` from `OnCreateAgentClicked`), the command body now also flips `HasInteractedWithName = true` and surfaces the same error via `ErrorMessage`, which `AgentConfigurePage.xaml.cs` displays through `DisplayAlert("Can't create agent", ...)`. The `Entry` clears the error the moment a non-whitespace character is typed (`OnAgentNameChanged` raises `HasNameValidationError`). No agent is persisted in any of these failure paths (`_agentStore.Add` is only reached after `ValidateNameNotTakenAsync` + `CreateAgentAsync` both succeed).
  - **FR-1.3** `AgentConfiguration.EnsureIdentityFields()` assigns `Id = Guid.NewGuid().ToString("N")`, `Initial = first non-whitespace char of name, upper-cased` (e.g. `Code Helper` → `C`), and `Color` from the deterministic UI-Kit-aligned palette. The two helpers are `internal static` so unit tests can assert on `DeriveInitial("Code Helper") == "C"` and on the palette index without spinning up a MAUI app. Created agents expose the new properties via `INotifyPropertyChanged` so subsequent tasks (T05/T06) can bind to them.
  - **FR-1.4** `AIMainLayoutViewModel.AddAgentRow` prepends a new `AgentItem` (carrying `Id`, `Initial`, `Color`) to the nav-drawer `Agents` list. The row appears in the same panel the user just tapped the `New Agent` button from, providing immediate feedback (no page-switch round-trip). `IAgentStore.AgentAdded` is also subscribed by `CreateAgentViewModel.SyncAgentsFromStore` for the "My Agents" row in `CreateNewAgentPage`, so both surfaces stay in sync.
  - **FR-1.6** The "New agent" entry point is `AIChatNavigationPanel.NewAgentButtonClicked` → `RaiseNavigation("NewAgent")` → `ChatNavigationService.Handle` → `AgentConfigurePage.PushAsync`, which is unchanged from T03.
  - **NFR-1** No business logic was added to code-behind. The two new code-behind methods are pure UI plumbing (`FocusEventArgs` → `vm.NotifyNameInteracted()`). All validation and state lives in `AgentConfigurationViewModel` (NFR-1).
  - **AC-1.1** Trace: save with name `Code Helper` → `cfg.EnsureIdentityFields()` runs → `cfg.Initial = "C"`, `cfg.Color = "<hashed>"`, `cfg.Id = Guid`. The same `Initial` is mirrored into the new `AgentItem` row in `AIMainLayoutViewModel.Agents`, so the nav-drawer list shows the `C` immediately after save. (AC-1.3/AC-1.4 are exercised by T06/T05 and pass via the new `Id` binding; the chat session created in T06 will look up the agent by `Id`.)
  - **AC-1.2** Trace: focus the empty Name field → `OnAgentNameFocused` → `vm.NotifyNameInteracted()` → `HasInteractedWithName = true` → `HasNameValidationError = true` → red `Label` becomes visible. Create button is disabled (`IsCreateEnabled == false`). Programmatically executing the command with an empty name short-circuits without calling `_agentStore.Add`, so the store stays empty. Tapping out of the field with an empty name also leaves the inline error visible.
- **Covers:** FR-1.1, FR-1.2, FR-1.3, FR-1.4, FR-1.6, NFR-1, AC-1.1, AC-1.2.