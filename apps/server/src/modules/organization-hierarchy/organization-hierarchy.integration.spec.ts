import { randomUUID } from 'crypto';
import assert from 'node:assert/strict';
import type {
  Branch,
  Department,
  DepartmentRole,
  PrismaClient,
  RoleDefinition,
  UserDepartmentAssignment,
} from '@prisma/client';
import { AccessControlService } from './services/access-control.service.ts';
import { DepartmentService } from './services/department.service.ts';
import { RoleDefinitionService } from './services/role-definition.service.ts';
import { UserAssignmentService } from './services/user-assignment.service.ts';
import { ConflictError, NotFoundError, ValidationError } from '../../common/exceptions.ts';

type TestFn = () => Promise<void> | void;
const tests: { name: string; fn: TestFn }[] = [];
const beforeEachHooks: TestFn[] = [];

function describe(_name: string, fn: TestFn) {
  fn();
}

function it(name: string, fn: TestFn) {
  tests.push({ name, fn });
}

function beforeEach(fn: TestFn) {
  beforeEachHooks.push(fn);
}

class FakeRedis {
  private readonly store = new Map<string, string>();
  public readonly setOperations: string[] = [];
  public readonly deletedKeys: string[] = [];

  async get(key: string) {
    return this.store.get(key) ?? null;
  }

  async set(key: string, value: string) {
    this.setOperations.push(key);
    this.store.set(key, value);
  }

  async del(key: string) {
    this.deletedKeys.push(key);
    this.store.delete(key);
  }
}

class InMemoryPrisma implements Partial<PrismaClient> {
  branches: Branch[] = [];
  departments: Department[] = [];
  departmentRoles: DepartmentRole[] = [];
  roleDefinitions: RoleDefinition[] = [];
  assignments: UserDepartmentAssignment[] = [];
  roleDefinitionLookups = 0;

  reset() {
    this.branches = [];
    this.departments = [];
    this.departmentRoles = [];
    this.roleDefinitions = [];
    this.assignments = [];
    this.roleDefinitionLookups = 0;
  }

  branch = {
    create: async ({ data }: { data: Omit<Branch, 'id' | 'createdAt' | 'updatedAt'> }) => {
      const exists = this.branches.find((branch) => branch.tenantId === data.tenantId && branch.code === data.code);
      if (exists) {
        throw new ConflictError('Branch code already exists for tenant.');
      }

      const branch: Branch = {
        ...data,
        id: randomUUID(),
        metadata: data.metadata ?? {},
        createdAt: new Date(),
        updatedAt: new Date(),
      };
      this.branches.push(branch);
      return branch;
    },
    findUnique: async ({ where: { id } }: { where: { id: string } }) =>
      this.branches.find((branch) => branch.id === id) ?? null,
    findFirst: async ({ where }: { where: Partial<Branch> }) =>
      this.branches.find((branch) => {
        return (!where.tenantId || branch.tenantId === where.tenantId) && (!where.code || branch.code === where.code);
      }) ?? null,
  };

  private includeDepartment(
    department: Department | undefined,
    include?: { branch?: boolean; children?: boolean; assignments?: boolean },
  ) {
    if (!department) return null;
    return {
      ...department,
      ...(include?.branch ? { branch: this.branches.find((branch) => branch.id === department.branchId) ?? null } : {}),
      ...(include?.children
        ? { children: this.departments.filter((child) => child.parentDepartmentId === department.id) }
        : {}),
      ...(include?.assignments
        ? { assignments: this.assignments.filter((assignment) => assignment.departmentId === department.id) }
        : {}),
    };
  }

