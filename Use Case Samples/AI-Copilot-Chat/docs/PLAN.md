# AI Chat Application — Development Plan

> **Harness ID:** `HC-AICHAT-001` · **Version:** 1.1 · **Companion docs:** [SPEC.md](SPEC.md) · [ARCHITECTURE.md](ARCHITECTURE.md) · [TASKS.md](TASKS.md)

## 1. Harness Metadata

| Field | Value |
|---|---|
| Deliverable type | Specification + task harness only. No implementation in this change set. |
| Harness ID | `HC-AICHAT-001` |
| Version | 1.1 (split from single HARNESS.md into PLAN / SPEC / ARCHITECTURE / TASKS) |
| Status | Approved for AI-assisted development |
| Stack | .NET MAUI (net10.0 multi-target: android, ios, maccatalyst, windows) |
| UI toolkit | Syncfusion .NET MAUI (AI AssistView `SfAIAssistView` + Core), Essential Studio UI Kit screens as design base |
| Architecture | MVVM (CommunityToolkit.Mvvm), DI via `MauiAppBuilder` |
| AI backend | Azure OpenAI via `Azure.AI.OpenAI` (abstracted behind `IAIService`; mock fallback for offline dev) |
| Sample location | `AI-Solution-Samples/AI-Copilot-Chat/CopilotChat/` (existing scaffold — extend, do not rewrite) |

## 2. Product Overview

The AI Chat sample showcases how users can create and manage AI assistants using Syncfusion .NET MAUI AI AssistView, together with screens derived from the Essential Studio UI Kit. It demonstrates **agent creation, agent customization (logo/avatar), profile management, and conversational AI chat** in a modern Copilot-like UI.

### 2.1 Design References (Figma / UI Kit)

- Design file: `Essential UI Kit for MAUI` — branch `KB3Zj1ygg7g9hcA6Bve2IZ`, node `21937-2`.
  URL: https://www.figma.com/design/4JPM3ZTiASrz0uIJGaEbFI/branch/KB3Zj1ygg7g9hcA6Bve2IZ/Essential-ui-kit-for-MaUI?node-id=21937-2
- Base screens: Profile, Create/Edit forms, Chat/Assistant layouts from the UI Kit.
- Customization: layouts and controls adapted for AI-specific functionality (agent creation flow, avatar/logo editor).
- Non-negotiables: consistent styling, navigation patterns, responsive behavior across target platforms.
- **Note:** Figma cannot be opened from the dev sandbox. Visual fidelity must be checked by a human against the design link before sign-off. Any deviation should be logged in `docs/DESIGN-DEVIATIONS.md` (create if needed).

### 2.2 Personas

- **End user (app user):** creates agents, personalizes branding, updates profile, chats with agents.
- **Developer (consumer of the sample):** learns the integration pattern for `SfAIAssistView`, agent models, and DI/MVVM wiring.

### 2.3 High-Level User Flows

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

### 2.4 Feature Summary

| # | Feature | Purpose | Screen |
|---|---|---|---|
| 0 | Copilot-style chat shell | Conversational baseline using `SfAIAssistView` | `MainPage` |
| 1 | Create New Agent | Personalized AI assistants per use case (Name / Description / Behaviour) | `CreateAgentPage` |
| 2 | Customize Agent Logo | Visual personalization (templates, background color, image upload) | `AgentLogoEditor` |
| 3 | Edit User Profile | Update name / email; live app-wide reflection | `ProfilePage` |
| 4 | Supporting: Search / Library / Suggestions | History filtering, saved content, context-aware prompts | `SearchView`, `LibraryView` |

## 3. Scope

**In scope (v1):** all FR/NFR rows in [SPEC.md](SPEC.md); tasks T01–T10 in [TASKS.md](TASKS.md).

**Out of scope (v1):** multi-user auth, server-side sync, real image editing/cropping, voice input, streaming token rendering beyond what AIAssistView provides natively.

## 4. Change Log

| Version | Change |
|---|---|
| 1.0 | Initial harness as single HARNESS.md |
| 1.1 | Split into PLAN.md / SPEC.md / ARCHITECTURE.md / TASKS.md; task list restructured to exactly 10 tasks |