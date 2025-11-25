import { PrismaClient } from '@prisma/client';
import { RoleDefinitionMap } from './seed-system-roles';

const TENANT_ID = 'acme-tenant';

type AssignmentSeed = {
  userId: string;
  roleKey: string;
  isPrimary?: boolean;
};

type DepartmentSeed = {
  code: string;
  name: string;
  roleKeys: string[];
  defaultRoleKey?: string;
  assignments?: AssignmentSeed[];
};

type BranchSeed = {
  code: string;
  name: string;
  departments: DepartmentSeed[];
};

const branchSeeds: BranchSeed[] = [
  {
    code: 'ACME-MEL',
    name: 'ACME Melbourne',
    departments: [
      {
        code: 'MEL-OPS',
        name: 'Operations Leadership',
        roleKeys: ['BRANCH_GENERAL_MANAGER', 'DEPARTMENT_MANAGER', 'DEPARTMENT_SUPERVISOR', 'DEPARTMENT_SPECIALIST'],
        defaultRoleKey: 'DEPARTMENT_SPECIALIST',
        assignments: [
          { userId: 'user.acme.owner', roleKey: 'BRANCH_GENERAL_MANAGER', isPrimary: true },
          { userId: 'user.acme.owner', roleKey: 'DEPARTMENT_MANAGER' },
        ],
      },
      {
        code: 'MEL-WKS',
        name: 'Workshop',
        roleKeys: ['BRANCH_WORKSHOP_MANAGER', 'DEPARTMENT_MANAGER', 'DEPARTMENT_SUPERVISOR', 'DEPARTMENT_SPECIALIST'],
        defaultRoleKey: 'DEPARTMENT_SPECIALIST',
        assignments: [
          { userId: 'user.mel.workshop', roleKey: 'BRANCH_WORKSHOP_MANAGER', isPrimary: true },
          { userId: 'user.mel.workshop', roleKey: 'DEPARTMENT_SUPERVISOR' },
        ],
      },
      {
        code: 'MEL-DISM',
        name: 'Dismantling & Parts',
        roleKeys: ['BRANCH_PARTS_MANAGER', 'DEPARTMENT_MANAGER', 'DEPARTMENT_SUPERVISOR', 'DEPARTMENT_SPECIALIST'],
        defaultRoleKey: 'DEPARTMENT_SPECIALIST',
        assignments: [
          { userId: 'user.mel.parts', roleKey: 'BRANCH_PARTS_MANAGER', isPrimary: true },
          { userId: 'user.mel.parts', roleKey: 'DEPARTMENT_SUPERVISOR' },
        ],
      },
      {
        code: 'MEL-LOG',
        name: 'Logistics & Warehouse',
        roleKeys: ['BRANCH_LOGISTICS_CONTROLLER', 'DEPARTMENT_MANAGER', 'DEPARTMENT_SUPERVISOR', 'DEPARTMENT_SPECIALIST'],
        defaultRoleKey: 'DEPARTMENT_SPECIALIST',
        assignments: [
          { userId: 'user.mel.logistics', roleKey: 'BRANCH_LOGISTICS_CONTROLLER', isPrimary: true },
        ],
      },
    ],
  },
  {
    code: 'ACME-SYD',
    name: 'ACME Sydney',
    departments: [
      {
        code: 'SYD-OPS',
        name: 'Operations Leadership',
        roleKeys: ['BRANCH_GENERAL_MANAGER', 'DEPARTMENT_MANAGER', 'DEPARTMENT_SUPERVISOR', 'DEPARTMENT_SPECIALIST'],
        defaultRoleKey: 'DEPARTMENT_SPECIALIST',
        assignments: [
          { userId: 'user.syd.manager', roleKey: 'BRANCH_GENERAL_MANAGER', isPrimary: true },
        ],
      },
      {
        code: 'SYD-WKS',
        name: 'Workshop',
        roleKeys: ['BRANCH_WORKSHOP_MANAGER', 'DEPARTMENT_MANAGER', 'DEPARTMENT_SUPERVISOR', 'DEPARTMENT_SPECIALIST'],
        defaultRoleKey: 'DEPARTMENT_SPECIALIST',
      },
      {
        code: 'SYD-DISM',
        name: 'Dismantling & Parts',
        roleKeys: ['BRANCH_PARTS_MANAGER', 'DEPARTMENT_MANAGER', 'DEPARTMENT_SUPERVISOR', 'DEPARTMENT_SPECIALIST'],
        defaultRoleKey: 'DEPARTMENT_SPECIALIST',
      },
      {
        code: 'SYD-LOG',
        name: 'Logistics & Warehouse',
        roleKeys: ['BRANCH_LOGISTICS_CONTROLLER', 'DEPARTMENT_MANAGER', 'DEPARTMENT_SUPERVISOR', 'DEPARTMENT_SPECIALIST'],
        defaultRoleKey: 'DEPARTMENT_SPECIALIST',
      },
    ],
  },
];