  department = {
    create: async ({ data }: { data: Omit<Department, 'id' | 'createdAt' | 'updatedAt'> }) => {
      const exists = this.departments.find(
        (department) => department.tenantId === data.tenantId && department.code === data.code,
      );
      if (exists) {
        throw new ConflictError('Department code already exists for tenant.');
      }

      const department: Department = {
        ...data,
        id: randomUUID(),
        metadata: data.metadata ?? {},
        createdAt: new Date(),
        updatedAt: new Date(),
      };
      this.departments.push(department);
      return department;
    },
    findUnique: async ({ where: { id }, include }: { where: { id: string }; include?: Record<string, boolean> }) =>
      this.includeDepartment(this.departments.find((department) => department.id === id), include as any),
    findFirst: async ({ where }: { where: Partial<Department> }) =>
      this.departments.find((department) => {
        return (
          (!where.tenantId || department.tenantId === where.tenantId) &&
          (!where.code || department.code === where.code)
        );
      }) ?? null,
    findMany: async ({ where, include, orderBy }: { where: any; include?: any; orderBy?: any }) => {
      const filtered = this.departments.filter((department) => {
        return (
          (!where.tenantId || department.tenantId === where.tenantId) &&
          (!where.branchId || department.branchId === where.branchId) &&
          (!where.parentDepartmentId || department.parentDepartmentId === where.parentDepartmentId) &&
          (!where.metadata?.path || department.metadata?.[where.metadata.path[0]] === where.metadata.equals)
        );
      });

      const sorted = orderBy?.createdAt === 'desc'
        ? [...filtered].sort((a, b) => b.createdAt.getTime() - a.createdAt.getTime())
        : filtered;

      return sorted.map((department) => this.includeDepartment(department, include) as Department);
    },
    update: async ({ where: { id }, data }: { where: { id: string }; data: Partial<Department> }) => {
      const index = this.departments.findIndex((department) => department.id === id);
      if (index < 0) return null;

      const updated: Department = {
        ...this.departments[index],
        ...data,
        metadata: data.metadata ?? this.departments[index].metadata,
        updatedAt: new Date(),
      } as Department;
      this.departments[index] = updated;
      return updated;
    },
    delete: async ({ where: { id } }: { where: { id: string } }) => {
      const index = this.departments.findIndex((department) => department.id === id);
      if (index < 0) return null;
      const [removed] = this.departments.splice(index, 1);
      this.departmentRoles = this.departmentRoles.filter((role) => role.departmentId !== id);
      this.assignments = this.assignments.filter((assignment) => assignment.departmentId !== id);
      return removed;
    },
  };

  roleDefinition = {
    create: async ({ data }: { data: Omit<RoleDefinition, 'id' | 'createdAt' | 'updatedAt'> }) => {
      const role: RoleDefinition = {
        ...data,
        id: randomUUID(),
        inheritsFromIds: data.inheritsFromIds ?? [],
        permissions: data.permissions ?? [],
        createdAt: new Date(),
        updatedAt: new Date(),
      };
      this.roleDefinitions.push(role);
      return role;
    },
    findUnique: async ({ where: { id } }: { where: { id: string } }) => {
      this.roleDefinitionLookups += 1;
      return this.roleDefinitions.find((role) => role.id === id) ?? null;
    },
    findFirst: async ({ where }: { where: any }) =>
      this.roleDefinitions.find((role) => role.tenantId === where.tenantId && role.key === where.key) ?? null,
    findMany: async ({ where, orderBy }: { where: any; orderBy?: any }) => {
      const filtered = this.roleDefinitions.filter((role) => {
        if (where.OR) {
          return where.OR.some((clause: any) => {
            if (clause.isSystem === true) return role.isSystem === true;
            return role.tenantId === clause.tenantId;
          });
        }
        if (where.id?.in) {
          return where.id.in.includes(role.id);
        }
        return true;
      });

      const sorted = orderBy?.displayName === 'asc'
        ? [...filtered].sort((a, b) => a.displayName.localeCompare(b.displayName))
        : filtered;

      return sorted;
    },
    update: async ({ where: { id }, data }: { where: { id: string }; data: Partial<RoleDefinition> }) => {
      const index = this.roleDefinitions.findIndex((role) => role.id === id);
      if (index < 0) return null;
      const updated: RoleDefinition = {
        ...this.roleDefinitions[index],
        ...data,
        inheritsFromIds: data.inheritsFromIds ?? this.roleDefinitions[index].inheritsFromIds,
        permissions: (data.permissions ?? this.roleDefinitions[index].permissions) as any,
        updatedAt: new Date(),
      } as RoleDefinition;
      this.roleDefinitions[index] = updated;
      return updated;
    },
  };

