var FACEBOOK_SDK_VERSION = "14.1.0";
var NUGET_VERSION = "14.1.0.0";

var CORE_KIT_VERSION           = FACEBOOK_SDK_VERSION;
var FACEBOOK_SDKS_VERSION      = FACEBOOK_SDK_VERSION;
var LOGIN_KIT_VERSION          = FACEBOOK_SDK_VERSION;

var CORE_KIT_NUGET_VERSION           = NUGET_VERSION;
var FACEBOOK_SDKS_NUGET_VERSION      = NUGET_VERSION;
var LOGIN_KIT_NUGET_VERSION          = NUGET_VERSION;

// Artifacts available to be built.
Artifact CORE_KIT_ARTIFACT           = new Artifact ("CoreKit",           CORE_KIT_NUGET_VERSION,           "10.0");
Artifact FACEBOOK_SDKS_ARTIFACT      = new Artifact ("FacebookSdks",      FACEBOOK_SDKS_NUGET_VERSION,      "10.0");
Artifact LOGIN_KIT_ARTIFACT          = new Artifact ("LoginKit",          LOGIN_KIT_NUGET_VERSION,          "10.0");

var ARTIFACTS = new Dictionary<string, Artifact> {
	{ "CoreKit", CORE_KIT_ARTIFACT },
	{ "FacebookSdks", FACEBOOK_SDKS_ARTIFACT },
	{ "LoginKit", LOGIN_KIT_ARTIFACT },
};

void SetArtifactsDependencies ()
{
	CORE_KIT_ARTIFACT.Dependencies           = null;
	FACEBOOK_SDKS_ARTIFACT.Dependencies      = new [] { CORE_KIT_ARTIFACT, LOGIN_KIT_ARTIFACT, };
	LOGIN_KIT_ARTIFACT.Dependencies          = new [] { CORE_KIT_ARTIFACT };
}

void SetArtifactsPodSpecs ()
{
	CORE_KIT_ARTIFACT.PodSpecs = new [] {
		PodSpec.Create ("FBSDKCoreKit", CORE_KIT_VERSION),
		PodSpec.Create ("FBSDKCoreKit_Basics", CORE_KIT_VERSION),
		PodSpec.Create ("FBAEMKit", CORE_KIT_VERSION)
	};
	FACEBOOK_SDKS_ARTIFACT.PodSpecs = new [] {
		PodSpec.Create ("FacebookSdks", FACEBOOK_SDKS_VERSION, frameworkSource: FrameworkSource.Custom)
	};
	LOGIN_KIT_ARTIFACT.PodSpecs = new [] {
		PodSpec.Create ("FBSDKLoginKit", LOGIN_KIT_VERSION)
	};
}

void SetArtifactsExtraPodfileLines ()
{
}

void SetArtifactsSamples ()
{
	CORE_KIT_ARTIFACT.Samples           = null;
	FACEBOOK_SDKS_ARTIFACT.Samples      = new [] { "HelloFacebook" };
	LOGIN_KIT_ARTIFACT.Samples          = null;
}
