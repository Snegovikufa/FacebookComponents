#addin nuget:?package=Cake.FileHelpers&version=3.2.1

var FB_VERSION = "14.1.0";
var NUGET_VERSION = "14.1.0";

var BUILD_COMMIT = EnvironmentVariable("BUILD_COMMIT") ?? "DEV";
var BUILD_NUMBER = EnvironmentVariable("BUILD_NUMBER") ?? "DEBUG";
var BUILD_TIMESTAMP = DateTime.UtcNow.ToString();

var TARGET = Argument ("t", Argument ("target", "ci"));

var ARTIFACTS = new List<ArtifactInfo> {
	new ArtifactInfo("facebook-core", FB_VERSION, NUGET_VERSION),
	new ArtifactInfo("facebook-common", FB_VERSION, NUGET_VERSION),
	new ArtifactInfo("facebook-login", FB_VERSION, NUGET_VERSION),
};

class ArtifactInfo
{
	public ArtifactInfo(string id, string version, string nugetVersion = null)
	{
		ArtifactId = id;
		Version = version;
		NugetVersion = nugetVersion ?? version;
	}

	public string ArtifactId { get;set; }
	public string Version { get;set; }
	public string NugetVersion { get;set; }
}

Task ("externals")
	.Does (() => 
{
	EnsureDirectoryExists("./output");
	EnsureDirectoryExists ("./externals/");

	foreach (var artifact in ARTIFACTS) {
		var url = $"http://search.maven.org/remotecontent?filepath=com/facebook/android/{artifact.ArtifactId}/{artifact.Version}/{artifact.ArtifactId}-{artifact.Version}.aar";
		var pomUrl = $"http://search.maven.org/remotecontent?filepath=com/facebook/android/{artifact.ArtifactId}/{artifact.Version}/{artifact.ArtifactId}-{artifact.Version}.pom";
		var docUrl = $"http://search.maven.org/remotecontent?filepath=com/facebook/android/{artifact.ArtifactId}/{artifact.Version}/{artifact.ArtifactId}-{artifact.Version}-javadoc.jar";

		var aar = $"./externals/{artifact.ArtifactId}.aar";
		if (!FileExists (aar))
			DownloadFile(url, aar);

		var pom = $"./externals/{artifact.ArtifactId}.pom";
		if (!FileExists (pom))
			DownloadFile(pomUrl, pom);

		try {
			var localDocsFile = $"./externals/{artifact.ArtifactId}-javadoc.jar";
			if (!FileExists (localDocsFile))
				DownloadFile(docUrl, localDocsFile);

			EnsureDirectoryExists ($"./externals/{artifact.ArtifactId}-docs/");
			Unzip (localDocsFile, $"./externals/{artifact.ArtifactId}-docs/");
		} catch {}
	}
});

Task ("libs")
	.IsDependentOn("externals")
	.IsDependentOn("ci-setup")
	.Does(() =>
{
	MSBuild("./source/Xamarin.Facebook.sln", c => 
		c.SetConfiguration("Release")
		.WithRestore()
 		.WithTarget("Build")
		.WithProperty("DesignTimeBuild", "false"));
});

Task ("samples")
	.IsDependentOn("libs")
	.Does(() =>
{
	var samples = new string[] { 
		"./samples/HelloFacebookSample.sln",
		"./samples/MessengerSendSample.sln",
	};

	foreach (var sampleSln in samples) {
		MSBuild(sampleSln, c => 
	 		c.SetConfiguration("Release")
 			.WithTarget("Restore"));

		MSBuild(sampleSln, c => 
			c.SetConfiguration("Release")
			.WithTarget("Build")
			.WithProperty("DesignTimeBuild", "false"));
	}
});

Task ("nuget")
	.IsDependentOn("libs")
	.Does(() =>
{
	EnsureDirectoryExists("./output");

	foreach (var art in ARTIFACTS) {
		var csproj = "./source/" + art.ArtifactId + "/" + art.ArtifactId + ".csproj";

		MSBuild(csproj, c => 
			c.SetConfiguration("Release")
			.WithProperty("NoBuild", "true")
			.WithProperty("PackageVersion", art.NugetVersion)
			.WithProperty("PackageOutputPath", MakeAbsolute((DirectoryPath)"./output/").FullPath)
			.WithProperty("DesignTimeBuild", "false")
 			.WithTarget("Pack"));	
	}
});


Task ("ci-setup")
	.WithCriteria (!BuildSystem.IsLocalBuild)
	.Does (() => 
{
	var glob = "./source/**/AssemblyInfo.cs";

	ReplaceTextInFiles(glob, "{BUILD_COMMIT}", BUILD_COMMIT);
	ReplaceTextInFiles(glob, "{BUILD_NUMBER}", BUILD_NUMBER);
	ReplaceTextInFiles(glob, "{BUILD_TIMESTAMP}", BUILD_TIMESTAMP);
});

Task ("clean")
	.Does (() => 
{
	MSBuild("./source/Xamarin.Facebook.sln", c => 
		c.SetConfiguration("Release")
 		.WithTarget("Clean"));

	if (DirectoryExists ("./externals/"))
		DeleteDirectory ("./externals", new DeleteDirectorySettings {
															Recursive = true,
															Force = true
														});
});

Task ("ci")
	.IsDependentOn("externals")
	.IsDependentOn("libs")
	.IsDependentOn("nuget");

RunTarget (TARGET);
