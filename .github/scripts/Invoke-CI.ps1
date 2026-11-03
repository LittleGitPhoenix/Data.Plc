#Requires -Version 7
param(
    [ValidateSet('Validate', 'Release', 'All')]
    [string] $Mode,
    [string] $ConfigPath,
    [switch] $ShowTags = $true
)

$ErrorActionPreference = 'Stop'

$toolsPath = $env:PHOENIX_REUSABLE_WORKFLOWS_REPOSITORY_PATH
if (-not $toolsPath -or -not (Test-Path $toolsPath)) {
    Write-Error "Set PHOENIX_REUSABLE_WORKFLOWS_REPOSITORY_PATH to a local clone of https://github.com/LittleGitPhoenix/Automation.Workflows"
    exit 1
}

$repoRoot = [System.IO.Path]::GetFullPath("$PSScriptRoot/../..")
$params = @{ WorkspaceRoot = $repoRoot; ShowTags = $ShowTags }
if ($Mode) { $params['Mode'] = $Mode }
if ($ConfigPath) { $params['ConfigPath'] = [System.IO.Path]::GetFullPath($ConfigPath, $repoRoot) }

& (Join-Path $toolsPath 'scripts/Invoke-CI.ps1') @params
exit $LASTEXITCODE
