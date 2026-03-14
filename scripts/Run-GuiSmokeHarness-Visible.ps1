param(
    [string]$Scenario = "boot-to-combat",
    [string]$Provider = "auto",
    [string]$RunRoot = ""
)

$ErrorActionPreference = "Stop"

$repoRoot = Split-Path -Parent $PSScriptRoot
if ([string]::IsNullOrWhiteSpace($RunRoot)) {
    $stamp = Get-Date -Format "yyyyMMdd-HHmmss"
    $RunRoot = Join-Path $repoRoot ("artifacts\gui-smoke\visible-{0}" -f $stamp)
}

$command = @"
Set-Location '$repoRoot'
dotnet run --project src\Sts2GuiSmokeHarness -- run --scenario $Scenario --provider $Provider --run-root '$RunRoot'
"@

Start-Process powershell.exe -WorkingDirectory $repoRoot -ArgumentList @(
    "-NoExit",
    "-ExecutionPolicy", "Bypass",
    "-Command", $command
)

Write-Host "Started visible smoke harness console."
Write-Host "Run root: $RunRoot"
