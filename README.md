# Markeli.TelegramBot

[![CI](https://github.com/Markeli/Markeli.TelegramBot/actions/workflows/ci.yml/badge.svg?branch=main)](https://github.com/Markeli/Markeli.TelegramBot/actions/workflows/ci.yml)
[![NuGet](https://img.shields.io/nuget/v/Markeli.TelegramBot)](https://www.nuget.org/packages/Markeli.TelegramBot)
[![NuGet Downloads](https://img.shields.io/nuget/dt/Markeli.TelegramBot)](https://www.nuget.org/packages/Markeli.TelegramBot)
[![Coverage](https://img.shields.io/endpoint?url=https://raw.githubusercontent.com/Markeli/Markeli.TelegramBot/badges/coverage.json)](https://github.com/Markeli/Markeli.TelegramBot/actions/workflows/ci.yml)

Infrastructure library for building Telegram bots on .NET: command dispatching, multi-step state management, update queue with persistence, and simple chat authentication.

## Prerequisites

- [.NET 6.0](https://dotnet.microsoft.com/download/dotnet/6.0) or later
- Telegram Bot API token — create one via [BotFather](https://core.telegram.org/bots#botfather)

## Features

- **Command dispatching** — register handlers via `ITelegramBotCommandHandler`, route updates by command text and supported update/message types.
- **State management** — multi-step conversational commands with in-memory state cache (`TelegramBotCommandStateBase`).
- **Update queue** — thread-safe queue with configurable parallelism and optional disk persistence on shutdown.
- **Authentication** — simple password-based chat verification with allowed chat ID filtering.
- **Built-in `/help` command** — opt-in handler that lists all registered commands via `AddHelpCommand()`.
- **DI integration** — `AddTelegramBotInfrastructure` / `AddTelegramBotCommandHandler<T>` extensions for `IServiceCollection`.

## Architecture

```
Telegram API
    │ polling via Telegram.Bot
    ▼
TelegramBotUpdateDispatcher          (IHostedService — starts polling, runs dispatch loop)
    ├─ on receive ──► TelegramUpdateQueue.Enqueue()
    └─ dispatch loop
         ├─ TelegramUpdateQueue.Take()
         ├─ ResolveCommand()           (state-cache aware routing)
         ├─ TryAcquireLock()           (optional per-key exclusive lock)
         ├─ SemaphoreSlim              (MaxDegreeOfParallelism)
         └─► TelegramUpdateProcessor.ProcessAsync()
              ├─ Auth gate             (AllowedChatIds / password challenge)
              ├─ Message type guard
              ├─ State lookup          (TelegramBotCommandStateCache)
              ├─ ITelegramBotCommandHandler.ProcessCommandAsync()
              └─ State update/remove   (based on result.State)
```

Updates are polled, enqueued into a thread-safe `BlockingCollection<Update>`, and dispatched to handlers with configurable concurrency (`MaxDegreeOfParallelism`, default 10). If a handler returns state, the next message from that chat is routed to the same handler automatically.

## Installation

```bash
dotnet add package Markeli.TelegramBot
```

## Quick start

Register the infrastructure and command handlers in your DI container:

```csharp
builder.Services.AddTelegramBotInfrastructure(new TelegramBotOptions
{
    ApiToken = "BOT_TOKEN",
    Password = "secret",
    AllowedChatIds = new[] { 123456L }
});

builder.Services.AddTelegramBotCommandHandler<PingCommandHandler>();
builder.Services.AddHelpCommand();
```

Implement a command handler:

```csharp
public class PingCommandHandler : ITelegramBotCommandHandler
{
    public string CommandName => "Ping";
    public string CommandText => "/ping";
    public IReadOnlySet<UpdateType> SupportedUpdateTypes => new HashSet<UpdateType> { UpdateType.Message };
    public IReadOnlySet<MessageType> SupportedMessageTypes => new HashSet<MessageType> { MessageType.Text };

    public async Task<TelegramBotCommandProcessingResult> ProcessCommandAsync(
        ITelegramBotClient telegramBotClient, Update telegramUpdate,
        ITelegramBotCommandState? commandState, CancellationToken cancellationToken)
    {
        await telegramBotClient.SendTextMessageAsync(
            telegramUpdate.Message!.Chat.Id, "pong", cancellationToken: cancellationToken);
        return TelegramBotCommandProcessingResult.WithoutState();
    }
}
```

The bot starts automatically as an `IHostedService` — no extra startup code required.

## Configuration

All settings are passed via `TelegramBotOptions`:

| Property | Type | Default | Description |
|---|---|---|---|
| `ApiToken` | `string` | *required* | Telegram Bot API token. |
| `Password` | `string` | *required* | Password for chat authentication (see below). |
| `AllowedChatIds` | `long[]` | `[]` | Pre-authorized chat IDs that skip password verification. |
| `MaxDegreeOfParallelism` | `int` | `10` | Maximum number of updates processed concurrently. |
| `HttpProxy` | `HttpProxyOptions?` | `null` | HTTP proxy settings. When set, all bot API traffic is routed through this proxy. See below. |
| `QueuePersistenceFilePath` | `string?` | `null` | File path for persisting pending updates on shutdown. If set, the queue is saved to disk during graceful shutdown and restored on next startup. |

### HTTP proxy

`HttpProxyOptions` fields:

| Property | Type | Description |
|---|---|---|
| `Url` | `string` | Proxy URL (e.g. `http://proxy.example.com:8080`). Required. |
| `Username` | `string?` | Proxy authentication username. |
| `Password` | `string?` | Proxy authentication password. |

```csharp
services.AddTelegramBotInfrastructure(new TelegramBotOptions
{
    ApiToken = "BOT_TOKEN",
    Password = "secret",
    HttpProxy = new HttpProxyOptions
    {
        Url = "http://proxy.example.com:8080",
        Username = "user",
        Password = "pass"
    }
});
```

### Authentication flow

Chats listed in `AllowedChatIds` are authorized automatically. When an unknown chat sends a message:

1. The bot replies with *"Hi! To use this bot, please, send a verification password."*
2. If the user sends the correct `Password`, the chat is added to the allowed set for the lifetime of the process. Authorization is stored in memory only and resets on application restart.
3. If incorrect, the bot replies *"Incorrect password! Please, try again."*

## Multi-step commands

Return `WithSimpleState()` from `ProcessCommandAsync` to keep the conversation going — the next message from that chat will be routed to the same handler with the previous state:

```csharp
public class GreetCommandHandler : ITelegramBotCommandHandler
{
    public string CommandName => "Greet";
    public string CommandText => "/greet";
    public IReadOnlySet<UpdateType> SupportedUpdateTypes => new HashSet<UpdateType> { UpdateType.Message };
    public IReadOnlySet<MessageType> SupportedMessageTypes => new HashSet<MessageType> { MessageType.Text };

    public async Task<TelegramBotCommandProcessingResult> ProcessCommandAsync(
        ITelegramBotClient telegramBotClient, Update telegramUpdate,
        ITelegramBotCommandState? commandState, CancellationToken cancellationToken)
    {
        var chatId = telegramUpdate.Message!.Chat.Id;

        if (commandState is null)
        {
            await telegramBotClient.SendTextMessageAsync(
                chatId, "What is your name?", cancellationToken: cancellationToken);
            return TelegramBotCommandProcessingResult.WithSimpleState();
        }

        var name = telegramUpdate.Message.Text;
        await telegramBotClient.SendTextMessageAsync(
            chatId, $"Hello, {name}!", cancellationToken: cancellationToken);
        return TelegramBotCommandProcessingResult.WithoutState();
    }
}
```

For custom state data, implement `ITelegramBotCommandState` (or extend `TelegramBotCommandStateBase` for timestamps) and return it via `new TelegramBotCommandProcessingResult { State = myState }`.

The user can abort a multi-step flow at any time by sending another `/command` — it will be matched to the new handler instead.

## Concurrent lock keys

Override `TryGetLockKey` to prevent parallel execution of the same command for a specific context (e.g., per chat):

```csharp
public bool TryGetLockKey(Update telegramUpdate, out string? lockKey)
{
    lockKey = $"my_command_{telegramUpdate.Message?.Chat.Id}";
    return true;
}
```

When a lock key is active, conflicting updates are re-enqueued and retried. This method has a default implementation that returns `false` (no locking), so most handlers don't need to override it.

## Build

```bash
dotnet build
dotnet test
```

The project uses [Cake](https://cakebuild.net/) for build automation. Available targets:

```bash
dotnet cake --target=Build            # Clean + build
dotnet cake --target=Test             # Build + run tests with coverage
dotnet cake --target=Coverage-Report  # Test + generate HTML coverage report
dotnet cake --target=Pack             # Coverage-Report + create NuGet package
```

Coverage reports are generated in `./artifacts/coverage-report/`.

## License

[MIT](LICENSE)
