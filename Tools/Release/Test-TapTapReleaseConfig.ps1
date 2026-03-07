param(
    [string]$ProjectSettingsPath = "ProjectSettings/ProjectSettings.asset",
    [switch]$RequireKeystore
)

$ErrorActionPreference = "Stop"

$expectedCompanyName = "Yurisa Project"
$expectedProductName = "Nightfall Survivors"
$expectedBundleVersion = "1.0.0"
$expectedApplicationId = "com.yurisa.nightfallsurvivors"
$expectedVersionCode = 100
$expectedAndroidMinSdk = 23
$expectedAndroidTargetSdk = 35
$expectedAndroidTargetArchitectures = 2
$expectedKeyAlias = "nightfallsurvivors-release"
$expectedKeystorePath = [System.IO.Path]::Combine($env:USERPROFILE, ".keystores", "NightfallSurvivors", "nightfallsurvivors-release.keystore")

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

function Normalize-PathValue([string]$PathValue) {
    if ([string]::IsNullOrWhiteSpace($PathValue)) {
        return ""
    }

    return ([Environment]::ExpandEnvironmentVariables($PathValue)).Replace("\", "/")
}

$projectSettingsFullPath = Resolve-ProjectPath $ProjectSettingsPath
if (-not (Test-Path $projectSettingsFullPath)) {
    throw "ProjectSettings.asset not found: $projectSettingsFullPath"
}

$lines = Get-Content $projectSettingsFullPath
$issues = New-Object System.Collections.Generic.List[string]

$companyName = Read-Setting $lines "companyName:"
$productName = Read-Setting $lines "productName:"
$bundleVersion = Read-Setting $lines "bundleVersion:"
$androidAppId = Read-Setting $lines "Android:"
$standaloneAppId = Read-Setting $lines "Standalone:"
$versionCode = Read-Setting $lines "AndroidBundleVersionCode:"
$minSdk = Read-Setting $lines "AndroidMinSdkVersion:"
$targetSdk = Read-Setting $lines "AndroidTargetSdkVersion:"
$targetArchitectures = Read-Setting $lines "AndroidTargetArchitectures:"
$keystoreName = Read-Setting $lines "AndroidKeystoreName:"
$keyaliasName = Read-Setting $lines "AndroidKeyaliasName:"
$useCustomKeystore = Read-Setting $lines "androidUseCustomKeystore:"
$metroApplicationDescription = Read-Setting $lines "metroApplicationDescription:"
$metroTileShortName = Read-Setting $lines "metroTileShortName:"

if ($companyName -ne $expectedCompanyName) {
    $issues.Add("companyName is '$companyName' but expected '$expectedCompanyName'.")
}

if ($productName -ne $expectedProductName) {
    $issues.Add("productName is '$productName' but expected '$expectedProductName'.")
}

if ($bundleVersion -ne $expectedBundleVersion) {
    $issues.Add("bundleVersion is '$bundleVersion' but expected '$expectedBundleVersion'.")
}

if ($androidAppId -ne $expectedApplicationId) {
    $issues.Add("Android applicationId is '$androidAppId' but expected '$expectedApplicationId'.")
}

if ($standaloneAppId -ne $expectedApplicationId) {
    $issues.Add("Standalone applicationId is '$standaloneAppId' but expected '$expectedApplicationId'.")
}

if ([int]$versionCode -ne $expectedVersionCode) {
    $issues.Add("Android versionCode is '$versionCode' but expected '$expectedVersionCode'.")
}

if ([int]$minSdk -ne $expectedAndroidMinSdk) {
    $issues.Add("Android min SDK is '$minSdk' but expected '$expectedAndroidMinSdk'.")
}

if ([int]$targetSdk -ne $expectedAndroidTargetSdk) {
    $issues.Add("Android target SDK is '$targetSdk' but expected '$expectedAndroidTargetSdk'.")
}

if ([int]$targetArchitectures -ne $expectedAndroidTargetArchitectures) {
    $issues.Add("Android target architectures are not ARM64-only.")
}

if ($useCustomKeystore -ne "1") {
    $issues.Add("androidUseCustomKeystore must be enabled for release builds.")
}

if ($metroApplicationDescription -ne $expectedProductName) {
    $issues.Add("metroApplicationDescription is '$metroApplicationDescription' but expected '$expectedProductName'.")
}

if ($metroTileShortName -ne $expectedProductName) {
    $issues.Add("metroTileShortName is '$metroTileShortName' but expected '$expectedProductName'.")
}

$normalizedKeystorePath = Normalize-PathValue $keystoreName
$normalizedExpectedKeystorePath = Normalize-PathValue $expectedKeystorePath
if ($normalizedKeystorePath -ne $normalizedExpectedKeystorePath) {
    $issues.Add("AndroidKeystoreName is '$keystoreName' but expected '$expectedKeystorePath'.")
}

if ($keyaliasName -ne $expectedKeyAlias) {
    $issues.Add("AndroidKeyaliasName is '$keyaliasName' but expected '$expectedKeyAlias'.")
}

if ($RequireKeystore) {
    if ([string]::IsNullOrWhiteSpace($keystoreName)) {
        $issues.Add("AndroidKeystoreName is empty.")
    } elseif (-not (Test-Path ([Environment]::ExpandEnvironmentVariables($keystoreName)))) {
        $issues.Add("Android keystore file does not exist: $keystoreName")
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
