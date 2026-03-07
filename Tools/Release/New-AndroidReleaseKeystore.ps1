param(
    [string]$KeystorePath = "",
    [string]$Alias = "nightfallsurvivors-release",
    [string]$StorePass = $env:NIGHTFALL_KEYSTORE_PASS,
    [string]$KeyPass = $env:NIGHTFALL_KEYALIAS_PASS,
    [string]$DistinguishedName = "CN=Yurisa Project, OU=Game, O=Yurisa Project, L=Shanghai, S=Shanghai, C=CN",
    [int]$ValidityDays = 10000,
    [switch]$Force
)

$ErrorActionPreference = "Stop"

if ([string]::IsNullOrWhiteSpace($KeystorePath)) {
    $KeystorePath = [System.IO.Path]::Combine($env:USERPROFILE, ".keystores", "NightfallSurvivors", "nightfallsurvivors-release.keystore")
}

function Get-KeytoolPath {
    $directCommand = Get-Command keytool -ErrorAction SilentlyContinue
    if ($directCommand) {
        return $directCommand.Source
    }

    if (-not [string]::IsNullOrWhiteSpace($env:JAVA_HOME)) {
        $javaHomeKeytool = Join-Path $env:JAVA_HOME "bin\\keytool.exe"
        if (Test-Path $javaHomeKeytool) {
            return $javaHomeKeytool
        }
    }

    $unityEditorsRoot = "C:\\Program Files\\Unity\\Hub\\Editor"
    if (Test-Path $unityEditorsRoot) {
        $unityKeytool = Get-ChildItem -Path $unityEditorsRoot -Directory | Sort-Object Name -Descending | ForEach-Object {
            Join-Path $_.FullName "Editor\\Data\\PlaybackEngines\\AndroidPlayer\\OpenJDK\\bin\\keytool.exe"
        } | Where-Object { Test-Path $_ } | Select-Object -First 1

        if ($unityKeytool) {
            return $unityKeytool
        }
    }

    return $null
}

if ([string]::IsNullOrWhiteSpace($StorePass)) {
    throw "StorePass is empty. Pass -StorePass or set NIGHTFALL_KEYSTORE_PASS in your local environment."
}

if ([string]::IsNullOrWhiteSpace($KeyPass)) {
    throw "KeyPass is empty. Pass -KeyPass or set NIGHTFALL_KEYALIAS_PASS in your local environment."
}

$keytoolPath = Get-KeytoolPath
if (-not $keytoolPath) {
    throw "Could not find keytool. Install Unity Android Build Support or set JAVA_HOME."
}

$keystoreDirectory = Split-Path -Parent $KeystorePath
New-Item -ItemType Directory -Path $keystoreDirectory -Force | Out-Null

if ((Test-Path $KeystorePath) -and (-not $Force)) {
    throw "Keystore already exists: $KeystorePath. Re-run with -Force only if you intend to replace it."
}

if (Test-Path $KeystorePath) {
    Remove-Item -Force $KeystorePath
}

& $keytoolPath `
    -genkeypair `
    -v `
    -keystore $KeystorePath `
    -alias $Alias `
    -storepass $StorePass `
    -keypass $KeyPass `
    -keyalg RSA `
    -keysize 2048 `
    -validity $ValidityDays `
    -dname $DistinguishedName

if (-not (Test-Path $KeystorePath)) {
    throw "Keystore generation failed: $KeystorePath"
}

Write-Host "Created Android release keystore: $KeystorePath" -ForegroundColor Green
Write-Host "Alias: $Alias" -ForegroundColor Green
Write-Host "Next step: open Unity Publishing Settings and enter the keystore/key alias passwords locally." -ForegroundColor Yellow
