# Banned Word Scan Contract Coverage

`BannedWordsScriptContractTests.cs` guards the source-scan scope used by `scripts/check-banned-words.sh`.

The tests are intentionally source-contract tests because the scheduled Linux environment does not provide the Windows/.NET validation stack needed to run the repository test suite here.
