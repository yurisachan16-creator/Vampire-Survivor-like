param(
    [string]$SourceDir = "Build/Windows",
    [string]$StageDir = "Release/TapTap/Windows/Stage",
    [string]$OutputDir = "Release/TapTap/Windows",
    [string]$NormalizedExeName = "Nightfall Survivors.exe",
    [ValidateSet("zip", "7z")]
    [string]$ArchiveFormat = "zip",
    [string]$ArchiveName = "",
    [switch]$SkipArchive
)

$ErrorActionPreference = "Stop"

function Resolve-ProjectPath([string]$PathValue) {
    if ([System.IO.Path]::IsPathRooted($PathValue)) {
        return $PathValue
    }

    return [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot "..\..\$PathValue"))
}

function Remove-IfExists([string]$PathValue) {
    if (Test-Path $PathValue) {
        Remove-Item -Recurse -Force $PathValue
    }
}

$sourcePath = Resolve-ProjectPath $SourceDir
$stagePath = Resolve-ProjectPath $StageDir
$outputPath = Resolve-ProjectPath $OutputDir

if (-not (Test-Path $sourcePath)) {
    throw "Windows build directory does not exist: $sourcePath"
}

$exe = Get-ChildItem -Path $sourcePath -Filter "*.exe" -File | Where-Object {
    $_.Name -notlike "UnityCrashHandler*"
} | Select-Object -First 1

if (-not $exe) {
    throw "No game executable was found in $sourcePath."
}

$dataDir = Join-Path $sourcePath (($exe.BaseName) + "_Data")
if (-not (Test-Path $dataDir)) {
    throw "Missing data directory: $dataDir"
}

$normalizedBaseName = [System.IO.Path]::GetFileNameWithoutExtension($NormalizedExeName)

Remove-IfExists $stagePath
New-Item -ItemType Directory -Force -Path $stagePath | Out-Null
New-Item -ItemType Directory -Force -Path $outputPath | Out-Null

$stageExePath = Join-Path $stagePath $NormalizedExeName
$stageDataDir = Join-Path $stagePath ($normalizedBaseName + "_Data")

Copy-Item -Path $exe.FullName -Destination $stageExePath -Force
Copy-Item -Path $dataDir -Destination $stageDataDir -Recurse -Force

$runtimeItems = @(
    "UnityPlayer.dll",
    "GameAssembly.dll",
    "baselib.dll"
)

foreach ($itemName in $runtimeItems) {
    $itemPath = Join-Path $sourcePath $itemName
    if (Test-Path $itemPath) {
        Copy-Item -Path $itemPath -Destination $stagePath -Recurse -Force
    }
}

$doNotShipPatterns = @(
    "*_BurstDebugInformation_DoNotShip*",
    "*_BackUpThisFolder_ButDontShipItWithYourGame*"
)

foreach ($pattern in $doNotShipPatterns) {
    Get-ChildItem -Path $stagePath -Filter $pattern -Force -ErrorAction SilentlyContinue | ForEach-Object {
        Remove-Item -Path $_.FullName -Recurse -Force
    }
}

if ($SkipArchive) {
    Write-Host "Prepared clean stage directory: $stagePath"
    exit 0
}

if ([string]::IsNullOrWhiteSpace($ArchiveName)) {
    $ArchiveName = "{0}-taptap-windows" -f $normalizedBaseName
}

switch ($ArchiveFormat) {
    "zip" {
        $archivePath = Join-Path $outputPath ($ArchiveName + ".zip")
        if (Test-Path $archivePath) {
            Remove-Item -Force $archivePath
        }

        Compress-Archive -Path (Join-Path $stagePath "*") -DestinationPath $archivePath -CompressionLevel Optimal
        Write-Host "Created zip archive: $archivePath"
    }
    "7z" {
        $sevenZip = Get-Command 7z -ErrorAction SilentlyContinue
        if (-not $sevenZip) {
            throw "7z command was not found. Install 7-Zip or use -ArchiveFormat zip."
        }

        $archivePath = Join-Path $outputPath ($ArchiveName + ".7z")
        if (Test-Path $archivePath) {
            Remove-Item -Force $archivePath
        }

        Push-Location $stagePath
        try {
            & $sevenZip.Source a -t7z $archivePath * | Out-Host
        }
        finally {
            Pop-Location
        }

        Write-Host "Created 7z archive: $archivePath"
    }
}
