# Feature Architecture Overview

## New Feature Stack

```
┌─────────────────────────────────────────────────────────────────────┐
│                          USER INTERFACE (UI Layer)                   │
│                           [Ready for XAML]                           │
└─────────────────────────────────────────────────────────────────────┘
                                    ▲
                                    │
                                    │ Data Binding
                                    │
┌─────────────────────────────────────────────────────────────────────┐
│                       VIEWMODELS (Presentation)                      │
│  ┌──────────────────┐  ┌──────────────────┐  ┌──────────────────┐  │
│  │  Maintenance     │  │  Calibration     │  │  Reservation     │  │
│  │  Management      │  │  Management      │  │  Management      │  │
│  │  ViewModel       │  │  ViewModel       │  │  ViewModel       │  │
│  │  (270 LOC)       │  │  (234 LOC)       │  │  (340 LOC)       │  │
│  └──────────────────┘  └──────────────────┘  └──────────────────┘  │
│  ┌──────────────────┐  ┌──────────────────┐  ┌──────────────────┐  │
│  │  Kit             │  │  Dashboard       │  │  Reports         │  │
│  │  Management      │  │  ViewModel       │  │  ViewModel       │  │
│  │  ViewModel       │  │  (Enhanced)      │  │  (Extended)      │  │
│  │  (390 LOC)       │  │                  │  │                  │  │
│  └──────────────────┘  └──────────────────┘  └──────────────────┘  │
└─────────────────────────────────────────────────────────────────────┘
                                    ▲
                                    │
                                    │ Business Logic
                                    │
┌─────────────────────────────────────────────────────────────────────┐
│                        SERVICES (Business Logic)                     │
│  ┌──────────────────┐  ┌──────────────────┐  ┌──────────────────┐  │
│  │  Maintenance     │  │  Calibration     │  │  Reservation     │  │
│  │  Service         │  │  Service         │  │  Service         │  │
│  │  (252 LOC)       │  │  (253 LOC)       │  │  (321 LOC)       │  │
│  │                  │  │                  │  │                  │  │
│  │  • Create        │  │  • Create        │  │  • Create        │  │
│  │  • Read          │  │  • Read          │  │  • Read          │  │
│  │  • Update        │  │  • Update        │  │  • Update        │  │
│  │  • Delete        │  │  • Delete        │  │  • Delete        │  │
│  │  • Complete      │  │  • Get Latest    │  │  • Confirm       │  │
│  │  • Get Overdue   │  │  • Get Overdue   │  │  • Cancel        │  │
│  │  • Get Upcoming  │  │  • Get Upcoming  │  │  • Fulfill       │  │
│  │                  │  │                  │  │  • Check Avail.  │  │
│  └──────────────────┘  └──────────────────┘  └──────────────────┘  │
│  ┌──────────────────┐  ┌──────────────────┐                         │
│  │  Kit             │  │  Report          │                         │
│  │  Service         │  │  Service         │                         │
│  │  (283 LOC)       │  │  (Enhanced)      │                         │
│  │                  │  │                  │                         │
│  │  • Create Kit    │  │  • 7 New Reports │                         │
│  │  • Manage Items  │  │  • Enhanced      │                         │
│  │  • Check Avail.  │  │    Summary       │                         │
│  └──────────────────┘  └──────────────────┘                         │
└─────────────────────────────────────────────────────────────────────┘
                                    ▲
                                    │
                                    │ Data Access
                                    │
┌─────────────────────────────────────────────────────────────────────┐
│                      MODELS (Domain Layer)                           │
│  ┌──────────────────┐  ┌──────────────────┐  ┌──────────────────┐  │
│  │  Maintenance     │  │  Calibration     │  │  Reservation     │  │
│  │  Record          │  │  Record          │  │                  │  │
│  │  (112 LOC)       │  │  (120 LOC)       │  │  (127 LOC)       │  │
│  └──────────────────┘  └──────────────────┘  └──────────────────┘  │
│  ┌──────────────────┐                                               │
│  │  Kit / KitItem   │                                               │
│  │  (123 LOC)       │                                               │
│  └──────────────────┘                                               │
└─────────────────────────────────────────────────────────────────────┘
                                    ▲
                                    │
                                    │ Persistence
                                    │
┌─────────────────────────────────────────────────────────────────────┐
│                    DATABASE (SQLite via DatabaseService)             │
│  ┌──────────────────────────────────────────────────────────────┐  │
│  │ MaintenanceRecords                                            │  │
│  │ - MaintenanceID (PK), ItemID (FK), ScheduledDate, Status...  │  │
│  │ - Indexes: ItemID, ScheduledDate, Status                     │  │
│  └──────────────────────────────────────────────────────────────┘  │
│  ┌──────────────────────────────────────────────────────────────┐  │
│  │ CalibrationRecords                                            │  │
│  │ - CalibrationID (PK), ItemID (FK), CalibrationDate, Next...  │  │
│  │ - Indexes: ItemID, NextCalibrationDue                        │  │
│  └──────────────────────────────────────────────────────────────┘  │
│  ┌──────────────────────────────────────────────────────────────┐  │
│  │ Reservations                                                  │  │
│  │ - ReservationID (PK), ItemID (FK), CustomerID (FK), Dates... │  │
│  │ - Indexes: ItemID, CustomerID, StartDate/EndDate, Status    │  │
│  └──────────────────────────────────────────────────────────────┘  │
│  ┌──────────────────────────────────────────────────────────────┐  │
│  │ Kits & KitItems                                               │  │
│  │ - KitID (PK), KitNumber, Name, Category...                   │  │
│  │ - KitItemID (PK), KitID (FK), ItemID (FK), Quantity...      │  │
│  │ - Indexes: KitNumber (unique), KitID, ItemID                │  │
│  └──────────────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────────────┘
```

