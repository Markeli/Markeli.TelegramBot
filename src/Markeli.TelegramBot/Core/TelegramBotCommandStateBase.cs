namespace Markeli.TelegramBot.Core;

/// <summary>
/// Base implementation for command states.
/// </summary>
public abstract class TelegramBotCommandStateBase : ITelegramBotCommandState
{
	/// <summary>
	/// Gets the UTC timestamp when this state was created.
	/// </summary>
	public DateTime CreatedAt { get; }

	/// <summary>
	/// Gets or sets the UTC timestamp when this state was last modified.
	/// </summary>
	public DateTime LastModifiedAt { get; set; }

	protected TelegramBotCommandStateBase()
	{
		CreatedAt = DateTime.UtcNow;
		LastModifiedAt = CreatedAt;
	}
}
