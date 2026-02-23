# Markeli.TelegramBot

[![CI](https://github.com/Markeli/Markeli.TelegramBot/actions/workflows/ci.yml/badge.svg?branch=main)](https://github.com/Markeli/Markeli.TelegramBot/actions/workflows/ci.yml)
[![NuGet](https://img.shields.io/nuget/v/Markeli.TelegramBot)](https://www.nuget.org/packages/Markeli.TelegramBot)
[![NuGet Downloads](https://img.shields.io/nuget/dt/Markeli.TelegramBot)](https://www.nuget.org/packages/Markeli.TelegramBot)
[![Coverage](https://img.shields.io/endpoint?url=https://raw.githubusercontent.com/Markeli/Markeli.TelegramBot/badges/coverage.json)](https://github.com/Markeli/Markeli.TelegramBot/actions/workflows/ci.yml)

Infrastructure library for building Telegram bots on .NET: command dispatching, multi-step state management, update queue with persistence, and simple chat authentication.

## Features

- **Command dispatching** — register handlers via `ITelegramBotCommandHandler`, route updates by command text and supported update/message types.
- **State management** — multi-step conversational commands with in-memory state cache (`TelegramBotCommandStateBase`).
- **Update queue** — thread-safe queue with configurable parallelism and optional disk persistence on shutdown.
- **Authentication** — simple password-based chat verification with allowed chat ID filtering.
- **DI integration** — `AddTelegramBotInfrastructure` / `AddTelegramBotCommandHandler<T>` extensions for `IServiceCollection`.

## Installation

```bash
dotnet add package Markeli.TelegramBot
```

## Quick start

```csharp
builder.Services.AddTelegramBotInfrastructure(new TelegramBotOptions
{
    ApiToken = "BOT_TOKEN",
    Password = "secret",
    AllowedChatIds = new[] { 123456L }
});

builder.Services.AddTelegramBotCommandHandler<PingCommandHandler>();
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
        ITelegramBotClient client, Update update, ITelegramBotCommandState? state,
        CancellationToken ct)
    {
        await client.SendTextMessageAsync(update.Message!.Chat.Id, "pong", cancellationToken: ct);
        return TelegramBotCommandProcessingResult.WithoutState();
    }

    public string? TryGetLockKey(Update update) => null;
}
```

## Build

```bash
dotnet build
dotnet test
```

Pack NuGet package:

```bash
dotnet pack src/Markeli.TelegramBot/Markeli.TelegramBot.csproj -c Release -o ./artifacts
```

## License

[MIT](LICENSE)
