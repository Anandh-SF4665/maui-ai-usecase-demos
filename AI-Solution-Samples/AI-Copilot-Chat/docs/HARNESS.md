# AI Chat Application — Development Harness

> **Deliverable type:** Specification + task harness only. No implementation in this change set.
> **Target sample:** .NET MAUI AI Chat application (Copilot-style) using **Syncfusion .NET MAUI AI AssistView (`SfAIAssistView`)** and Syncfusion MAUI components.
> **Design reference:** Essential Studio UI Kit for .NET MAUI (Figma) — branch `KB3Zj1ygg7g9hcA6Bve2IZ`, node `21937-2`. The harness textually encodes the plan; pixel-level styling must be verified against the Figma by the implementer.

---

## 1. Harness Metadata

| Field | Value |
|---|---|
| Harness ID | `HC-AICHAT-001` |
| Version | 1.0 |
| Status | Approved for AI-assisted development |
| Stack | .NET MAUI (net10.0 multi-target: android, ios, maccatalyst, windows) |
| UI toolkit | Syncfusion .NET MAUI (AIAssistView + Core), Essential Studio UI Kit screens as design base |
| Architecture | MVVM (CommunityToolkit.Mvvm), DI via `MauiAppBuilder` |
| AI backend | Azure OpenAI via `Azure.AI.OpenAI` (abstracted behind `IAIService`; mock fallback for offline dev) |
| Sample location | `AI-Solution-Samples/AI-Copilot-Chat/CopilotChat/` (existing scaffold — extend, do not rewrite) |

### 1.1 Design References (Figma / UI Kit)
- Design file: `Essential UI Kit for MAUI` — node `21937-2`.
- Base screens: Profile, Create/Edit forms, Chat/Assistant layouts from the UI Kit.
- Customization: layouts and controls adapted for AI-specific functionality (agent creation flow, avatar/logo editor).
- Non-negotiables: consistent styling, navigation patterns, responsive behavior across target platforms.
- **Note:** Figma cannot be opened from the dev sandbox. Visual fidelity must be checked by a human against the design link before sign-off. Any deviation should be logged in `docs/DESIGN-DEVIATIONS.md` (create if needed).

---

## 2. Product Overview

The AI Chat sample showcases how users can create and manage AI assistants using Syncfusion .NET MAUI AI AssistView, together with screens derived from the Essential Studio UI Kit. It demonstrates **agent creation, agent customization (logo/avatar), profile management, and conversational AI chat** in a modern Copilot-like UI.

### 2.1 Personas
- **End user (app user):** creates agents, personalizes branding, updates profile, chats with agents.
- **Developer (consumer of the sample):** learns the integration pattern for `SfAIAssistView`, agent models, and DI/MVVM wiring.

### 2.2 High-Level User Flows
```
App start
 └─> Copilot home (default chat + left navigation)
      ├─> New chat                       (Feature 0)
      ├─> Create New Agent ────────────> Agent listing (Feature 1)
      │        └─> Customize Agent Logo (Feature 2)
      ├─> Open agent ─> Chat with agent  (Feature 0, uses agent Behaviour as system prompt)
      ├─> Search chat history           (supporting)
      ├─> Library                        (supporting)
      └─> Edit Profile ────────────────> Profile screen (Feature 3)
```

---

## 3. Requirements Specification

Requirement IDs: `FR-<n>` (functional), `NFR-<n>` (non-functional). Each requirement is **mandatory** unless marked *(nice-to-have)*.

### 3.1 Core Chat (existing scaffold — must keep working)

| ID | Requirement |
|---|---|
| FR-0.1 | Home screen shows a Copilot-style shell: left navigation panel (profile, New chat, Search, Library, Recent chats, Agents) and main chat area hosting `SfAIAssistView`. |
| FR-0.2 | Users can send a prompt and receive an AI response rendered in the AI AssistView. |
| FR-0.3 | Chats started against an agent carry that agent's `Behaviour` as the system prompt context. |
| FR-0.4 | `New chat` creates a fresh `ChatSession`; chat history is listed under "Recent" with most recent first. |
| FR-0.5 | Navigation panel is collapsible (toggle command) and hidden on narrow/windowed layouts where applicable. |