  departmentRole = {
    create: async ({ data }: { data: Omit<DepartmentRole, 'id' | 'createdAt' | 'updatedAt'> }) => {
      const exists = this.departmentRoles.find(
        (role) => role.departmentId === data.departmentId && role.roleDefinitionId === data.roleDefinitionId,
      );
      if (exists) {
        throw new ConflictError('Role already exists for department.');
      }

      const departmentRole: DepartmentRole = {
        ...data,
        id: randomUUID(),
        createdAt: new Date(),
        updatedAt: new Date(),
      };
      this.departmentRoles.push(departmentRole);
      return departmentRole;
    },
    deleteMany: async ({ where: { departmentId } }: { where: { departmentId: string } }) => {
      const before = this.departmentRoles.length;
      this.departmentRoles = this.departmentRoles.filter((role) => role.departmentId !== departmentId);
      return { count: before - this.departmentRoles.length } as any;
    },
    findUnique: async ({ where: { id }, include }: { where: { id: string }; include?: any }) => {
      const departmentRole = this.departmentRoles.find((role) => role.id === id);
      if (!departmentRole) return null;

      return {
        ...departmentRole,
        ...(include?.department ? { department: this.departments.find((dept) => dept.id === departmentRole.departmentId) } : {}),
        ...(include?.definition ? { definition: this.roleDefinitions.find((role) => role.id === departmentRole.roleDefinitionId) } : {}),
      } as any;
    },
  };

  userDepartmentAssignment = {
    create: async ({ data }: { data: Omit<UserDepartmentAssignment, 'id' | 'assignedAt'> }) => {
      const assignment: UserDepartmentAssignment = {
        ...data,
        id: randomUUID(),
        assignedAt: new Date(),
        removedAt: data.removedAt ?? null,
      };
      this.assignments.push(assignment);
      return assignment;
    },
    findFirst: async ({ where }: { where: any }) =>
      this.assignments.find((assignment) => {
        return Object.entries(where).every(([key, value]) => {
          if (value === undefined) return true;
          if (key === 'NOT') {
            return Object.entries(value).every(([notKey, notValue]) => (assignment as any)[notKey] !== notValue);
          }
          if (value && typeof value === 'object' && 'department' in value) {
            return assignment.departmentId === (value as any).department.id;
          }
          return (assignment as any)[key] === value;
        });
      }) ?? null,
    findUnique: async ({ where: { id }, include }: { where: { id: string }; include?: any }) => {
      const assignment = this.assignments.find((record) => record.id === id);
      if (!assignment) return null;
      const departmentRole = this.departmentRoles.find((role) => role.id === assignment.departmentRoleId)!;
      return {
        ...assignment,
        ...(include?.departmentRole
          ? {
              departmentRole: {
                ...departmentRole,
                ...(include.departmentRole.include?.definition
                  ? { definition: this.roleDefinitions.find((role) => role.id === departmentRole.roleDefinitionId) }
                  : {}),
                ...(include.departmentRole.include?.department
                  ? { department: this.departments.find((dept) => dept.id === assignment.departmentId)! }
                  : {}),
              },
            }
          : {}),
      } as any;
    },
    findMany: async ({ where, include, orderBy }: { where: any; include?: any; orderBy?: any }) => {
      const filtered = this.assignments.filter((assignment) => {
        const removedAtMatches = where.removedAt !== undefined ? assignment.removedAt === where.removedAt : true;
        return (
          (!where.tenantId || assignment.tenantId === where.tenantId) &&
          (!where.userId || assignment.userId === where.userId) &&
          (!where.departmentId || assignment.departmentId === where.departmentId) &&
          removedAtMatches &&
          (!where.department?.branchId ||
            this.departments.find((dept) => dept.id === assignment.departmentId)?.branchId === where.department.branchId)
        );
      });

      const sorted = orderBy?.assignedAt === 'desc'
        ? [...filtered].sort((a, b) => b.assignedAt.getTime() - a.assignedAt.getTime())
        : filtered;

      return sorted.map((assignment) => ({
        ...assignment,
        ...(include?.department ? { department: this.departments.find((dept) => dept.id === assignment.departmentId)! } : {}),
        ...(include?.departmentRole
          ? {
              departmentRole: {
                ...this.departmentRoles.find((role) => role.id === assignment.departmentRoleId)!,
                ...(include.departmentRole.include?.definition
                  ? {
                      definition: this.roleDefinitions.find(
                        (role) => role.id === this.departmentRoles.find((role) => role.id === assignment.departmentRoleId)!
                          .roleDefinitionId,
                      ),
                    }
                  : {}),
                ...(include.departmentRole.include?.department
                  ? { department: this.departments.find((dept) => dept.id === assignment.departmentId)! }
                  : {}),
              },
            }
          : {}),
      }));
    },
    update: async ({ where: { id }, data }: { where: { id: string }; data: Partial<UserDepartmentAssignment> }) => {
      const index = this.assignments.findIndex((assignment) => assignment.id === id);
      if (index < 0) return null;
      const updated: UserDepartmentAssignment = {
        ...this.assignments[index],
        ...data,
      } as UserDepartmentAssignment;
      this.assignments[index] = updated;
      return updated;
    },
  };

