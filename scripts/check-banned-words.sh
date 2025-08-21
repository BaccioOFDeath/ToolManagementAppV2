#!/usr/bin/env bash
set -euo pipefail
# This script checks for the banned words 'tool' or 'tools' outside Items.csv
# Returns non-zero exit code if such occurrences are found.
REPO_ROOT="$(git rev-parse --show-toplevel)"
cd "$REPO_ROOT"
# Run ripgrep and capture matches excluding Items.csv, this script, and known safe terms
MATCHES=$(rg -n -i '\btools?\b' \
  --glob '!Items.csv' \
  --glob '!scripts/check-banned-words.sh' \
  --glob '!**/ToolBar*' \
  --glob '!**/Toolkit*' || true)
if [[ -n "$MATCHES" ]]; then
  echo "$MATCHES"
  echo "Banned word 'tool' or 'tools' found outside Items.csv" >&2
  exit 1
fi
