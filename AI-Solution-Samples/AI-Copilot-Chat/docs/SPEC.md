# AI Chat Application — Requirements Specification

> **Harness ID:** `HC-AICHAT-001` · **Version:** 1.1 · **Companion docs:** [PLAN.md](PLAN.md) · [ARCHITECTURE.md](ARCHITECTURE.md) · [TASKS.md](TASKS.md)

Requirement IDs: `FR-<n>` (functional), `NFR-<n>` (non-functional). Each requirement is **mandatory** unless marked *(nice-to-have)*.

## 1. Core Chat (existing scaffold — must keep working)

| ID | Requirement |
|---|---|
| FR-0.1 | Home screen shows a Copilot-style shell: left navigation panel (profile, New chat, Search, Library, Recent chats, Agents) and main chat area hosting `SfAIAssistView`. |
| FR-0.2 | Users can send a prompt and receive an AI response rendered in the AI AssistView. |
| FR-0.3 | Chats started against an agent carry that agent's `Behaviour` as the system prompt context. |
| FR-0.4 | `New chat` creates a fresh `ChatSession`; chat history is listed under "Recent" with most recent first. |
| FR-0.5 | Navigation panel is collapsible (toggle command) and hidden on narrow/windowed layouts where applicable. |

## 2. Feature 1 — Create New Agent

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

## 3. Feature 2 — Customize Agent Logo

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

## 4. Feature 3 — Edit User Profile

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

## 5. Supporting (existing scaffold — retain)

| ID | Requirement |
|---|---|
| FR-4.1 | **Search** view filters chat history by title/query. |
| FR-4.2 | **Library** view lists saved content (per existing `LibraryView`). |
| FR-4.3 | Suggestions (initial prompts / context-aware follow-ups) driven by the AI service remain functional. |

## 6. Non-Functional Requirements

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