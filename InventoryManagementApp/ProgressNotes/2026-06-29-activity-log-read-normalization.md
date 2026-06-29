# Activity Log Read Normalization

## Summary
- Trimmed Activity Log user names and actions when rows are mapped back from SQLite.
- Added source-contract coverage so legacy padded audit rows are normalized on read, matching the current write-side normalization.

## Why
Recent Activity Log hardening normalized new audit entries before persistence. Legacy rows or rows created before that change could still carry padded user names/actions into reports, filters, and history views, creating duplicate-looking audit values. Normalizing in `MapLog` keeps the visible audit trail consistent without changing the stored historical row text.

## Validation
- Connector readback was used because direct clone/raw access is blocked in this scheduled Linux environment.
- Local `dotnet` restore/build/test, PowerShell validation, WPF runtime checks, screenshots, and banned-word checks were unavailable here.
- This is not a UI layout change; it affects the Activity Log read model used by existing grids, reports, and history views across the supported screen sizes without changing control sizing.
