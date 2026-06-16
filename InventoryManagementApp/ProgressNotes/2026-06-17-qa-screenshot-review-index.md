# QA Screenshot Review Index - 2026-06-17

## Completed

- Enhanced `scripts/run-app-qa-screenshots.ps1` so every successful QA screenshot run writes a browser-friendly `index.html` gallery beside the existing README.
- Grouped captures by app area in the review index so the login, overview, operations, insights, data, admin, and dialog surfaces can be scanned quickly during UI review.
- Added screenshot drift validation: unexpected PNG files now fail the run unless the expected manifest is updated intentionally.
- Kept existing missing-file, folder, byte-size, and pixel-dimension checks so blank, cropped, missing, or renamed captures fail loudly.

## Validation

- Reviewed the script logic through the GitHub connector after editing.
- Did not run the WPF screenshot workflow because this scheduled Linux container does not have the .NET SDK or Windows/WPF runtime, and direct local cloning remains blocked by the network tunnel.
- Did not run unrelated tests, per instruction.
