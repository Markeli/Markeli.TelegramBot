///////////////////////////////////////////////////////////////////////////////
// ARGUMENTS
///////////////////////////////////////////////////////////////////////////////

var target = Argument<string>("target", "Build");
var configuration = Argument<string>("configuration", "Release");
var framework = Argument<string>("framework", "");
var collectCoverage = Argument<bool>("coverage", false);

var artifactsDir = Directory("./artifacts");
var solutionPath = "./Markeli.TelegramBot.sln";
var projectPath = "./src/Markeli.TelegramBot/Markeli.TelegramBot.csproj";

///////////////////////////////////////////////////////////////////////////////
// TASKS
///////////////////////////////////////////////////////////////////////////////

Task("Clean")
	.Does(() =>
	{
		DotNetClean(solutionPath);
		CleanDirectory(artifactsDir);
		EnsureDirectoryExists(artifactsDir);
	});

Task("Build")
	.IsDependentOn("Clean")
	.Does(() =>
	{
		var settings = new DotNetBuildSettings
		{
			Configuration = configuration
		};

		if (!String.IsNullOrEmpty(framework))
			settings.Framework = framework;

		DotNetBuild(solutionPath, settings);
	});

Task("Test")
	.IsDependentOn("Build")
	.Does(() =>
	{
		var settings = new DotNetTestSettings
		{
			Configuration = configuration,
			NoBuild = true
		};

		if (!String.IsNullOrEmpty(framework))
			settings.Framework = framework;

		if (collectCoverage)
		{
			settings.ArgumentCustomization = args => args
				.Append("/p:CollectCoverage=true")
				.Append("/p:CoverletOutputFormat=cobertura")
				.Append($"/p:CoverletOutput={MakeAbsolute(artifactsDir)}/coverage.cobertura.xml");
		}

		DotNetTest(solutionPath, settings);
	});

Task("Coverage-Report")
	.IsDependentOn("Test")
	.Does(() =>
	{
		var reportPath = $"{MakeAbsolute(artifactsDir)}/coverage.cobertura.xml";
		if (!FileExists(reportPath))
			throw new Exception($"Coverage report not found at {reportPath}. Did you run with --coverage=true?");

		StartProcess("dotnet", new ProcessSettings
		{
			Arguments = new ProcessArgumentBuilder()
				.Append("tool").Append("run").Append("reportgenerator")
				.Append($"-reports:{reportPath}")
				.Append($"-targetdir:{MakeAbsolute(artifactsDir)}/coverage-report")
				.Append("-reporttypes:Html;TextSummary")
		});
	});

Task("Pack")
	.IsDependentOn("Build")
	.Does(() =>
	{
		var settings = new DotNetPackSettings
		{
			Configuration = configuration,
			OutputDirectory = artifactsDir,
			NoBuild = true
		};

		DotNetPack(projectPath, settings);
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
			DotNetNuGetPush(package.FullPath, new DotNetNuGetPushSettings
			{
				Source = "https://api.nuget.org/v3/index.json",
				ApiKey = apiKey
			});
		}
	});

RunTarget(target);
