#tool nuget:?package=NUnit.ConsoleRunner&version=3.4.0
#tool nuget:?package=xunit.runner.console
//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");

//////////////////////////////////////////////////////////////////////
// PREPARATION
//////////////////////////////////////////////////////////////////////

// Define directories.
var projects = new []{
    "BMSF.Reactive.Utilities",
    "BMSF.Reactive.Utilities.Tests",
    "BMSF.Reactive.Validation",
    "BMSF.Reactive.Validation.Tests",
    "BMSF.Utilities",
    "BMSF.Utilities.Tests",
    "BMSF.WPF.AutoCompleteControls",
    "BMSF.WPF.DemoApp",
    "BMSF.WPF.Utilities",
    "BMSF.WPF.Utilities.Tests"
};
// Define directories.
var buildDirs = projects.Select(x => Directory("./"+ x + "/bin") + Directory(configuration));
var rootAbsoluteDir = MakeAbsolute(Directory("./")).FullPath;
//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Task("Clean")
    .Does(() =>
{
    foreach (var dir in buildDirs)
    {
        CleanDirectory(dir);   
    }
});

Task("Restore-NuGet-Packages")
    .IsDependentOn("Clean")
    .Does(() =>
{
    NuGetRestore("./BMSF.sln");
});

Task("Build")
    .IsDependentOn("Restore-NuGet-Packages")
    .Does(() =>
{
    MSBuild("./BMSF.sln", settings =>
    settings.SetConfiguration(configuration));
});

Task("Run-Unit-Tests")
    .IsDependentOn("Build")
    .Does(() =>
{
    XUnit2("./**/bin/" + configuration + "/*.Tests.dll");
});
Task("Pack")
    .IsDependentOn("Build")
    .Does(() =>
{
    var nuGetPackSettings = new NuGetPackSettings
	{
		OutputDirectory = rootAbsoluteDir + @"\artifacts\",
		IncludeReferencedProjects = true,
		Properties = new Dictionary<string, string>
		{
			{ "Configuration", configuration }
		}
	};
    NuGetPack(@".\BMSF.Utilities\BMSF.Utilities.csproj", nuGetPackSettings);
    NuGetPack(@".\BMSF.Reactive.Utilities\BMSF.Reactive.Utilities.csproj", nuGetPackSettings);
    NuGetPack(@".\BMSF.Reactive.Validation\BMSF.Reactive.Validation.csproj", nuGetPackSettings);
    NuGetPack(@".\BMSF.WPF.Utilities\BMSF.WPF.Utilities.csproj", nuGetPackSettings);
    NuGetPack(@".\BMSF.WPF.AutoCompleteControls\BMSF.WPF.AutoCompleteControls.csproj", nuGetPackSettings);
});

//////////////////////////////////////////////////////////////////////
// TASK TARGETS
//////////////////////////////////////////////////////////////////////

Task("Default")
    .IsDependentOn("Run-Unit-Tests");

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);
