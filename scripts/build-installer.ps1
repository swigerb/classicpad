param(
    [string]$Configuration = "Release",
    [string]$Runtime = "win-x64",
    [string]$Version = "1.0.0",
    [string]$ProductName = "ClassicPad",
    [string]$Manufacturer = "ClassicPad"
)

$ErrorActionPreference = "Stop"

$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot ".." )).Path
$publishDir = Join-Path $repoRoot "artifacts\publish\$Runtime"
$installerDir = Join-Path $repoRoot "artifacts\installer"
$wxsPath = Join-Path $repoRoot "installer\ClassicPadInstaller.wxs"
$iconPath = Join-Path $repoRoot "ClassicPad\Assets\classicpad.ico"
$msiName = "${ProductName}-${Version}-${Runtime}.msi"
$msiPath = Join-Path $installerDir $msiName

Write-Host "Publishing $ProductName ($Configuration, $Runtime) to $publishDir" -ForegroundColor Cyan
$null = New-Item -ItemType Directory -Path $publishDir -Force
$null = New-Item -ItemType Directory -Path $installerDir -Force

$publishArgs = @(
    "ClassicPad/ClassicPad.csproj",
    "-c", $Configuration,
    "-r", $Runtime,
    "--self-contained", "false",
    "/p:UseAppHost=true",
    "-o", $publishDir
)

dotnet publish @publishArgs

if (-not (Get-Command wix -ErrorAction SilentlyContinue)) {
    throw "WiX CLI (wix) is not installed. Install it with `dotnet tool install --global wix`."
}

if (-not (Test-Path $wxsPath)) {
    throw "Missing installer source: $wxsPath"
}

if (-not (Test-Path $iconPath)) {
    throw "Missing icon file: $iconPath"
}

$bindPublishDir = if ($publishDir.EndsWith([IO.Path]::DirectorySeparatorChar)) {
    $publishDir
} else {
    $publishDir + [IO.Path]::DirectorySeparatorChar
}

Write-Host "Building MSI -> $msiPath" -ForegroundColor Cyan

$wixArgs = @(
    "build",
    $wxsPath,
    "-d", "PublishDir=$bindPublishDir",
    "-d", "ProductVersion=$Version",
    "-d", "ProductName=$ProductName",
    "-d", "Manufacturer=$Manufacturer",
    "-d", "IconPath=$iconPath",
    "-o",
    $msiPath
)

wix @wixArgs

Write-Host "MSI created: $msiPath" -ForegroundColor Green
