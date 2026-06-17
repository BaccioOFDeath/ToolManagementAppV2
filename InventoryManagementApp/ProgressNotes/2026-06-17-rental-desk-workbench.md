# Rental Desk Workbench Pass - 2026-06-17

## Completed

- Reworked `ManageRentalsPage` from three stacked grids into a rental desk workstation with a main rental directory, selected-rental advisor handoff panel, and open request queue.
- Kept the existing end-to-end commands available where the advisor or technician needs them: details, check in, extend, place request, history, picking slip, invoice, rental print, checked-out print, request print, and delete.
- Added a selected-rental panel that shows customer contact, timing, shelf location, document actions, and a desk checklist so checkout and check-in are visible from one place.
- Preserved row double-click and row-correct context-menu behavior while making the context menus match the visible workflow actions.
- Rebalanced the page with splitter-based desktop panels, wrapping toolbar actions, and compact footer actions so the page stays usable on narrower workstations.

## Why it matters

Advisors renting a item out and technicians checking it back in should not have to jump between grids or scan a long toolbar to find the next step. The rental page now matches the newer Maintenance, Calibration, Customers, and Reservations workbench style: select the operational record, review the handoff, act, then review the request queue before the item goes back to the shelf.

## Validation

- Created the branch from current `master` and compared it against `master`; the branch is ahead and not behind.
- Read back the changed XAML through the GitHub connector and checked the main rental desk, advisor handoff, request queue, context menus, and footer sections.
- Local `dotnet` build/test and WPF screenshot execution were not run because this scheduled Linux container does not have the .NET SDK or Windows/WPF runtime, and direct local clone/raw fetches remain blocked by the network tunnel.
- Did not run unrelated tests, per instruction.
