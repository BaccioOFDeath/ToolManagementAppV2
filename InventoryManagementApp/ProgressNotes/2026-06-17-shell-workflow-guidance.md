# Shell Workflow Guidance - 2026-06-17

## Completed

- Added a shell-level workflow guidance strip that updates with the current page and explains the next operational step in technician, advisor, data, insight, or admin language.
- Added permission-aware quick jumps from each page to related workbenches, such as Search to Rentals, Customers to Reservations, Settings to Users, and technician pages to each other.
- Reworked the main header into a wrapping layout with a minimum window size and trimmed user/title text so search, identity, and session controls hold together on narrower workstations.
- Enhanced the QA screenshot gallery output with a visual and workflow review checklist for layout, clipping, handoff, drilling, and role-completion checks.

## Why it matters

The individual pages have been upgraded into focused workbenches, but users still need to move between them without guessing where the next step lives. This pass gives the shell a consistent operator handoff layer so a technician, advisor, or admin can see the page purpose and jump to the most likely follow-up without backing out or hunting through every section.

## Validation

- Read and changed `MainWindow.xaml`, `MainViewModel.cs`, and `scripts/run-app-qa-screenshots.ps1` through the GitHub connector.
- Local `dotnet` build/test and WPF screenshot execution were not run because this scheduled Linux container does not have the .NET SDK or Windows/WPF runtime, and direct local clone/raw fetches remain blocked by the network tunnel.
- Did not run unrelated tests, per instruction.
