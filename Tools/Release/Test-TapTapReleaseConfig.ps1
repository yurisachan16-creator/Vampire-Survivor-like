param(
    [string]$ProjectSettingsPath = "ProjectSettings/ProjectSettings.asset",
    [switch]$RequireKeystore
)

$ErrorActionPreference = "Stop"

function Resolve-ProjectPath([string]$PathValue) {
    if ([System.IO.Path]::IsPathRooted($PathValue)) {
        return $PathValue
    }

    return [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot "..\..\$PathValue"))
}

function Read-Setting([string[]]$Lines, [string]$Prefix) {
    foreach ($line in $Lines) {
        $trimmed = $line.Trim()
        if ($trimmed.StartsWith($Prefix)) {
            return $trimmed.Substring($Prefix.Length).Trim()
        }
    }

    return $null
}

$projectSettingsFullPath = Resolve-ProjectPath $ProjectSettingsPath
if (-not (Test-Path $projectSettingsFullPath)) {
    throw "ProjectSettings.asset not found: $projectSettingsFullPath"
}

$lines = Get-Content $projectSettingsFullPath
$issues = New-Object System.Collections.Generic.List[string]

$bundleVersion = Read-Setting $lines "bundleVersion:"
$androidAppId = Read-Setting $lines "Android:"
$standaloneAppId = Read-Setting $lines "Standalone:"
$versionCode = Read-Setting $lines "AndroidBundleVersionCode:"
$minSdk = Read-Setting $lines "AndroidMinSdkVersion:"
$targetSdk = Read-Setting $lines "AndroidTargetSdkVersion:"
$targetArchitectures = Read-Setting $lines "AndroidTargetArchitectures:"
$keystoreName = Read-Setting $lines "AndroidKeystoreName:"
$keyaliasName = Read-Setting $lines "AndroidKeyaliasName:"

if ($bundleVersion -ne "0.12.0") {
    $issues.Add("bundleVersion is '$bundleVersion' but expected '0.12.0'.")
}

if ($androidAppId -eq "com.DefaultCompany.VampireSurvivorLike" -or [string]::IsNullOrWhiteSpace($androidAppId)) {
    $issues.Add("Android applicationId is still default or empty.")
}

if ($standaloneAppId -eq "com.DefaultCompany.2DProject" -or [string]::IsNullOrWhiteSpace($standaloneAppId)) {
    $issues.Add("Standalone applicationId is still default or empty.")
}

if ([int]$versionCode -le 1) {
    $issues.Add("Android versionCode must be greater than 1.")
}

if ([int]$minSdk -lt 23) {
    $issues.Add("Android min SDK must be at least 23.")
}

if ([int]$targetSdk -le 0) {
    $issues.Add("Android target SDK cannot be Auto.")
}

if ([int]$targetArchitectures -ne 2) {
    $issues.Add("Android target architectures are not ARM64-only.")
}

if ($RequireKeystore) {
    if ([string]::IsNullOrWhiteSpace($keystoreName)) {
        $issues.Add("AndroidKeystoreName is empty.")
    }

    if ([string]::IsNullOrWhiteSpace($keyaliasName)) {
        $issues.Add("AndroidKeyaliasName is empty.")
    }
}

if ($issues.Count -gt 0) {
    Write-Host "TapTap release configuration validation failed:" -ForegroundColor Red
    foreach ($issue in $issues) {
        Write-Host "- $issue" -ForegroundColor Red
    }
    exit 1
}

Write-Host "TapTap release configuration validation passed." -ForegroundColor Green
