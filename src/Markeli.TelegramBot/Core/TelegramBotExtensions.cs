using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Markeli.TelegramBot.Core;

/// <summary>
/// Extension methods for Telegram bot operations.
/// </summary>
public static class TelegramBotExtensions
{
	/// <summary>
	/// Deletes multiple messages from a chat, logging each operation.
	/// </summary>
	public static async Task DeleteMessagesAsync(
		this ITelegramBotClient telegramBotClient,
		ChatId chatId,
		IReadOnlyList<int> messageIds,
		ILogger logger,
		CancellationToken cancellationToken = default)
	{
		foreach (var messageId in messageIds)
		{
			try
			{
				logger.LogDebug(
					"Deleting message with MessageId={MessageId} in chat ChatId={ChatId}...",
					messageId,
					chatId);
				await telegramBotClient.DeleteMessageAsync(
					chatId,
					messageId,
					cancellationToken);
				logger.LogInformation(
					"Deleted message with MessageId={MessageId} in chat ChatId={ChatId}",
					messageId,
					chatId);
			}
			catch (Exception e)
			{
				logger.LogError(
					e,
					"Error while deleting message with MessageId={MessageId} in chat ChatId={ChatId}",
					messageId,
					chatId);
			}
		}
	}

	/// <summary>
	/// Extracts the chat ID from a Telegram update.
	/// </summary>
	public static long GetChatId(this Update update)
	{
		ArgumentNullException.ThrowIfNull(update);

		return update.Message?.Chat.Id ?? update.CallbackQuery?.Message?.Chat.Id ?? -1;
	}
}
