var target = Argument("target", "Default");

Task("Build")
  .Does(() =>
{
  DotNetCoreBuild("./src/Typed.sln");

});

Task("Test")
  .Does(() =>
{
  var settings = new DotNetCoreTestSettings
  {
    Verbosity = DotNetCoreVerbosity.Minimal
  };

  DotNetCoreTest("./src/Typed.sln", settings);
})
;

Task("Default")
  .IsDependentOn("Build")
  .IsDependentOn("Test")
;

Task("Pack")
  .IsDependentOn("Build")
  .IsDependentOn("Test")
  .Does(() =>
{
  Pack("Typed.With", new [] { "netstandard2.0", "netcoreapp2.0" });
})
;

RunTarget(target);

public void Pack(string projectName, string[] targets) 
{
  var buildSettings = new DotNetCoreMSBuildSettings()
    .WithProperty("NuspecFile", $"../../nuget/{projectName}.nuspec")
    .WithProperty("NuspecBasePath", "bin/Release");
  var settings = new DotNetCorePackSettings
  {
    MSBuildSettings = buildSettings,
    Verbosity = DotNetCoreVerbosity.Minimal,
    Configuration = "Release",
    IncludeSource = true,
    IncludeSymbols = true,
    OutputDirectory = "./nuget"
  };
  DotNetCorePack($"./src/{projectName}/{projectName}.csproj", settings);

}