async function upsertBranch(prisma: PrismaClient, branch: BranchSeed) {
  return prisma.branch.upsert({
    where: { tenantId_code: { tenantId: TENANT_ID, code: branch.code } },
    update: { name: branch.name, metadata: {} },
    create: { tenantId: TENANT_ID, code: branch.code, name: branch.name, metadata: {} },
  });
}

async function upsertDepartment(prisma: PrismaClient, branchId: string, department: DepartmentSeed) {
  return prisma.department.upsert({
    where: { tenantId_code: { tenantId: TENANT_ID, code: department.code } },
    update: { name: department.name, branchId },
    create: {
      tenantId: TENANT_ID,
      branchId,
      code: department.code,
      name: department.name,
      metadata: {},
    },
  });
}

async function upsertDepartmentRoles(
  prisma: PrismaClient,
  departmentId: string,
  department: DepartmentSeed,
  roleMap: RoleDefinitionMap,
) {
  const departmentRoleIds = new Map<string, string>();

  for (const roleKey of department.roleKeys) {
    const roleDefinitionId = roleMap.get(roleKey);
    if (!roleDefinitionId) {
      console.warn(`Role definition for key ${roleKey} not found; skipping department role linkage.`);
      continue;
    }

    const departmentRole = await prisma.departmentRole.upsert({
      where: {
        departmentId_roleDefinitionId: {
          departmentId,
          roleDefinitionId,
        },
      },
      update: {
        isDefault: department.defaultRoleKey === roleKey,
      },
      create: {
        tenantId: TENANT_ID,
        departmentId,
        roleDefinitionId,
        isDefault: department.defaultRoleKey === roleKey,
      },
    });

    departmentRoleIds.set(roleKey, departmentRole.id);
  }

  return departmentRoleIds;
}

async function seedAssignments(
  prisma: PrismaClient,
  departmentId: string,
  departmentRoles: Map<string, string>,
  assignments?: AssignmentSeed[],
) {
  if (!assignments?.length) return;

  for (const assignment of assignments) {
    const departmentRoleId = departmentRoles.get(assignment.roleKey);
    if (!departmentRoleId) {
      console.warn(`Department role for ${assignment.roleKey} missing; skipping assignment.`);
      continue;
    }

    await prisma.userDepartmentAssignment.upsert({
      where: {
        tenantId_userId_departmentRoleId: {
          tenantId: TENANT_ID,
          userId: assignment.userId,
          departmentRoleId,
        },
      },
      update: {
        departmentId,
        isPrimary: Boolean(assignment.isPrimary),
        removedAt: null,
      },
      create: {
        tenantId: TENANT_ID,
        userId: assignment.userId,
        departmentRoleId,
        departmentId,
        isPrimary: Boolean(assignment.isPrimary),
        assignedAt: new Date(),
        removedAt: null,
      },
    });
  }
}

export async function seedDemoOrgStructure(prisma: PrismaClient, roleMap: RoleDefinitionMap) {
  console.log('Seeding ACME demo tenant structure...');

  for (const branchSeed of branchSeeds) {
    const branch = await upsertBranch(prisma, branchSeed);

    for (const department of branchSeed.departments) {
      const departmentRecord = await upsertDepartment(prisma, branch.id, department);
      const departmentRoleIds = await upsertDepartmentRoles(prisma, departmentRecord.id, department, roleMap);
      await seedAssignments(prisma, departmentRecord.id, departmentRoleIds, department.assignments);
    }
  }

  console.log('Demo organization hierarchy seeded for tenant:', TENANT_ID);
}
