import { PrismaClient, Prisma } from '@prisma/client';

const prisma = new PrismaClient();
const DEMO_TENANT_ID = 'demo-tenant';

type RoleSeed = {
  key: string;
  displayName: string;
  description: string;
  scope: 'SYSTEM' | 'TENANT' | 'BRANCH' | 'DEPARTMENT';
  permissions: Prisma.JsonObject;
  inheritsFrom?: string[];
  isSystem?: boolean;
};

const roleSeeds: RoleSeed[] = [
  {
    key: 'SYSTEM_ROOT',
    displayName: 'System Root',
    description: 'Full platform control including tenant provisioning and schema changes.',
    scope: 'SYSTEM',
    isSystem: true,
    permissions: { managePlatform: true, manageTenants: true, assumeIdentity: true, viewAudit: true },
  },
  {
    key: 'SYSTEM_SUPPORT',
    displayName: 'System Support',
    description: 'Operational support with audit visibility.',
    scope: 'SYSTEM',
    isSystem: true,
    permissions: { managePlatform: false, manageTenants: true, viewAudit: true, impersonateTenant: true },
    inheritsFrom: ['SYSTEM_AUDITOR'],
  },
  {
    key: 'SYSTEM_AUDITOR',
    displayName: 'System Auditor',
    description: 'Read-only access to tenant and platform audit data.',
    scope: 'SYSTEM',
    isSystem: true,
    permissions: { viewAudit: true },
  },
  {
    key: 'TENANT_OWNER',
    displayName: 'Tenant Owner',
    description: 'Owns tenant configuration, billing, and branch enablement.',
    scope: 'TENANT',
    permissions: {
      manageTenant: true,
      manageBilling: true,
      manageBranches: true,
      manageUsers: true,
      manageCatalog: true,
      viewAudit: true,
    },
    inheritsFrom: ['TENANT_ADMIN', 'TENANT_FINANCE'],
  },
  {
    key: 'TENANT_ADMIN',
    displayName: 'Tenant Admin',
    description: 'Configures branches, departments, and staff membership.',
    scope: 'TENANT',
    permissions: { manageBranches: true, manageDepartments: true, manageUsers: true, manageCatalog: true },
    inheritsFrom: ['TENANT_AUDITOR'],
  },
  {
    key: 'TENANT_FINANCE',
    displayName: 'Tenant Finance',
    description: 'Handles tenant-level billing and ledger integrations.',
    scope: 'TENANT',
    permissions: { manageBilling: true, manageLedgerIntegrations: true, viewAudit: true },
    inheritsFrom: ['TENANT_AUDITOR'],
  },
  {
    key: 'TENANT_AUDITOR',
    displayName: 'Tenant Auditor',
    description: 'Read-only visibility into tenant configuration and audit trails.',
    scope: 'TENANT',
    permissions: { viewAudit: true },
  },
  {
    key: 'BRANCH_MANAGER',
    displayName: 'Branch Manager',
    description: 'Owns branch operations across workshop, dismantling, and parts.',
    scope: 'BRANCH',
    permissions: {
      manageBranchSettings: true,
      manageStaffing: true,
      approveQuotes: true,
      manageInventory: true,
      manageWorkOrders: true,
      manageCompliance: true,
    },
    inheritsFrom: ['BRANCH_SERVICE_LEAD', 'BRANCH_PARTS_MANAGER', 'BRANCH_WAREHOUSE_CONTROLLER', 'DEPARTMENT_MANAGER'],
  },
  {
    key: 'BRANCH_SERVICE_LEAD',
    displayName: 'Branch Service Lead',
    description: 'Runs workshop scheduling and technician dispatch.',
    scope: 'BRANCH',
    permissions: { manageWorkOrders: true, manageTechnicians: true, approveWork: true },
    inheritsFrom: ['DEPARTMENT_MANAGER'],
  },
  {
    key: 'BRANCH_PARTS_MANAGER',
    displayName: 'Branch Parts Manager',
    description: 'Oversees dismantling priorities and parts catalog quality.',
    scope: 'BRANCH',
    permissions: { manageInventory: true, manageCatalog: true, approveProcurement: true },
    inheritsFrom: ['DEPARTMENT_SUPERVISOR'],
  },
  {
    key: 'BRANCH_WAREHOUSE_CONTROLLER',
    displayName: 'Branch Warehouse Controller',
    description: 'Controls inbound/outbound logistics and freight.',
    scope: 'BRANCH',
    permissions: { manageWarehouse: true, manageFreight: true, reconcileStock: true },
    inheritsFrom: ['DEPARTMENT_SUPERVISOR'],
  },
  {
    key: 'DEPARTMENT_MANAGER',
    displayName: 'Department Manager',
    description: 'Owns department performance and staffing.',
    scope: 'DEPARTMENT',
    permissions: { manageDepartment: true, scheduleStaff: true, approveExceptions: true },
    inheritsFrom: ['DEPARTMENT_SUPERVISOR'],
  },
  {
    key: 'DEPARTMENT_SUPERVISOR',
    displayName: 'Department Supervisor',
    description: 'Supervises day-to-day work and validates outputs.',
    scope: 'DEPARTMENT',
    permissions: { reviewTasks: true, assignTasks: true, closeJobs: true },
    inheritsFrom: ['DEPARTMENT_SPECIALIST'],
  },
  {
    key: 'DEPARTMENT_SPECIALIST',
    displayName: 'Department Specialist',
    description: 'Executes tasks within the department workflow.',
    scope: 'DEPARTMENT',
    permissions: { executeTasks: true, logTime: true, updateStatus: true },
  },
];

