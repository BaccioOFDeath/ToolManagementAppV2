# Async Shell Branding Startup

Date: 2026-07-03

## Summary

Improved app startup responsiveness by removing synchronous settings waits from the main shell ViewModel constructor. The shell can now show its default title immediately while company logo and application-name branding load asynchronously.

## Completed Work

- Removed blocking `GetAwaiter().GetResult()` settings reads for `CompanyLogoPath` during `MainViewModel` construction.
- Removed blocking `GetAwaiter().GetResult()` settings reads for `ApplicationName` during `MainViewModel` construction.
- Added an asynchronous shell-branding loader that starts after the core page commands and shared ViewModels are initialized.
- Started logo-path and application-name reads together before awaiting both results, reducing sequential startup I/O.
- Kept the existing default `{item label} Management` title visible until a non-blank application name is available.
- Preserved non-blank guards so empty branding settings do not erase visible shell branding.
- Logged shell-branding read failures as warnings instead of failing the main shell constructor or dashboard startup.
- Preserved live Settings page branding updates for company logo and application name.
- Added source-contract coverage that prevents constructor-blocking settings reads from being reintroduced.
- Added source-contract coverage for concurrent branding reads, default title fallback, non-blank application, warning logging, and live settings update preservation.

## Validation

- Connector source readback and compare can verify this branch is limited to `MainViewModel.cs`, startup performance source-contract coverage, and this progress note.
- Direct local validation could not run in this scheduled Linux environment because direct checkout is blocked and `dotnet`, `pwsh`, `gh`, and the WPF runtime are unavailable.

## Follow-Up

- Run `pwsh -File scripts/run-full-validation.ps1` from a Windows/.NET-capable checkout.
- Smoke test first app launch with blank branding settings, populated branding settings, and an unavailable or slow settings store to confirm the default title appears quickly and branding updates shortly after load.
