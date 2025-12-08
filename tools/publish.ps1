[CmdletBinding(SupportsShouldProcess = $True, DefaultParameterSetName = 'Individual')]

param (
    [Alias("d")]
    [switch] $dry
)

$dryRun = $dry.IsPresent

. "./functions.ps1"

Clear-Host

$projectRoot = Join-Path $PSScriptRoot "..\NVMQuickSwitch"
$versionPath = Join-Path $projectRoot "\VERSION"
$version = [Version](Get-Content -Path $versionPath -Raw).Trim()

Write-Host "Publishing a new version of NVM Quick Switch" -BackgroundColor White -ForegroundColor Black
Write-Host "Current version: $version" -BackgroundColor White -ForegroundColor Black
Write-Host ""

# User input for version increment.

try {
    $newVersion = Get-NewVersion -CurrentVersion $version
}
catch {
    Exit-WithRollBack -Message $_
}

if (-not (Show-YesNoQuestion -Question "Publish version $($newVersion.toString())?")) {
    Exit-WithRollBack -Message "Aborted by user."
}

# Update global VERSION file.

Write-Host "Updating version file..." -ForegroundColor Cyan

Set-Content -Path $versionPath -Value $newVersion.toString()

Write-Host "Version updated to $($newVersion.toString())." -ForegroundColor Green

# Build and publish project.

$projectPath = Join-Path $projectRoot "\NVMQuickSwitch.csproj"
$publishProfile = "Default"

Write-Host "Publishing project..." -ForegroundColor Cyan

& dotnet publish $projectPath -c Release /p:PublishProfile=$publishProfile

Write-Host "Successfully published." -ForegroundColor Green

# Create installer using Inno Setup

Write-Host "Creating installer..." -ForegroundColor Cyan

try {
    $iscc = Get-ISCCPath

    & $iscc /Qp "/DAppVersion=$($newVersion.toString())" "../inno-build.iss"

    Write-Host "Successfully created installer." -ForegroundColor Green
}
catch {
    Exit-WithRollBack -Message $_
}

# Zip up release files.

Write-Host "Zipping up release files..." -ForegroundColor Cyan

$publishProfilePath = Join-Path $projectRoot "\Properties\PublishProfiles\$publishProfile.pubxml"
$xml = [xml](Get-Content $publishProfilePath)
$publishDir = Join-Path $projectRoot $xml.Project.PropertyGroup.PublishDir
$outputDir = Join-Path $PSScriptRoot "..\release"
$outputPath = Join-Path $outputDir "\NVMQuickSwitch-$($newVersion.toString()).zip"

if (Test-Path -Path $publishDir) {
    New-Item -ItemType Directory -Path $outputDir -Force | Out-Null
    Compress-Archive -Path (Join-Path $publishDir "\*") -DestinationPath $outputPath -Force
}
else {
    Exit-WithRollBack -Message "Cannot find publish directory."
}

Write-Host "Release files successfully zipped." -ForegroundColor Green

# Tag and create new release.

Write-Host "Tagging and creating new release..." -ForegroundColor Cyan

try {
    $tag = "v$($newVersion.toString())"
    $title = "Version $($newVersion.toString())"

    Invoke-GitTagAndPush -ReleaseTag $tag -DryRun $dryRun

    Write-Host "Tag created and pushed successfully." -ForegroundColor Green
}
catch {
    Exit-WithRollBack -Message $_
}

# Generate GitHub release URL.

Write-Host "Generating GitHub release..." -ForegroundColor Cyan

try {
    $releaseUrl = Get-ReleaseUrl -ReleaseTag $tag -ReleaseTitle $title

    Write-Host "Release generated successfully." -ForegroundColor Green

    if (-not $dryRun -and (Show-YesNoQuestion -Question "Open release in GitHub?")) {
        Start-Process $releaseUrl
    }
}
catch {
    Exit-WithRollBack -Message $_
}

Write-Host $releaseUrl
Write-Host "Finished." -ForegroundColor Green

if ($dryRun) {
    Exit-WithRollBack -Message "Dry run complete. Rolling back to $($version.ToString())."
}