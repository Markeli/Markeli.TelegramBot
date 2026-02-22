# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [0.1.0] - 2026-02-22

### Added

- `ITelegramBotCommandHandler` interface for implementing bot commands.
- `ITelegramBotCommandState` / `TelegramBotCommandStateBase` for multi-step conversational state.
- `TelegramBotCommandStateCache` — in-memory state cache with 1-hour expiration.
- `TelegramBotUpdateDispatcher` — hosted service for parallel update dispatching with lock support.
- `TelegramUpdateProcessor` — update processing with authentication and message type validation.
- `TelegramUpdateQueue` — thread-safe update queue with optional disk persistence.
- `TelegramBotOptions` — configuration with token, password, allowed chats, parallelism, and persistence path.
- `AddTelegramBotInfrastructure` / `AddTelegramBotCommandHandler<T>` DI extensions.
- `SimpleCommandState` for stateless command results.
- Cake build script (`build.cake`) with Clean, Build, Test, Pack, and Push targets.
- xUnit test project with Moq.