### 3.2 Feature 1 — Create New Agent

**Purpose:** Allow users to create personalized AI assistants for different use cases.
**Screen:** `CreateAgentPage` (UI Kit form screen as base).

| ID | Requirement |
|---|---|
| FR-1.1 | Screen exposes input fields: **Agent Name**, **Description**, **Behaviour**. |
| FR-1.2 | Agent Name is required; Description and Behaviour optional but encouraged (validation on empty/whitespace name). |
| FR-1.3 | Each agent gets a unique Id, default avatar color, and initial derived from its name. |
| FR-1.4 | On save, the agent appears immediately in the **Agents** section of the navigation listing page. |
| FR-1.5 | Newly created agent is immediately usable: tapping it starts a chat session bound to that agent. |
| FR-1.6 | Entry point: "New Agent" action from the agents listing area (per Figma flow "When click on New Agent"). |

**Acceptance criteria (AC-1):**
1. Create agent with name `Code Helper` → agent visible in listing with initial `C`.
2. Attempt to save with empty name → save blocked, user is informed, no agent persisted.
3. New agent tap → chat opens; first AI response reflects the agent's `Behaviour`.
4. Agent persists for the app lifetime (in-memory minimum; persistence optional per NFR-3).

### 3.3 Feature 2 — Customize Agent Logo

**Purpose:** Enable users to visually personalize their AI assistants.
**Screen:** `AgentLogoEditor`.

| ID | Requirement |
|---|---|
| FR-2.1 | Select avatar from **predefined templates** (color/initial-based avatar set; template gallery per Figma). |
| FR-2.2 | Change **background color** of the avatar (color palette or picker). |
| FR-2.3 | Upload a **logo/image** from device (FilePicker / MediaPicker) as custom avatar image *(nice-to-have if MAUI picker available on all targets)*. |
| FR-2.4 | **Save** persists the avatar choice to the agent. |
| FR-2.5 | Customized logo is reflected everywhere the agent is rendered: agent listing, nav avatar, and chat screen header/assistant bubble. |

**Acceptance criteria (AC-2):**
1. Open logo editor for an agent → change background color → Save → new color visible in agent listing and in an open chat with that agent.
2. Select a predefined template → Save → template glyph/color replaces default initial.
3. (If FR-2.3 implemented) Upload PNG → image shows instead of initial, aspect preserved (no stretch).

### 3.4 Feature 3 — Edit User Profile

**Purpose:** Allow users to update existing profile information.
**Screen:** `ProfilePage` (UI Kit profile screen as base).

| ID | Requirement |
|---|---|
| FR-3.1 | Screen exposes editable fields: **Name** and **Email Address**. |
| FR-3.2 | Save applies changes to the single shared `UserProfile` used app-wide. |
| FR-3.3 | Updated configuration is reflected immediately across the application (navigation header avatar initial/name, any user-identifying UI) without restart. |
| FR-3.4 | Basic validation: non-empty name; non-empty email matching a simple `.*@.*\..*` pattern. |

**Acceptance criteria (AC-3):**
1. Change name to `Sam` → nav header shows `S` and `Sam` immediately after save.
2. Invalid email (`abc`) → save blocked with inline feedback.
3. Back navigation returns to previous screen; no crash when profile screen opened twice.

### 3.5 Supporting (existing scaffold — retain)

| ID | Requirement |
|---|---|
| FR-4.1 | **Search** view filters chat history by title/query. |
| FR-4.2 | **Library** view lists saved content (per existing `LibraryView`). |
| FR-4.3 | Suggestions (initial prompts / context-aware follow-ups) driven by the AI service remain functional. |

### 3.6 Non-Functional Requirements

