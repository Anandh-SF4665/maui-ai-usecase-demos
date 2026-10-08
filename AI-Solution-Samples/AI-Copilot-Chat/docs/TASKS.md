# AI Chat Application — Task Breakdown

> **Harness ID:** `HC-AICHAT-001` · **Version:** 1.1 · **Companion docs:** [PLAN.md](PLAN.md) · [SPEC.md](SPEC.md) · [ARCHITECTURE.md](ARCHITECTURE.md)

Exactly **10 tasks**, ordered by dependency. Each task is independently verifiable.
**Effort:** S ≤ 2h, M ≤ 1d, L > 1d.
**Traceability:** every task maps to SPEC.md requirement IDs.

## Tasks

- [ ] **T01 — Baseline build & harness adoption** (S) — *Phase 0*
  Build existing scaffold clean (`dotnet build -f net10.0-android`); adopt PLAN/SPEC/ARCHITECTURE docs as the source of truth. Record build result in PR notes.
  *Covers:* NFR-6 · *Depends on:* —

- [ ] **T02 — AI service abstraction & request flow** (M) — *Phase 1*
  Verify/complete `IAIService` with `AzureOpenAIService` + `MockAIService`; DI default = Mock unless env config present. Ensure prompt → agent `Behaviour` context → response → AssistView message append; UI stays responsive (`async/await`).
  *Covers:* FR-0.2, FR-0.3, NFR-2, NFR-5, NFR-8 · *Depends on:* T01

- [ ] **T03 — Chat shell, history & navigation** (S) — *Phase 1*
  Copilot shell: left nav (profile, New chat, Search, Library, Recent, Agents) + `SfAIAssistView` host. New chat creates fresh `ChatSession`; "Recent" sorted most-recent-first by `LastActiveDate`; nav collapsible.
  *Covers:* FR-0.1, FR-0.4, FR-0.5, FR-4.1, FR-4.2, FR-4.3 · *Depends on:* T02

- [ ] **T04 — Create agent page (UI + validation)** (M) — *Phase 2*
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