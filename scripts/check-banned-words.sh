#!/usr/bin/env bash
set -euo pipefail
# This script checks for the banned word 'tool' outside Items.csv
# Returns non-zero exit code if such occurrences are found.
REPO_ROOT="$(git rev-parse --show-toplevel)"
cd "$REPO_ROOT"
# Run ripgrep and capture matches excluding Items.csv
MATCHES=$(rg -n -i '\btool' --glob '!Items.csv' || true)
if [[ -n "$MATCHES" ]]; then
  echo "$MATCHES"
  echo "Banned word 'tool' found outside Items.csv" >&2
  exit 1
fi
