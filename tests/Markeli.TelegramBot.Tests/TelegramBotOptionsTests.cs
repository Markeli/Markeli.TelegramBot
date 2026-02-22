using Xunit;

namespace Markeli.TelegramBot.Tests;

public class TelegramBotOptionsTests
{
	[Fact]
	public void Validate_WithValidOptions_ReturnsNoErrors()
	{
		var options = new TelegramBotOptions
		{
			ApiToken = "test-token",
			Password = "test-password",
			MaxDegreeOfParallelism = 5
		};

		var errors = options.Validate();

		Assert.Empty(errors);
	}

	[Fact]
	public void Validate_WithEmptyApiToken_ReturnsError()
	{
		var options = new TelegramBotOptions
		{
			ApiToken = "",
			Password = "test-password"
		};

		var errors = options.Validate();

		Assert.Contains(errors, e => e.Contains(nameof(TelegramBotOptions.ApiToken)));
	}

	[Fact]
	public void Validate_WithEmptyPassword_ReturnsError()
	{
		var options = new TelegramBotOptions
		{
			ApiToken = "test-token",
			Password = ""
		};

		var errors = options.Validate();

		Assert.Contains(errors, e => e.Contains(nameof(TelegramBotOptions.Password)));
	}

	[Fact]
	public void Validate_WithZeroParallelism_ReturnsError()
	{
		var options = new TelegramBotOptions
		{
			ApiToken = "test-token",
			Password = "test-password",
			MaxDegreeOfParallelism = 0
		};

		var errors = options.Validate();

		Assert.Contains(errors, e => e.Contains(nameof(TelegramBotOptions.MaxDegreeOfParallelism)));
	}

	[Fact]
	public void AssertValid_WithInvalidOptions_ThrowsInvalidOperationException()
	{
		var options = new TelegramBotOptions
		{
			ApiToken = "",
			Password = ""
		};

		Assert.Throws<InvalidOperationException>(() => options.AssertValid());
	}

	[Fact]
	public void DefaultValues_AreCorrect()
	{
		var options = new TelegramBotOptions();

		Assert.Equal(10, options.MaxDegreeOfParallelism);
		Assert.Empty(options.AllowedChatIds);
		Assert.Null(options.QueuePersistenceFilePath);
	}
}
