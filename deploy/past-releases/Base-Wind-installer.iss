#define PackageName      "Base Wind"
#define PackageNameLong  "Base Wind Extension"
#define Version          "1.2.2"
#define ReleaseType      "official"

#define CoreVersion      "5.1"
#define CoreReleaseAbbr  ""

#include AddBackslash(GetEnv("LANDIS_DEPLOY")) + "package (Setup section).iss"

#if ReleaseType != "official"
  #define Configuration  "debug"
#else
  #define Configuration  "release"
#endif


[Files]

; Extension's assembly and program database file
#define SrcBuildDir     "..\src\build"
#define ConfigBuildDir  SrcBuildDir + "\" + Configuration
#define ExtensionDll    "Landis.Extension.BaseWind.dll"
#define ExtensionPdb    ChangeFileExt(ExtensionDll, "pdb")
Source: {#ConfigBuildDir}\{#ExtensionDll}; DestDir: {app}\bin
#if ReleaseType != "official"
  ; Source: {#ConfigBuildDir}\{#ExtensionPdb}; DestDir: {app}\bin
#endif

; User guide
#define UserGuide "LANDIS-II " + PackageName + " v" + MajorMinor + " User Guide.pdf"
Source: ..\docs\{#UserGuide}; DestDir: {app}\docs

; Sample input files
Source: examples\*; DestDir: {app}\examples

; Extension info file
#define ExtInfoSrc  PackageName + ".txt"
#define ExtInfo     PackageName + " " + MajorMinor + ".txt"
Source: {#ExtInfoSrc}; DestDir: {#LandisPlugInDir}; DestName: {#ExtInfo}


[Run]
;; Run plug-in admin tool to add the entry for the plug-in
#define PlugInAdminTool  CoreBinDir + "\Landis.PlugIns.Admin.exe"

Filename: {#PlugInAdminTool}; Parameters: "remove ""{#PackageName}"" "; WorkingDir: {#LandisPlugInDir}
Filename: {#PlugInAdminTool}; Parameters: "add ""{#ExtInfo}"" "; WorkingDir: {#LandisPlugInDir}

[UninstallRun]
;; Run plug-in admin tool to remove extension's entry


[Code]
{ Check for other prerequisites during the setup initialization }

#define BundledWithCore

#include AddBackslash(LandisDeployDir) + "package (Code section).iss"

//-----------------------------------------------------------------------------

function InitializeSetup_FirstPhase(): Boolean;
begin
  Result := True
end;
