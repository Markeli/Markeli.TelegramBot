///////////////////////////////////////////////////////////////////////////////
// ARGUMENTS
///////////////////////////////////////////////////////////////////////////////

var target = Argument<string>("target", "Build");
var configuration = Argument<string>("configuration", "Release");

var artifactsDir = Directory("./artifacts");
var solutionPath = "./Markeli.TelegramBot.sln";

///////////////////////////////////////////////////////////////////////////////
// TASKS
///////////////////////////////////////////////////////////////////////////////

Task("Clean")
	.Does(() =>
	{
		DotNetCoreClean(solutionPath);
		CleanDirectory(artifactsDir);
		EnsureDirectoryExists(artifactsDir);
	});

Task("Build")
	.IsDependentOn("Clean")
	.Does(() =>
	{
		var settings = new DotNetCoreBuildSettings
		{
			Configuration = configuration
		};

		DotNetCoreBuild(solutionPath, settings);
	});

Task("Test")
	.IsDependentOn("Build")
	.Does(() =>
	{
		var settings = new DotNetCoreTestSettings
		{
			Configuration = configuration,
			NoBuild = true
		};

		DotNetCoreTest(solutionPath, settings);
	});

Task("Pack")
	.IsDependentOn("Test")
	.Does(() =>
	{
		var settings = new DotNetCorePackSettings
		{
			Configuration = configuration,
			OutputDirectory = artifactsDir,
			NoBuild = true
		};

		DotNetCorePack("./src/Markeli.TelegramBot/Markeli.TelegramBot.csproj", settings);
	});

Task("Push")
	.IsDependentOn("Pack")
	.Does(() =>
	{
		var apiKey = EnvironmentVariable("NUGET_API_KEY");
		if (String.IsNullOrWhiteSpace(apiKey))
			throw new Exception("NUGET_API_KEY environment variable is not set");

		var packages = GetFiles($"{artifactsDir}/*.nupkg");
		foreach (var package in packages)
		{
			DotNetCoreNuGetPush(package.FullPath, new DotNetCoreNuGetPushSettings
			{
				Source = "https://api.nuget.org/v3/index.json",
				ApiKey = apiKey
			});
		}
	});

RunTarget(target);
