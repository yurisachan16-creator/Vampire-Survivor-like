param(
    [string]$ProjectRoot = (Resolve-Path ".").Path,
    [switch]$Strict
)

$ErrorActionPreference = "Stop"

function Add-Issue {
    param([string]$Message)
    $script:Issues.Add($Message) | Out-Null
}

function Add-Warn {
    param([string]$Message)
    $script:Warnings.Add($Message) | Out-Null
}

$Issues = New-Object System.Collections.Generic.List[string]
$Warnings = New-Object System.Collections.Generic.List[string]

Push-Location $ProjectRoot
try {
    Write-Host "Inspecting project structure under: $ProjectRoot"

    $requiredDirs = @(
        "Assets/Scripts/Global",
        "Assets/Scripts/Game",
        "Assets/Scripts/System",
        "Assets/Scripts/UI",
        "Assets/Scripts/Localization",
        "Assets/Scripts/Config",
        "Assets/StreamingAssets/Config",
        "Assets/StreamingAssets/Localization",
        "Docs",
        "Tools"
    )

    foreach ($dir in $requiredDirs) {
        if (-not (Test-Path $dir)) {
            Add-Issue "Missing required directory: $dir"
        }
    }

    $uiScriptsInGame = Get-ChildItem "Assets/Scripts/Game" -File -Filter "UI*.cs" -ErrorAction SilentlyContinue
    foreach ($file in $uiScriptsInGame) {
        Add-Issue "UI script in Game folder: $($file.FullName.Replace($ProjectRoot + '\', ''))"
    }

    $misplacedEnemyDesigner = "Assets/Scripts/Game/Enemy.Designer.cs"
    if (Test-Path $misplacedEnemyDesigner) {
        Add-Issue "Misplaced designer file: $misplacedEnemyDesigner (move to Assets/Scripts/Game/Enemy/)"
    }

    $emptyDirs = Get-ChildItem "Assets/Scripts" -Directory -Recurse -ErrorAction SilentlyContinue | Where-Object {
        (Get-ChildItem $_.FullName -Force -ErrorAction SilentlyContinue | Measure-Object).Count -eq 0
    }
    foreach ($dir in $emptyDirs) {
        Add-Warn "Empty directory: $($dir.FullName.Replace($ProjectRoot + '\', ''))"
    }

    Write-Host ""
    Write-Host "Summary:"
    Write-Host "  Issues:   $($Issues.Count)"
    Write-Host "  Warnings: $($Warnings.Count)"

    if ($Issues.Count -gt 0) {
        Write-Host ""
        Write-Host "[Issues]"
        foreach ($item in $Issues) {
            Write-Host "  - $item"
        }
    }

    if ($Warnings.Count -gt 0) {
        Write-Host ""
        Write-Host "[Warnings]"
        foreach ($item in $Warnings) {
            Write-Host "  - $item"
        }
    }

    if ($Strict -and $Issues.Count -gt 0) {
        exit 1
    }
}
finally {
    Pop-Location
}
