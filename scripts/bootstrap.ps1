# APEX repo bootstrap — run once from PowerShell in the repo root.
# Usage: PS> cd C:\Users\brenn\Repo\apex-game ; .\scripts\bootstrap.ps1

param(
    [string]$RemoteUrl = "https://github.com/LilxBean/apex-game.git"
)

$ErrorActionPreference = "Stop"

Write-Host "APEX bootstrap starting..." -ForegroundColor Cyan

# 0. Sanity: we must be in the repo root
if (-not (Test-Path ".\CLAUDE.md")) {
    throw "Run this from the repo root (C:\Users\brenn\Repo\apex-game). CLAUDE.md not found."
}

# 1. Clean any broken .git directory
if (Test-Path ".\.git") {
    Write-Host "Removing existing .git directory..." -ForegroundColor Yellow
    Remove-Item -Recurse -Force ".\.git"
}

# 2. git init on main
Write-Host "git init (branch: main)..." -ForegroundColor Cyan
git init -b main

# 3. Git LFS — install and track
Write-Host "Configuring Git LFS..." -ForegroundColor Cyan
git lfs install --local
# .gitattributes already lists the LFS-tracked types; nothing else to do.

# 4. Stage + initial commit
Write-Host "Creating initial commit..." -ForegroundColor Cyan
git add -A
git commit -m "Initial scaffold: gitignore, gitattributes, docs, CLAUDE.md"

# 5. Remote + push (only if remote argument given / reachable)
Write-Host "Setting remote origin -> $RemoteUrl" -ForegroundColor Cyan
git remote remove origin 2>$null
git remote add origin $RemoteUrl

Write-Host ""
Write-Host "Done. Next steps:" -ForegroundColor Green
Write-Host "  1. Make sure the GitHub repo exists at $RemoteUrl (create it empty on github.com)."
Write-Host "  2. Push:  git push -u origin main"
Write-Host ""
Write-Host "Then open Unity Hub and create a new 2D (URP) project pointing at this folder."