## Feature Integration Points

### 1. Dashboard Integration
```
DashboardViewModel
    ├── Displays "Overdue Maintenance" count
    ├── Displays "Overdue Calibrations" count  
    ├── Displays "Active Reservations" count
    └── Displays "Active Kits" count
```

### 2. Reporting Integration
```
ReportService
    ├── GenerateMaintenanceReport(overdueOnly)
    ├── GenerateCalibrationReport(overdueOnly)
    ├── GenerateReservationReport(activeOnly)
    ├── GenerateKitReport()
    └── GenerateSummaryReport() [Enhanced with new metrics]
```

### 3. Navigation Flow (Ready for UI)
```
MainWindow
    └── MainViewModel
        ├── Navigate to Maintenance Management
        ├── Navigate to Calibration Management
        ├── Navigate to Reservation Management
        └── Navigate to Kit Management
```

## Cross-Feature Relationships

```
Items ──┬── can have ──→ MaintenanceRecords
        ├── can have ──→ CalibrationRecords
        ├── can have ──→ Reservations
        └── can be in ──→ Kits

Customers ──── can make ──→ Reservations

Users ──┬── create ──→ MaintenanceRecords
        ├── create ──→ CalibrationRecords
        ├── create ──→ Reservations
        └── create ──→ Kits

Reservations ──── can link to ──→ Rentals (when fulfilled)

Kits ──── contain ──→ KitItems ──→ Items
```

## SDAutoOS Backend Integration (apps/server)

- **Migrations & Seeds:** Prisma migrations live in `apps/server/prisma`; run `npm run prisma:status` to review pending changes, `npm run prisma:migrate-dev` to apply locally, and `npm run prisma:seed-org-roles` to preload demo tenants/branches/departments plus hierarchical role definitions. 【F:apps/server/prisma/check-migrations.ts†L8-L34】【F:apps/server/package.json†L6-L12】【F:apps/server/prisma/seed-org-roles.ts†L233-L308】
- **Tenant & Branch Scoping:** The organization context helper resolves `tenantId`, `userId`, and `branchId` from request headers or legacy `user.branchId`, ensuring guard checks and service lookups always target the correct tenant/branch. Department creation and updates enforce tenant ownership for branches and parent departments. 【F:apps/server/src/modules/organization-hierarchy/organization-context.ts†L5-L33】【F:apps/server/src/modules/organization-hierarchy/services/department.service.ts†L10-L67】
- **Authorization & Caching:** `OrganizationAccessGuard` relies on `AccessControlService` to fetch permissions derived from department assignments, caching them in Redis for five minutes and invalidating on assignment changes. Missing assignments raise `NotFoundError` to prevent default allow behavior. 【F:apps/server/src/modules/organization-hierarchy/organization-access.guard.ts†L35-L69】【F:apps/server/src/modules/organization-hierarchy/services/access-control.service.ts†L9-L64】
- **GraphQL & REST Surface:** Department data is available via GraphQL queries/mutations and mirrored REST endpoints, all protected by the organization guard and `manageDepartmentPermission`. GraphQL connections support cursor-based pagination; REST endpoints accept optional `branchId` filters to keep responses branch-aware. 【F:apps/server/src/modules/organization-hierarchy/department.resolver.ts†L18-L90】【F:apps/server/src/modules/organization-hierarchy/controllers/department.controller.ts†L15-L80】

## Testing Coverage

```
Unit Tests (28 test cases)
    ├── MaintenanceServiceTests (7 tests)
    │   ├── Create, Read, Update, Delete
    │   ├── Get Overdue
    │   └── Complete Maintenance
    ├── CalibrationServiceTests (6 tests)
    │   ├── Create, Read, Update, Delete
    │   └── Get Overdue
    ├── ReservationServiceTests (7 tests)
    │   ├── Create, Read, Update, Delete
    │   ├── Confirm, Cancel, Fulfill
    │   └── Check Availability
    └── KitServiceTests (8 tests)
        ├── Create, Read, Update, Delete Kit
        ├── Add, Update, Remove KitItem
        └── Check Kit Availability
```

## Implementation Statistics

| Component              | Files | Lines of Code | Description                    |
|------------------------|-------|---------------|--------------------------------|
| Domain Models          | 4     | ~480          | Data structures                |
| Services               | 4     | ~1,100        | Business logic                 |
| ViewModels             | 4     | ~1,230        | Presentation logic             |
| Database Schema        | -     | ~100          | Table definitions + indexes    |
| Report Extensions      | 2     | ~180          | New reports + enhanced metrics |
| Unit Tests             | 4     | ~460          | Test coverage                  |
| Documentation          | 2     | ~230          | Summary + architecture docs    |
| **TOTAL**              | **20**| **~3,780**    | **Production-ready code**      |

## Benefits of This Architecture

1. **Separation of Concerns**: Each layer has clear responsibilities
2. **Testability**: Services can be tested independently with mocked dependencies
3. **Maintainability**: Changes to one layer don't ripple to others
4. **Extensibility**: New features can follow the same pattern
5. **MVVM Compliance**: ViewModels don't know about UI, UI binds to ViewModels
6. **Reusability**: Services can be used by multiple ViewModels
7. **Performance**: Database indexes optimize common queries
8. **Integration**: New features integrate seamlessly with existing infrastructure

## Ready for UI Integration

To complete the UI layer, create XAML files for:
- MaintenancePage.xaml → MaintenanceManagementViewModel
- CalibrationPage.xaml → CalibrationManagementViewModel
- ReservationPage.xaml → ReservationManagementViewModel
- KitManagementPage.xaml → KitManagementViewModel

Plus edit dialogs for each feature area.

All business logic, data access, and presentation logic is complete and tested!
