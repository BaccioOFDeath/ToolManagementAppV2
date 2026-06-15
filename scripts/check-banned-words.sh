#!/usr/bin/env bash
set -euo pipefail

# Keep the legacy standalone lowercase banned term out of source files while
# allowing the intentionally named Items.csv data file and historical docs.
if rg --ignore-case --line-number \
  --glob '!Items.csv' \
  --glob '!SAMPLE_EXPORTS.md' \
  --glob '!designprompt.md' \
  --glob '!tool-app-master-prompt.md' \
  --glob '!SERVER_DEPLOYMENT_GUIDE.md' \
  --glob '!EXPANSION_SUMMARY.md' \
  --glob '!README.md' \
  --glob '!scripts/check-banned-words.sh' \
  --glob '!.git/**' \
  '\btool\b' .; then
  echo "Banned word check failed: standalone banned term found outside allowed legacy files." >&2
  exit 1
fi

echo "Banned word check passed."
