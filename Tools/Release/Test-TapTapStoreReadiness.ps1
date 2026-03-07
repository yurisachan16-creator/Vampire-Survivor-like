param(
    [string]$RootPath = ".",
    [switch]$RequireKeystore
)

$ErrorActionPreference = "Stop"

function Resolve-ProjectPath([string]$PathValue) {
    if ([System.IO.Path]::IsPathRooted($PathValue)) {
        return $PathValue
    }

    return [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot "..\..\$PathValue"))
}

function Add-Issue([System.Collections.Generic.List[string]]$Issues, [string]$Message) {
    $Issues.Add($Message)
}

$projectRoot = Resolve-ProjectPath $RootPath
$issues = New-Object System.Collections.Generic.List[string]

$requiredDocs = @(
    "Docs/TapTap-Store-Draft.md",
    "Docs/TapTap-Backend-Final-Entry.md",
    "Docs/TapTap-Trial-Notice.md",
    "Docs/TapTap-Privacy-Policy-Template.md",
    "Docs/TapTap-Store-Assets-Checklist.md",
    "Docs/TapTap-Submission-Checklist.md"
)

$requiredAssets = @(
    "Release/TapTap/Assets/Icon/icon-main.png",
    "Release/TapTap/Assets/Hero/hero-horizontal.png",
    "Release/TapTap/Assets/Capsule/cover-vertical.png",
    "Release/TapTap/Assets/Screenshots/screenshot-01-start.png",
    "Release/TapTap/Assets/Screenshots/screenshot-02-battle.png",
    "Release/TapTap/Assets/Screenshots/screenshot-03-upgrade.png",
    "Release/TapTap/Assets/Screenshots/screenshot-04-evolution.png",
    "Release/TapTap/Assets/Screenshots/screenshot-05-meta.png",
    "Release/TapTap/Assets/Screenshots/screenshot-06-mobile-ui.png",
    "Release/TapTap/Assets/Policies/privacy-policy-url.txt"
)

$requiredPackageGroups = @(
    @{
        Label = "Windows package artifact"
        Paths = @(
            "Release/TapTap/Windows/Nightfall Survivors-taptap-windows.zip",
            "Release/TapTap/Windows/Nightfall Survivors-taptap-windows.7z"
        )
    },
    @{
        Label = "Android package artifact"
        Paths = @(
            "Build/Android/Development/Nightfall Survivors.apk",
            "Build/Android/Release/Nightfall Survivors.apk",
            "Build/TapTap/Android/Unsigned/Nightfall Survivors.apk"
        )
    }
)

foreach ($doc in $requiredDocs) {
    $fullPath = Join-Path $projectRoot $doc
    if (-not (Test-Path $fullPath)) {
        Add-Issue $issues "Missing document: $doc"
    }
}

foreach ($asset in $requiredAssets) {
    $fullPath = Join-Path $projectRoot $asset
    if (-not (Test-Path $fullPath)) {
        Add-Issue $issues "Missing store asset: $asset"
    }
}

foreach ($packageGroup in $requiredPackageGroups) {
    $existingPath = $null
    foreach ($packagePath in $packageGroup.Paths) {
        $fullPath = Join-Path $projectRoot $packagePath
        if (Test-Path $fullPath) {
            $existingPath = $packagePath
            break
        }
    }

    if (-not $existingPath) {
        Add-Issue $issues ("Missing package artifact: " + ($packageGroup.Paths -join " | "))
    }
}

$releaseConfigScript = Join-Path $projectRoot "Tools/Release/Test-TapTapReleaseConfig.ps1"
if (-not (Test-Path $releaseConfigScript)) {
    Add-Issue $issues "Missing release config validation script."
} else {
    $argList = @("-ExecutionPolicy", "Bypass", "-File", $releaseConfigScript)
    if ($RequireKeystore) {
        $argList += "-RequireKeystore"
    }

    $output = & powershell @argList 2>&1
    if ($LASTEXITCODE -ne 0) {
        foreach ($line in $output) {
            Add-Issue $issues "Release config: $line"
        }
    }
}

if ($issues.Count -gt 0) {
    Write-Host "TapTap store readiness check failed:" -ForegroundColor Red
    foreach ($issue in $issues) {
        Write-Host "- $issue" -ForegroundColor Red
    }
    exit 1
}

Write-Host "TapTap store readiness check passed." -ForegroundColor Green
