import { Prisma, PrismaClient } from '@prisma/client';

export type RoleDefinitionMap = Map<string, string>;

type RoleSeed = {
  key: string;
  displayName: string;
  description: string;
  scope: 'SYSTEM' | 'TENANT' | 'BRANCH' | 'DEPARTMENT';
  permissions: string[];
  inheritsFrom?: string[];
};

const SYSTEM_TENANT_ID = 'system';

const roleSeeds: RoleSeed[] = [
  {
    key: 'SYSTEM_ROOT',
    displayName: 'System Root',
    description: 'Full platform control including tenant provisioning, schema changes, and identity escalation.',
    scope: 'SYSTEM',
    permissions: [
      'system.platform.manage',
      'system.tenants.manage',
      'system.audit.read',
      'system.identity.assume',
      'roles.manage',
    ],
    inheritsFrom: ['SYSTEM_OPERATIONS'],
  },
  {
    key: 'SYSTEM_OPERATIONS',
    displayName: 'System Operations',
    description: 'Enables production support tasks with audit visibility and tenant impersonation.',
    scope: 'SYSTEM',
    permissions: ['system.tenants.support', 'system.audit.read', 'system.identity.assumeTenant'],
    inheritsFrom: ['SYSTEM_AUDITOR'],
  },
  {
    key: 'SYSTEM_AUDITOR',
    displayName: 'System Auditor',
    description: 'Read-only access to platform-level audit and configuration artefacts.',
    scope: 'SYSTEM',
    permissions: ['system.audit.read'],
  },
  {
    key: 'TENANT_OWNER',
    displayName: 'Tenant Owner',
    description: 'Owns tenant configuration, billing, security, and platform integrations.',
    scope: 'TENANT',
    permissions: [
      'tenant.lifecycle.manage',
      'tenant.billing.manage',
      'tenant.security.manage',
      'tenant.audit.read',
    ],
    inheritsFrom: ['TENANT_ADMIN', 'TENANT_FINANCE'],
  },
  {
    key: 'TENANT_ADMIN',
    displayName: 'Tenant Admin',
    description: 'Configures branches, departments, staff membership, and product catalogue rules.',
    scope: 'TENANT',
    permissions: ['tenant.configuration.manage', 'branch.manage:*', 'department.manage:*', 'department.assign:*', 'roles.manage'],
    inheritsFrom: ['TENANT_AUDITOR'],
  },
  {
    key: 'TENANT_FINANCE',
    displayName: 'Tenant Finance',
    description: 'Handles tenant-level billing, ledger integrations, and commercial controls.',
    scope: 'TENANT',
    permissions: ['tenant.billing.manage', 'tenant.ledger.manage', 'tenant.audit.read'],
    inheritsFrom: ['TENANT_AUDITOR'],
  },
  {
    key: 'TENANT_AUDITOR',
    displayName: 'Tenant Auditor',
    description: 'Read-only visibility into tenant configuration, branches, and compliance trails.',
    scope: 'TENANT',
    permissions: ['tenant.audit.read', 'branch.view:*', 'department.view:*'],
  },
  {
    key: 'BRANCH_GENERAL_MANAGER',
    displayName: 'Branch General Manager',
    description: 'Owns branch operations across dismantling, workshop, logistics, and compliance.',
    scope: 'BRANCH',
    permissions: ['branch.manage:*', 'branch.operations.configure', 'branch.compliance.review', 'department.assign:*'],
    inheritsFrom: ['BRANCH_WORKSHOP_MANAGER', 'BRANCH_PARTS_MANAGER', 'BRANCH_LOGISTICS_CONTROLLER', 'DEPARTMENT_MANAGER'],
  },
  {
    key: 'BRANCH_WORKSHOP_MANAGER',
    displayName: 'Branch Workshop Manager',
    description: 'Runs workshop scheduling, approvals, and technician dispatch.',
    scope: 'BRANCH',
    permissions: ['jobs.manage', 'branch.workshop.schedule', 'branch.workshop.approve'],
    inheritsFrom: ['DEPARTMENT_MANAGER'],
  },
  {
    key: 'BRANCH_PARTS_MANAGER',
    displayName: 'Branch Parts & Dismantling Manager',
    description: 'Oversees dismantling priorities, catalog quality, and procurement approvals.',
    scope: 'BRANCH',
    permissions: ['inventory.manage', 'inventory.procurement.approve', 'branch.catalog.manage'],
    inheritsFrom: ['DEPARTMENT_SUPERVISOR'],
  },
  {
    key: 'BRANCH_LOGISTICS_CONTROLLER',
    displayName: 'Branch Logistics Controller',
    description: 'Controls inbound/outbound logistics, freight, and stock reconciliation.',
    scope: 'BRANCH',
    permissions: ['branch.freight.manage', 'branch.warehouse.manage', 'inventory.reconcile'],
    inheritsFrom: ['DEPARTMENT_SUPERVISOR'],
  },
  {
    key: 'DEPARTMENT_MANAGER',
    displayName: 'Department Manager',
    description: 'Owns department performance, staffing, and compliance exceptions.',
    scope: 'DEPARTMENT',
    permissions: ['department.manage:*', 'department.assign:*', 'worklog.approvals'],
    inheritsFrom: ['DEPARTMENT_SUPERVISOR'],
  },
  {
    key: 'DEPARTMENT_SUPERVISOR',
    displayName: 'Department Supervisor',
    description: 'Supervises day-to-day work, dispatches jobs, and validates outputs.',
    scope: 'DEPARTMENT',
    permissions: ['department.assign:*', 'jobs.dispatch', 'jobs.validate'],
    inheritsFrom: ['DEPARTMENT_SPECIALIST'],
  },
  {
    key: 'DEPARTMENT_SPECIALIST',
    displayName: 'Department Specialist',
    description: 'Executes frontline tasks, captures time, and updates work statuses.',
    scope: 'DEPARTMENT',
    permissions: ['jobs.execute', 'status.update', 'time.capture'],
  },
];

export async function seedSystemRoles(prisma: PrismaClient): Promise<RoleDefinitionMap> {
  const createdDefinitions = await Promise.all(
    roleSeeds.map((role) =>
      prisma.roleDefinition.upsert({
        where: {
          tenantId_key: {
            tenantId: SYSTEM_TENANT_ID,
            key: role.key,
          },
        },
        update: {
          displayName: role.displayName,
          description: role.description,
          scope: role.scope,
          permissions: role.permissions as Prisma.JsonArray,
          isSystem: true,
        },
        create: {
          tenantId: SYSTEM_TENANT_ID,
          key: role.key,
          displayName: role.displayName,
          description: role.description,
          scope: role.scope,
          permissions: role.permissions as Prisma.JsonArray,
          inheritsFromIds: [],
          isSystem: true,
        },
      }),
    ),
  );

  const definitionByKey = new Map(createdDefinitions.map((role) => [role.key, role.id]));

  for (const role of roleSeeds) {
    const inheritIds = (role.inheritsFrom ?? [])
      .map((key) => definitionByKey.get(key))
      .filter((value): value is string => Boolean(value));

    await prisma.roleDefinition.update({
      where: {
        tenantId_key: {
          tenantId: SYSTEM_TENANT_ID,
          key: role.key,
        },
      },
      data: { inheritsFromIds: inheritIds },
    });
  }

  console.log(`Seeded ${roleSeeds.length} system roles with hierarchical permissions.`);
  return definitionByKey;
}