const demoBranches = [
  {
    code: 'MEL',
    name: 'Melbourne Branch',
    departments: [
      { code: 'MEL-DISM', name: 'Dismantling' },
      { code: 'MEL-WKS', name: 'Workshop' },
      { code: 'MEL-PRT', name: 'Parts & Sales' },
    ],
  },
  {
    code: 'SYD',
    name: 'Sydney Branch',
    departments: [
      { code: 'SYD-DISM', name: 'Dismantling' },
      { code: 'SYD-WKS', name: 'Workshop' },
      { code: 'SYD-PRT', name: 'Parts & Sales' },
    ],
  },
];

async function seedRoleDefinitions() {
  const createdDefinitions = [] as Awaited<ReturnType<typeof prisma.roleDefinition.upsert>>[];

  for (const role of roleSeeds) {
    const record = await prisma.roleDefinition.upsert({
      where: {
        tenantId_key: {
          tenantId: DEMO_TENANT_ID,
          key: role.key,
        },
      },
      update: {
        displayName: role.displayName,
        description: role.description,
        scope: role.scope,
        permissions: role.permissions,
        isSystem: role.isSystem ?? false,
      },
      create: {
        tenantId: DEMO_TENANT_ID,
        key: role.key,
        displayName: role.displayName,
        description: role.description,
        scope: role.scope,
        permissions: role.permissions,
        inheritsFromIds: [],
        isSystem: role.isSystem ?? false,
      },
    });

    createdDefinitions.push(record);
  }

  const definitionByKey = new Map(createdDefinitions.map((role) => [role.key, role.id]));

  for (const role of roleSeeds) {
    const inheritIds = (role.inheritsFrom ?? [])
      .map((key) => definitionByKey.get(key))
      .filter((val): val is string => Boolean(val));

    await prisma.roleDefinition.update({
      where: {
        tenantId_key: {
          tenantId: DEMO_TENANT_ID,
          key: role.key,
        },
      },
      data: { inheritsFromIds: inheritIds },
    });
  }

  return definitionByKey;
}

async function seedBranchesAndDepartments(roleMap: Map<string, string>) {
  for (const branch of demoBranches) {
    const branchRecord = await prisma.branch.upsert({
      where: {
        tenantId_code: {
          tenantId: DEMO_TENANT_ID,
          code: branch.code,
        },
      },
      update: {
        name: branch.name,
        metadata: {},
      },
      create: {
        tenantId: DEMO_TENANT_ID,
        code: branch.code,
        name: branch.name,
        metadata: {},
      },
    });

    for (const dept of branch.departments) {
      const department = await prisma.department.upsert({
        where: {
          tenantId_code: {
            tenantId: DEMO_TENANT_ID,
            code: dept.code,
          },
        },
        update: {
          name: dept.name,
          branchId: branchRecord.id,
        },
        create: {
          tenantId: DEMO_TENANT_ID,
          branchId: branchRecord.id,
          code: dept.code,
          name: dept.name,
          metadata: {},
        },
      });

      const departmentRoleKeys = ['DEPARTMENT_MANAGER', 'DEPARTMENT_SUPERVISOR', 'DEPARTMENT_SPECIALIST'];

      for (const roleKey of departmentRoleKeys) {
        const roleDefinitionId = roleMap.get(roleKey);
        if (!roleDefinitionId) {
          continue;
        }

        await prisma.departmentRole.upsert({
          where: {
            departmentId_roleDefinitionId: {
              departmentId: department.id,
              roleDefinitionId,
            },
          },
          update: {
            isDefault: roleKey === 'DEPARTMENT_SPECIALIST',
          },
          create: {
            tenantId: DEMO_TENANT_ID,
            departmentId: department.id,
            roleDefinitionId,
            isDefault: roleKey === 'DEPARTMENT_SPECIALIST',
          },
        });
      }
    }
  }
}

async function main() {
  console.log('Seeding organizational role definitions and demo structure...');
  const roleMap = await seedRoleDefinitions();
  await seedBranchesAndDepartments(roleMap);
  console.log('Seed complete. Migration remains unapplied by default; run prisma migrate dev when ready.');
}

main()
  .catch((error) => {
    console.error(error);
    process.exit(1);
  })
  .finally(async () => prisma.$disconnect());
