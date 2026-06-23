#!/usr/bin/env bash
set -euo pipefail

# Keep the legacy standalone lowercase banned term out of source files while
# allowing the intentionally seeded item CSV data.
if ! command -v rg >/dev/null 2>&1; then
  if command -v powershell.exe >/dev/null 2>&1; then
    powershell.exe -NoProfile -Command "\$matches = rg --ignore-case --line-number --glob '!Items.csv' --glob '!items.csv' --glob '!scripts/check-banned-words.sh' --glob '!.git/**' '\\btool\\b' .; if (\$LASTEXITCODE -eq 0) { \$matches; Write-Error 'Banned word check failed: standalone banned term found outside allowed legacy files.'; exit 1 } elseif (\$LASTEXITCODE -eq 1) { Write-Output 'Banned word check passed.'; exit 0 } else { Write-Error \"rg failed with exit code \$LASTEXITCODE\"; exit \$LASTEXITCODE }"
    exit $?
  fi

  echo "Banned word check failed: rg is not available on PATH." >&2
  exit 127
fi

if rg --ignore-case --line-number \
  --glob '!Items.csv' \
  --glob '!items.csv' \
  --glob '!scripts/check-banned-words.sh' \
  --glob '!.git/**' \
  '\btool\b' .; then
  echo "Banned word check failed: standalone banned term found outside allowed legacy files." >&2
  exit 1
fi

echo "Banned word check passed."
