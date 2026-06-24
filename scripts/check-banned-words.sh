#!/usr/bin/env bash
set -euo pipefail

# Keep the legacy standalone lowercase banned term out of source files while
# allowing the intentionally seeded item CSV data.
force_powershell_fallback="${BANNED_WORD_CHECK_FORCE_POWERSHELL:-}"
use_powershell_fallback=false
if [[ -n "$force_powershell_fallback" ]]; then
  use_powershell_fallback=true
elif ! command -v rg >/dev/null 2>&1; then
  use_powershell_fallback=true
fi

if [[ "$use_powershell_fallback" == true ]]; then
  powershell_command=()
  if command -v powershell.exe >/dev/null 2>&1; then
    powershell_command=(powershell.exe -NoProfile -ExecutionPolicy Bypass -Command -)
  elif command -v pwsh >/dev/null 2>&1; then
    powershell_command=(pwsh -NoProfile -Command -)
  fi

  if ((${#powershell_command[@]} > 0)); then
    "${powershell_command[@]}" <<'POWERSHELL'
$ErrorActionPreference = "Stop"
$root = (Get-Location).Path.TrimEnd([char[]]@('\', '/'))
$textFileExtensions = @(
    ".bat",
    ".cmd",
    ".config",
    ".cs",
    ".csproj",
    ".csv",
    ".editorconfig",
    ".json",
    ".md",
    ".props",
    ".ps1",
    ".sln",
    ".targets",
    ".txt",
    ".xaml",
    ".xml",
    ".yaml",
    ".yml"
)
$textFileNames = @(
    ".gitattributes",
    ".gitignore",
    "Dockerfile",
    "Makefile"
)
$matches = Get-ChildItem -Path . -Recurse -File -Force |
    Where-Object {
        $relative = $_.FullName.Substring($root.Length).TrimStart([char[]]@('\', '/')).Replace('\', '/')
        $extension = [System.IO.Path]::GetExtension($relative).ToLowerInvariant()
        $fileName = [System.IO.Path]::GetFileName($relative)
        $relative -notlike ".git/*" -and
            $relative -notmatch '(^|/)(bin|obj|publish)/' -and
            $relative -ne "Items.csv" -and
            $relative -ne "items.csv" -and
            $relative -ne "scripts/check-banned-words.sh" -and
            ($textFileExtensions -contains $extension -or $textFileNames -contains $fileName)
    } |
    Select-String -Pattern "\btool\b"

if ($matches) {
    $matches | ForEach-Object { "{0}:{1}:{2}" -f $_.Path, $_.LineNumber, $_.Line.Trim() }
    Write-Error "Banned word check failed: standalone banned term found outside allowed legacy files."
    exit 1
}

Write-Output "Banned word check passed."
exit 0
POWERSHELL
    exit $?
  fi

  echo "Banned word check failed: neither rg nor PowerShell (powershell.exe or pwsh) is available on PATH." >&2
  exit 127
fi

if rg --ignore-case --line-number \
  --glob '!Items.csv' \
  --glob '!items.csv' \
  --glob '!scripts/check-banned-words.sh' \
  --glob '!.git/**' \
  --glob '!**/bin/**' \
  --glob '!**/obj/**' \
  --glob '!**/publish/**' \
  '\btool\b' .; then
  echo "Banned word check failed: standalone banned term found outside allowed legacy files." >&2
  exit 1
fi

echo "Banned word check passed."