  $transaction = async <T>(fn: (tx: PrismaClient) => Promise<T>) => {
    return fn(this as unknown as PrismaClient);
  };
}

const createTenantEntities = async (prisma: InMemoryPrisma, tenantId: string) => {
  const branch = await prisma.branch.create({ data: { tenantId, code: 'BR-01', name: 'Main', metadata: {} } });
  const fallbackBranch = await prisma.branch.create({ data: { tenantId, code: 'BR-02', name: 'Secondary', metadata: {} } });

  const parentDepartment = await prisma.department.create({
    data: {
      tenantId,
      branchId: branch.id,
      code: 'OPS',
      name: 'Operations',
      parentDepartmentId: null,
      metadata: { type: 'operations' },
    },
  });

  const serviceDepartment = await prisma.department.create({
    data: {
      tenantId,
      branchId: branch.id,
      code: 'SERV',
      name: 'Service',
      parentDepartmentId: parentDepartment.id,
      metadata: { type: 'service' },
    },
  });

  const partsDepartment = await prisma.department.create({
    data: {
      tenantId,
      branchId: fallbackBranch.id,
      code: 'PARTS',
      name: 'Parts',
      parentDepartmentId: parentDepartment.id,
      metadata: { type: 'parts' },
    },
  });

  return { branch, fallbackBranch, parentDepartment, serviceDepartment, partsDepartment };
};

const seedRoles = async (prisma: InMemoryPrisma, tenantId: string, branchId: string) => {
  const roleService = new RoleDefinitionService(prisma as unknown as PrismaClient);

  const serviceManager = await roleService.createRoleDefinition(tenantId, {
    key: 'service_manager',
    displayName: 'Service Manager',
    scope: 'branch',
    permissions: ['jobs.manage', 'department.assign'],
  });

  const branchManager = await roleService.createRoleDefinition(tenantId, {
    key: 'branch_manager',
    displayName: 'Branch Manager',
    scope: 'branch',
    permissions: [`branch.manage:${branchId}`],
    inheritsFromIds: [serviceManager.id],
  });

  const tenantAdmin = await roleService.createRoleDefinition(tenantId, {
    key: 'tenant_admin',
    displayName: 'Tenant Admin',
    scope: 'tenant',
    permissions: ['tenant.manage'],
    inheritsFromIds: [branchManager.id],
  });

  return { serviceManager, branchManager, tenantAdmin };
};

