using Markeli.TelegramBot.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Telegram.Bot;

namespace Markeli.TelegramBot.DependencyInjection;

/// <summary>
/// Extension methods for registering Telegram bot infrastructure services.
/// </summary>
public static class TelegramBotServiceCollectionExtensions
{
	/// <summary>
	/// Registers Telegram bot infrastructure services (queue, cache, processor, dispatcher, bot client).
	/// Command handlers should be registered separately via <see cref="AddTelegramBotCommandHandler{T}"/>.
	/// </summary>
	/// <param name="services">The service collection.</param>
	/// <param name="options">The Telegram bot options.</param>
	/// <returns>The service collection for chaining.</returns>
	public static IServiceCollection AddTelegramBotInfrastructure(
		this IServiceCollection services,
		TelegramBotOptions options)
	{
		ArgumentNullException.ThrowIfNull(services);
		ArgumentNullException.ThrowIfNull(options);

		options.AssertValid();

		services.TryAddSingleton(options);

		services.AddMemoryCache();
		services.AddSingleton<ITelegramBotClient>(_ => new TelegramBotClient(options.ApiToken));
		services.AddSingleton<TelegramUpdateQueue>();
		services.AddSingleton<TelegramBotCommandStateCache>();
		services.AddSingleton<TelegramUpdateProcessor>();
		services.AddHostedService<TelegramBotUpdateDispatcher>();

		return services;
	}

	/// <summary>
	/// Registers a Telegram bot command handler.
	/// </summary>
	/// <typeparam name="T">The command handler type implementing <see cref="ITelegramBotCommandHandler"/>.</typeparam>
	/// <param name="services">The service collection.</param>
	/// <returns>The service collection for chaining.</returns>
	public static IServiceCollection AddTelegramBotCommandHandler<T>(this IServiceCollection services)
		where T : class, ITelegramBotCommandHandler
	{
		ArgumentNullException.ThrowIfNull(services);

		services.AddSingleton<ITelegramBotCommandHandler, T>();

		return services;
	}
}
