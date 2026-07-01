# Current Work Queue Refresh

Date: 2026-07-02

## Completed

- Refreshed `ToDo.md` so the release/validation queue reflects the current repository state instead of continuing to center old 2026-06-24 cleanup wording.
- Recorded the latest completed reliability direction, including validation diagnostics, bounded report/export reads, setup guard hardening, generated report polish, and customer export paging.
- Called out draft PR #1458 as still requiring real Windows/.NET validation before it should be marked ready or merged.
- Reprioritized the immediate queue around full validation, dependency audit review, WPF visual smoke testing, behavior-focused test cleanup, and the next concrete item import normalization risk.

## Why This Mattered

The scheduled environment still cannot clone or run the WPF/.NET validation stack, so stale durable notes can easily steer future hourly runs toward old validation wording or already-completed report/export cleanup. Keeping the repo work queue current is a release-readiness improvement: it makes the next safe engineering step clearer and reduces repeated work.

## Validation

- GitHub connector readback should confirm `ToDo.md` now references the current default branch, current validation blocker, latest completed customer export paging work, open draft PR #1458, and the refreshed next-work priorities.
- Local validation still needs a Windows/.NET-capable checkout because this scheduled environment cannot clone the repository and does not provide `dotnet`, `pwsh`, `gh`, or WPF runtime support.