describe('organization hierarchy integration', () => {
  let prisma: InMemoryPrisma;
  let redis: FakeRedis;
  let accessControl: AccessControlService;
  let departmentService: DepartmentService;
  let assignmentService: UserAssignmentService;

  beforeEach(() => {
    prisma = new InMemoryPrisma();
    redis = new FakeRedis();
    accessControl = new AccessControlService(prisma as unknown as PrismaClient, redis as any);
    departmentService = new DepartmentService(prisma as unknown as PrismaClient);
    assignmentService = new UserAssignmentService(prisma as unknown as PrismaClient, accessControl);
    prisma.reset();
  });

  it('creates branches and departments while enforcing tenant scoping', async () => {
    const tenantId = 'tenant-a';
    const branch = await prisma.branch.create({ data: { tenantId, code: 'BR-A', name: 'HQ', metadata: {} } });
    const otherTenantBranch = await prisma.branch.create({
      data: { tenantId: 'tenant-b', code: 'BR-B', name: 'Other', metadata: {} },
    });

    const department = await departmentService.createDepartment(tenantId, {
      branchId: branch.id,
      code: 'ADM',
      name: 'Admin',
      metadata: { region: 'north' },
      parentDepartmentId: null,
      type: 'administration',
    });

    assert.equal(department.metadata?.['region'], 'north');
    assert.equal(department.metadata?.['type'], 'administration');

    await assert.rejects(
      () =>
        departmentService.createDepartment(tenantId, {
          branchId: otherTenantBranch.id,
          code: 'ILLEGAL',
          name: 'CrossTenant',
          parentDepartmentId: null,
        }),
      ValidationError,
    );
  });

  it('resolves inherited permissions and caches results without N+1 lookups', async () => {
    const tenantId = 'tenant-a';
    const { branch, serviceDepartment, partsDepartment } = await createTenantEntities(prisma, tenantId);
    const { serviceManager, branchManager, tenantAdmin } = await seedRoles(prisma, tenantId, branch.id);

    const branchManagerForService = await prisma.departmentRole.create({
      data: {
        tenantId,
        departmentId: serviceDepartment.id,
        roleDefinitionId: branchManager.id,
        isDefault: true,
      },
    });

    const branchManagerForParts = await prisma.departmentRole.create({
      data: {
        tenantId,
        departmentId: partsDepartment.id,
        roleDefinitionId: branchManager.id,
        isDefault: false,
      },
    });

    const tenantAdminRole = await prisma.departmentRole.create({
      data: {
        tenantId,
        departmentId: serviceDepartment.id,
        roleDefinitionId: tenantAdmin.id,
        isDefault: false,
      },
    });

    const branchUser = 'user-branch';
    await assignmentService.assignUser({
      tenantId,
      userId: branchUser,
      departmentId: serviceDepartment.id,
      departmentRoleId: branchManagerForService.id,
      isPrimary: true,
    });
    await assignmentService.assignUser({
      tenantId,
      userId: branchUser,
      departmentId: partsDepartment.id,
      departmentRoleId: branchManagerForParts.id,
      isPrimary: false,
    });

    const permissions = await accessControl.getEffectivePermissions(tenantId, branchUser);
    assert.ok(permissions.includes(`branch.manage:${branch.id}`));
    assert.ok(permissions.includes('jobs.manage'));
    assert.ok(permissions.includes('department.assign'));

    assert.ok(prisma.roleDefinitionLookups <= 2, 'roles resolved only once per unique definition');
    assert.ok(redis.setOperations.length >= 1, 'permissions cached to redis');

      const scopedByBranch = await accessControl.getScopedDepartments(tenantId, branchUser, branch.id);
      assert.equal(scopedByBranch.length, 1);
      assert.ok(scopedByBranch.every((dept) => dept.branchId === branch.id));

      const allScoped = await accessControl.getScopedDepartments(tenantId, branchUser);
      assert.equal(allScoped.length, 2);

    const tenantAdminUser = 'tenant-admin-user';
    await assignmentService.assignUser({
      tenantId,
      userId: tenantAdminUser,
      departmentId: serviceDepartment.id,
      departmentRoleId: tenantAdminRole.id,
      isPrimary: true,
    });

    const adminPermissions = await accessControl.getEffectivePermissions(tenantId, tenantAdminUser);
    assert.ok(adminPermissions.includes('tenant.manage'));
    assert.ok(adminPermissions.includes(`branch.manage:${branch.id}`));
    assert.ok(adminPermissions.includes('jobs.manage'));
    await assert.rejects(() => accessControl.getEffectivePermissions('tenant-b', tenantAdminUser), NotFoundError);
    assert.ok(redis.setOperations.length >= 2, 'separate cache entry stored per user');
  });

  it('handles transfers, inheritance, and termination with correct access control outcomes', async () => {
    const tenantId = 'tenant-a';
    const { branch, serviceDepartment, partsDepartment } = await createTenantEntities(prisma, tenantId);
    const { serviceManager, branchManager } = await seedRoles(prisma, tenantId, branch.id);

    const branchRole = await prisma.departmentRole.create({
      data: {
        tenantId,
        departmentId: serviceDepartment.id,
        roleDefinitionId: branchManager.id,
        isDefault: true,
      },
    });

    const serviceRole = await prisma.departmentRole.create({
      data: {
        tenantId,
        departmentId: partsDepartment.id,
        roleDefinitionId: serviceManager.id,
        isDefault: false,
      },
    });

    const userId = 'movable-user';
    const assignment = await assignmentService.assignUser({
      tenantId,
      userId,
      departmentId: serviceDepartment.id,
      departmentRoleId: branchRole.id,
      isPrimary: true,
    });

    await accessControl.ensurePermission(tenantId, userId, `branch.manage:${branch.id}`);

    const transferred = await assignmentService.transfer({
      tenantId,
      userId,
      fromDepartmentId: serviceDepartment.id,
      fromDepartmentRoleId: branchRole.id,
      toDepartmentId: partsDepartment.id,
      toDepartmentRoleId: serviceRole.id,
    });

    const original = prisma.assignments.find((entry) => entry.id === assignment.id)!;
    assert.ok(original.removedAt, 'original assignment marked removed');
    assert.equal(transferred.departmentId, partsDepartment.id);
    assert.equal(transferred.isPrimary, true);
    assert.ok(redis.deletedKeys.some((key) => key.includes(userId)), 'cache invalidated on transfer');

    const newPermissions = await accessControl.getEffectivePermissions(tenantId, userId);
    assert.ok(newPermissions.includes('jobs.manage'));
    assert.ok(!newPermissions.includes(`branch.manage:${branch.id}`), 'branch scope removed after transfer');

    await assignmentService.terminateAssignment({ tenantId, assignmentId: transferred.id });
    await assert.rejects(() => accessControl.getEffectivePermissions(tenantId, userId), NotFoundError);
    assert.ok(redis.deletedKeys.filter((key) => key.includes(userId)).length >= 2, 'cache invalidated on termination');
  });
});

(async () => {
  let failures = 0;

  for (const test of tests) {
    try {
      for (const hook of beforeEachHooks) {
        await hook();
      }
      await test.fn();
      console.log(`\u2713 ${test.name}`);
    } catch (error) {
      failures += 1;
      console.error(`\u2717 ${test.name}`);
      console.error(error);
    }
  }

  if (failures > 0) {
    process.exitCode = 1;
  }
})();
