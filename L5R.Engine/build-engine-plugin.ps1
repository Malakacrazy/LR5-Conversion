<#
.SYNOPSIS
Builds L5R.Engine's netstandard2.1 target for the Unity client.

.DESCRIPTION
The actual sync into L5R.Client's Assets/Scripts/Engine/Plugins/ now happens
automatically as an MSBuild target (CopyToUnityClient, in L5R.Engine.csproj)
on every netstandard2.1 build - from `dotnet build`, an IDE, or this script -
so there's only one place the destination path is defined. This script is
just a convenience wrapper for a clean, deliberate Release rebuild (e.g.
after pulling engine changes, or for CI); day-to-day iterative builds from an
IDE sync just as automatically without running this at all.

System.Text.Json (a NuGet dependency of the netstandard2.1 build, since
Unity's own netstandard2.1 API surface doesn't include it) is restored into
the Unity project separately via NuGetForUnity (see Assets/packages.config)
- not something this script or the MSBuild target handles. If it hasn't been
restored yet, open the Unity Editor once and let NuGetForUnity auto-restore
on load.

.PARAMETER ClientPluginsDir
Overrides where the DLL gets copied (passed through to MSBuild's
UnityClientPluginsDir property). Defaults to the csproj's own default - the
sibling L5R.Client checkout this was originally set up against.
#>
param(
    [string]$ClientPluginsDir
)

$ErrorActionPreference = "Stop"

$srcProject = Join-Path $PSScriptRoot "src\L5R.Engine.csproj"
if (-not (Test-Path $srcProject)) {
    throw "Could not find $srcProject - run this script from its own location inside the L5R.Engine repo."
}

$buildArgs = @($srcProject, "-c", "Release", "-f", "netstandard2.1", "--nologo")
if ($ClientPluginsDir) {
    $buildArgs += "-p:UnityClientPluginsDir=$ClientPluginsDir"
}

Write-Host "Building L5R.Engine (netstandard2.1, Release)..."
dotnet build @buildArgs
if ($LASTEXITCODE -ne 0) { throw "dotnet build failed with exit code $LASTEXITCODE" }
