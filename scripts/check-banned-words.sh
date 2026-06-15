#!/usr/bin/env bash
set -euo pipefail

# Keep the legacy standalone lowercase banned term out of source/docs while
# allowing the intentionally named Items.csv data file.
if rg --ignore-case --line-number --glob '!Items.csv' --glob '!.git/**' '\btool\b' .; then
  echo "Banned word check failed: standalone banned term found outside Items.csv." >&2
  exit 1
fi

echo "Banned word check passed."