| ID | Requirement |
|---|---|
| NFR-1 | MVVM throughout: no business logic in code-behind; commands + `ObservableProperty` via CommunityToolkit.Mvvm. |
| NFR-2 | AI access abstracted behind `IAIService`; `AzureOpenAIService` is one implementation. A mock implementation must exist so the sample runs without credentials. |
| NFR-3 | State is in-memory (`ChatDataService`); no persistence requirement in v1. *(nice-to-have: Preferences/JSON file persistence.)* |
| NFR-4 | All Syncfusion controls registered via `builder.ConfigureSyncfusionCore()`. |
| NFR-5 | No secrets in source: endpoint/key read from environment/user-secrets, never committed. |
| NFR-6 | Builds clean (`0 errors`) for at least `net10.0-android` and `net10.0-windows` in CI/sandbox. |
| NFR-7 | Consistent theme resources (colors, fonts) from the UI Kit palette; both light and default theme must not have contrast regressions. |
| NFR-8 | AI responses must be cancellable/non-blocking: UI remains responsive while awaiting AI (`async/await` on background, marshal to main thread). |

---

## 4. Architecture Specification

```
CopilotChat/
├── App.xaml / AppShell            # boot + resource dictionary (theme)
├── MauiProgram.cs                 # DI registrations
├── MainPage(.xaml/.cs)            # shell: nav + AI AssistView host
├── Models/Models.cs               # Agent, ChatSession, UserProfile
├── Services/
│   ├── ChatDataService.cs         # in-memory state: agents, chats, profile
│   └── AIService/IAIService      # + AzureOpenAIService + MockAIService
├── ViewModels/
│   ├── MainViewModel.cs           # nav, chat, agents, search
│   ├── CreateAgentViewModel.cs
│   ├── AgentLogoEditorViewModel.cs  (to add if missing)
│   └── ProfileViewModel.cs          (to add if missing)
└── Views/
    ├── CreateAgentPage.xaml       # Feature 1
    ├── AgentLogoEditor.xaml      # Feature 2
    ├── ProfilePage.xaml           # Feature 3
    ├── SearchView.xaml            # supporting
    └── LibraryView.xaml           # supporting
```

**Key contracts (already scaffolded — preserve signatures):**
- `Agent { Id, Name, Description, Behaviour, Color, Initial, CreatedDate }`
- `ChatSession { Id, Title, CreatedDate, LastActiveDate, AgentId?, Messages: ObservableCollection<IAssistItem> }`
- `UserProfile { Name, Email, Color }`
- `MainViewModel` exposes `RequestCommand` bound to `SfAIAssistView`, plus `NewChat/OpenChat/OpenAgent/ShowSearch/ShowLibrary/ToggleNav/EditProfile/CreateAgent` commands.

**Data flow (chat):**
1. User submits prompt via AI AssistView → `RequestCommand`.
2. Prompt + current agent `Behaviour` (system context) sent to `IAIService`.
3. Response (+ optional follow-up suggestions) appended to `ChatSession.Messages` / `Suggestions`.
4. Session `LastActiveDate` updated; history re-sorted.

---

## 5. Task Breakdown

Tasks are ordered; each is independently verifiable. Implementers (human or AI agent) must check off tasks with evidence. **Effort:** S ≤ 2h, M ≤ 1d, L > 1d.

### Phase 0 — Harness & baseline (this change set)
- [ ] **T0.1** (S) Commit this harness doc. — *done in this change set.*
- [ ] **T0.2** (S) Baseline build of existing scaffold passes (`dotnet build -f net10.0-android`). Record result in PR notes.

### Phase 1 — Core chat hardening (existing scaffold)
- [ ] **T1.1** (M) Verify/complete `IAIService` abstraction: `AzureOpenAIService` + `MockAIService`; DI default = Mock unless env config present. *(NFR-2)*
- [ ] **T1.2** (S) Request flow: prompt → agent behaviour context → streaming/response → AssistView message append; UI stays responsive (NFR-8).
- [ ] **T1.3** (S) New chat / chat history switching works; most-recent-first ordering by `LastActiveDate`.
- [ ] **T1.4** (S) Suggestions: initial prompts on empty chat; follow-up suggestions after responses where service provides them.

