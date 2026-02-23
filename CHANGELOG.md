# Changelog

## [0.2.0] - 2026-02-23

### Added

- Multi-target framework support: `net6.0`, `net7.0`, `net8.0`, `net9.0`, `net10.0`.
- Built-in `/help` command handler that lists all registered commands, auto-registered via `addDefaultHelpCommand` parameter in `AddTelegramBotInfrastructure`.
- CI/CD workflows: `ci.yml` with matrix testing across all target frameworks and separate coverage job; `release.yml` with multi-framework SDK setup and automated GitHub release creation.
- Cake build targets: `Coverage-Report` and `Pack`; `--framework` and `--coverage` arguments for granular control.
- SourceLink support, deterministic builds, and NuGet package metadata for CI.
- Comprehensive README with prerequisites, architecture overview, configuration reference, authentication flow, and usage examples.
- Unit tests for `SimpleCommandState`, `TelegramBotCommandProcessingResult`, `TelegramBotExtensions`, `TelegramBotServiceCollectionExtensions`, `TelegramBotUpdateDispatcher`, `TelegramUpdateProcessor`, `TelegramUpdateQueue`, and `HelpCommandHandler`.
- Cake build tool configuration in `dotnet-tools.json`.
- Per-framework `Microsoft.Extensions.*` dependency versioning in `Directory.Packages.props`.

### Changed

- Restructured `Core` namespace: command interfaces moved to `Core.Commands`, state types moved to `Core.Commands.States`.
- Authentication prompt now includes the chat ID so users can easily share it with the bot administrator.
- Centralized dependency management via `Directory.Build.props` / `Directory.Packages.props`; removed shared `TargetFramework` from `Directory.Build.props` in favour of per-project `TargetFrameworks`.
- `Pack` Cake target now depends on `Build` instead of `Coverage-Report`.
- Updated package versions: `xunit` 2.9.3, `xunit.runner.visualstudio` 3.1.5, `coverlet.msbuild` 8.0.0, `Microsoft.NET.Test.Sdk` 18.0.1.
- Enabled `CS1591` warning (removed `NoWarn` suppression) to enforce XML documentation.

### Removed

- Legacy `build.sh` script (replaced by Cake).

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
