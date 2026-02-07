param(
    [switch]$Deep
)

$projectRoot = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)

$targets = @(
    (Join-Path $projectRoot 'Library\Bee\Android\Prj\IL2CPP\Gradle'),
    (Join-Path $projectRoot 'Library\Bee\Android\Prj\Mono\Gradle')
) | Where-Object { Test-Path $_ }

foreach ($t in $targets) {
    Remove-Item -LiteralPath $t -Recurse -Force -ErrorAction SilentlyContinue
    Write-Output ("Removed: " + $t)
}

if ($Deep) {
    $userGradle = Join-Path $env:USERPROFILE '.gradle\caches'
    if (Test-Path $userGradle) {
        Remove-Item -LiteralPath $userGradle -Recurse -Force -ErrorAction SilentlyContinue
        Write-Output ("Removed: " + $userGradle)
    }
}

Write-Output "Done."