### Phase 2 — Feature 1: Create New Agent
- [ ] **T2.1** (M) `CreateAgentPage`: Name/Description/Behaviour editors per Figma form layout; validation per FR-1.2.
- [ ] **T2.2** (S) `CreateAgentViewModel`: save creates `Agent` via `ChatDataService`; listing refreshes (FR-1.4).
- [ ] **T2.3** (S) "New Agent" entry point in agent listing navigates to the create screen (FR-1.6).
- [ ] **T2.4** (S) Tapping new agent starts bound chat session (FR-1.5); behaviour injected into AI request (FR-0.3).
- [ ] **T2.5** (S) Verify all AC-1 criteria manually on at least one target platform.

### Phase 3 — Feature 2: Customize Agent Logo
- [ ] **T3.1** (M) `AgentLogoEditor`: predefined template gallery (FR-2.1) + background color selection (FR-2.2).
- [ ] **T3.2** (M) Image upload via `FilePicker` → custom avatar (FR-2.3) with graceful fallback if unsupported on a platform.
- [ ] **T3.3** (S) Save propagates avatar to agent listing, nav, and chat header (FR-2.5). Requires shared avatar-rendering template (may extract `AgentAvatarTemplate` into a reusable resource if duplicated 3+ places).
- [ ] **T3.4** (S) Verify all AC-2 criteria.

### Phase 4 — Feature 3: Edit User Profile
- [ ] **T4.1** (M) `ProfilePage`: Name + Email editors per Figma; validation per FR-3.4.
- [ ] **T4.2** (S) Save mutates shared `UserProfile`; nav header (initials + name) updates live (FR-3.3). Requires `INotifyPropertyChanged` propagation from `UserProfile` (make it `ObservableObject` or wrap).
- [ ] **T4.3** (S) Verify all AC-3 criteria.

### Phase 5 — Polish & release
- [ ] **T5.1** (M) Theme pass: resource dictionary alignment with UI Kit palette; check contrast on dark/light.
- [ ] **T5.2** (M) Responsive pass: nav collapse behavior on windowed Android/Windows; tablet layout if feasible.
- [ ] **T5.3** (S) README for the sample: features, screenshots/GIF, how AI config is supplied (no keys in repo).
- [ ] **T5.4** (S) Final manual test matrix: each Feature × {Android, Windows} minimum; log results.

### Dependency order
```
T0.2 → T1.* → T2.* → T3.* (needs T2.2 model) → T4.* → T5.*
T3.3 depends on T2.2 (agent model) and T3.1/T3.2.
```

---

## 6. Verification & Definition of Done

**Per-task evidence (mandatory):** every task closure must record — command run (or manual step), result, and the acceptance-criteria IDs covered.

**Definition of Done (whole sample):**
- [ ] All FR and NFR rows above satisfied (or explicitly waived with reason).
- [ ] All acceptance criteria (AC-1, AC-2, AC-3) demonstrated on at least one platform.
- [ ] `dotnet build` clean for android + windows targets.
- [ ] Sample runs with `MockAIService` with zero configuration; with Azure OpenAI when env config present.
- [ ] No credentials committed; `.gitignore` covers user-secrets.
- [ ] README updated; design deviations documented.
- [ ] PR reviewers sign off; each commit maps to one or more task IDs (e.g., `feat: T2.2 create-agent save flow (FR-1.4)`).

**Out of scope (v1):** multi-user auth, server-side sync, real image editing/cropping, voice input, streaming token rendering beyond what AIAssistView provides natively.

---

## 7. Change Log

| Version | Change |
|---|---|
| 1.0 | Initial harness: requirements (FR-0.x chat baseline, FR-1.x–FR-3.x features, FR-4.x supporting, NFR-1..8), architecture contracts, 5-phase task breakdown, AC sets, DoD. |