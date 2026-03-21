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
		Assert.Null(options.HttpProxyUrl);
	}

	[Fact]
	public void Validate_WithNullHttpProxyUrl_ReturnsNoErrors()
	{
		var options = new TelegramBotOptions
		{
			ApiToken = "test-token",
			Password = "test-password",
			HttpProxyUrl = null
		};

		var errors = options.Validate();

		Assert.Empty(errors);
	}

	[Theory]
	[InlineData("http://proxy.example.com:8080")]
	[InlineData("https://proxy.example.com:3128")]
	[InlineData("http://user:pass@proxy.example.com:8080")]
	public void Validate_WithValidHttpProxyUrl_ReturnsNoErrors(string proxyUrl)
	{
		var options = new TelegramBotOptions
		{
			ApiToken = "test-token",
			Password = "test-password",
			HttpProxyUrl = proxyUrl
		};

		var errors = options.Validate();

		Assert.Empty(errors);
	}

	[Theory]
	[InlineData("not-a-url")]
	[InlineData("just-text")]
	public void Validate_WithInvalidHttpProxyUrl_ReturnsError(string proxyUrl)
	{
		var options = new TelegramBotOptions
		{
			ApiToken = "test-token",
			Password = "test-password",
			HttpProxyUrl = proxyUrl
		};

		var errors = options.Validate();

		Assert.Contains(errors, e => e.Contains(nameof(TelegramBotOptions.HttpProxyUrl)));
	}

	[Theory]
	[InlineData("ftp://proxy.example.com:21")]
	[InlineData("socks5://proxy.example.com:1080")]
	public void Validate_WithNonHttpHttpProxyUrl_ReturnsError(string proxyUrl)
	{
		var options = new TelegramBotOptions
		{
			ApiToken = "test-token",
			Password = "test-password",
			HttpProxyUrl = proxyUrl
		};

		var errors = options.Validate();

		Assert.Contains(errors, e => e.Contains(nameof(TelegramBotOptions.HttpProxyUrl)));
	}
}
