# AI-Copilot-Chat Sample

## Overview

This sample demonstrates a customizable multi-agent AI chat experience built with .NET MAUI and Syncfusion UI components.

The sample focuses on:

- Creating and managing agents
- Customizing agent branding and avatar appearance
- Editing the active user profile
- Rendering a responsive chat experience with Syncfusion AI AssistView patterns

## Documentation

- [Plan](docs/PLAN.md)
- [Specification](docs/SPEC.md)
- [Architecture](docs/ARCHITECTURE.md)
- [Tasks](docs/TASKS.md)

## AI Configuration

This sample is designed around the `IAIService` abstraction documented in `docs/ARCHITECTURE.md`.

Do not store API keys in source control. Configure secrets through environment-specific app configuration or secure platform storage.

## Supported Platforms

The harness is written for the sample’s MAUI target matrix and is intended for Android and Windows validation.

## Release Matrix

See [docs/RELEASE_MATRIX.md](docs/RELEASE_MATRIX.md) for the feature-by-platform validation matrix.
