# Organizational hierarchy seeds

These seeds provision system-wide roles and a demo ACME organization structure used for local development and QA of the SDAutoOS organizational hierarchy module.

## Permission model

Permissions are stored as **namespaced strings** inside the `permissions` JSON column on `RoleDefinition`. Examples:

- `system.platform.manage`
- `branch.manage:*`
- `department.assign:*`
- `jobs.execute`

This keeps the JSON payload structured (a list of scoped capabilities) while remaining compatible with the existing access-control resolver that flattens permission arrays.

## Seeded system roles

The `prisma/seeds/seed-system-roles.ts` script upserts fourteen system roles under the `system` tenant with inheritance chains:

| Key | Scope | Inherits | Purpose (permission highlights) |
| --- | --- | --- | --- |
| SYSTEM_ROOT | SYSTEM | SYSTEM_OPERATIONS | Full platform control (`system.platform.manage`, `system.tenants.manage`, audit + impersonation). |
| SYSTEM_OPERATIONS | SYSTEM | SYSTEM_AUDITOR | Tenant support with audit visibility and impersonation. |
| SYSTEM_AUDITOR | SYSTEM | — | Read-only platform audit access. |
| TENANT_OWNER | TENANT | TENANT_ADMIN, TENANT_FINANCE | Owns tenant lifecycle, billing, and security. |
| TENANT_ADMIN | TENANT | TENANT_AUDITOR | Configures branches, departments, catalog rules, and staff assignment. |
| TENANT_FINANCE | TENANT | TENANT_AUDITOR | Billing and ledger integration management. |
| TENANT_AUDITOR | TENANT | — | Read-only tenant/branch/department visibility. |
| BRANCH_GENERAL_MANAGER | BRANCH | BRANCH_WORKSHOP_MANAGER, BRANCH_PARTS_MANAGER, BRANCH_LOGISTICS_CONTROLLER, DEPARTMENT_MANAGER | End-to-end branch leadership (`branch.manage:*`, compliance review, department assignment). |
| BRANCH_WORKSHOP_MANAGER | BRANCH | DEPARTMENT_MANAGER | Workshop scheduling/approvals (`jobs.manage`). |
| BRANCH_PARTS_MANAGER | BRANCH | DEPARTMENT_SUPERVISOR | Dismantling + catalog quality + procurement approvals. |
| BRANCH_LOGISTICS_CONTROLLER | BRANCH | DEPARTMENT_SUPERVISOR | Freight, warehouse, and stock reconciliation. |
| DEPARTMENT_MANAGER | DEPARTMENT | DEPARTMENT_SUPERVISOR | Department performance, staffing, and approvals. |
| DEPARTMENT_SUPERVISOR | DEPARTMENT | DEPARTMENT_SPECIALIST | Dispatches and validates day-to-day work. |
| DEPARTMENT_SPECIALIST | DEPARTMENT | — | Executes frontline tasks and status updates. |

> All permissions remain arrays of namespaced strings so they are consumable by the existing `AccessControlService` without further transformation.

## Seeded ACME organization

`prisma/seeds/seed-demo-org-structure.ts` provisions the demo tenant `acme-tenant` using the system roles above:

- **Branches**
  - `ACME-MEL` (ACME Melbourne)
  - `ACME-SYD` (ACME Sydney)
- **Departments per branch**
  - Operations Leadership (`*-OPS`): BRANCH_GENERAL_MANAGER + department role set (default: DEPARTMENT_SPECIALIST).
  - Workshop (`*-WKS`): BRANCH_WORKSHOP_MANAGER + department role set (default: DEPARTMENT_SPECIALIST).
  - Dismantling & Parts (`*-DISM`): BRANCH_PARTS_MANAGER + department role set (default: DEPARTMENT_SPECIALIST).
  - Logistics & Warehouse (`*-LOG`): BRANCH_LOGISTICS_CONTROLLER + department role set (default: DEPARTMENT_SPECIALIST).
- **Demo users and assignments**
  - `user.acme.owner` → MEL-OPS as BRANCH_GENERAL_MANAGER (primary) + DEPARTMENT_MANAGER.
  - `user.mel.workshop` → MEL-WKS as BRANCH_WORKSHOP_MANAGER (primary) + DEPARTMENT_SUPERVISOR.
  - `user.mel.parts` → MEL-DISM as BRANCH_PARTS_MANAGER (primary) + DEPARTMENT_SUPERVISOR.
  - `user.mel.logistics` → MEL-LOG as BRANCH_LOGISTICS_CONTROLLER (primary).
  - `user.syd.manager` → SYD-OPS as BRANCH_GENERAL_MANAGER (primary).

Assignments are created with `removedAt = null` to indicate active status. Primary roles set `isPrimary = true` for the user’s anchor department.

## Running the seeds

From `apps/server`:

```bash
# Seed system roles then ACME demo data
npm run seed:org
```

> The script uses `ts-node --project tsconfig.json` to pick up the Prisma schema and TypeScript settings in this workspace.

## Extending or modifying the seeds

- **Adding permissions**: keep new permissions namespaced (e.g., `inventory.audit.read`) and append them to the relevant role’s permission array so downstream access-control resolution remains stable.
- **Adjusting inheritance**: update parent keys in `roleSeeds` and rerun the seed; inheritance is re-applied via upsert.
- **Changing structure**: edit the branch/department arrays in `seed-demo-org-structure.ts`. Codes must remain unique per tenant because of Prisma constraints.
- **Users and assignments**: add user IDs plus their `roleKey` in the `assignments` array for a department. The seed uses upserts, so rerunning is safe for iterative refinement.
