import { PrismaClient, Branch, Department, DepartmentRole, RoleDefinition } from '../apps/server/node_modules/@prisma/client';

const prisma = new PrismaClient();
const DEFAULT_DEPARTMENT_NAME = 'General Operations';
const FALLBACK_ROLE_KEY = 'standard_staff';

const LEGACY_ROLE_MAP: Record<string, string> = {
  owner: 'TENANT_OWNER',
  admin: 'TENANT_ADMIN',
  manager: 'BRANCH_MANAGER',
  supervisor: 'DEPARTMENT_SUPERVISOR',
  specialist: 'DEPARTMENT_SPECIALIST',
  technician: 'DEPARTMENT_SPECIALIST',
  mechanic: 'DEPARTMENT_SPECIALIST',
  foreman: 'DEPARTMENT_SUPERVISOR',
  sales: 'BRANCH_PARTS_MANAGER',
  warehouse: 'BRANCH_WAREHOUSE_CONTROLLER',
  finance: 'TENANT_FINANCE',
  accountant: 'TENANT_FINANCE',
  support: 'SYSTEM_SUPPORT',
};

type LegacyUser = {
  id: string;
  tenantId: string;
  branchId: string | null;
  legacyRole?: string | null;
  createdAt: Date;
};

type ReportEntry = {
  branchId: string;
  branchName: string;
  branchCode?: string;
  departmentId: string;
  departmentName: string;
  count: number;
};

const roleCache = new Map<string, Map<string, RoleDefinition>>();
const departmentCache = new Map<string, Department>();
const departmentRoleCache = new Map<string, DepartmentRole>();

function normalizeRoleKey(legacyRole?: string | null): string {
  if (!legacyRole) return FALLBACK_ROLE_KEY;
  const normalized = legacyRole.trim().toLowerCase();
  return LEGACY_ROLE_MAP[normalized] ?? FALLBACK_ROLE_KEY;
}

function deriveDepartmentCode(branch: Branch) {
  const suffix = 'GENOPS';
  if (branch.code) {
    return `${branch.code}-${suffix}`.toUpperCase();
  }
  return `${branch.id.slice(0, 8)}-${suffix}`.toUpperCase();
}

async function fetchRoleDefinition(tenantId: string, key: string) {
  const tenantCache = roleCache.get(tenantId) ?? new Map<string, RoleDefinition>();
  if (tenantCache.has(key)) {
    return tenantCache.get(key) ?? null;
  }

  const role = await prisma.roleDefinition.findUnique({
    where: { tenantId_key: { tenantId, key } },
  });

  if (role) {
    tenantCache.set(key, role);
    roleCache.set(tenantId, tenantCache);
  }

  return role ?? null;
}

async function resolveRoleDefinition(tenantId: string, legacyRole?: string | null) {
  const desiredKey = normalizeRoleKey(legacyRole);
  const role = (await fetchRoleDefinition(tenantId, desiredKey)) ?? (await fetchRoleDefinition(tenantId, FALLBACK_ROLE_KEY));

  if (!role) {
    console.warn(`⚠️  No role definition found for keys [${desiredKey}, ${FALLBACK_ROLE_KEY}] in tenant ${tenantId}.`);
  }

  return role;
}

async function ensureDefaultDepartment(branch: Branch) {
  const cacheKey = `${branch.tenantId}:${branch.id}`;
  if (departmentCache.has(cacheKey)) {
    return departmentCache.get(cacheKey)!;
  }

  const code = deriveDepartmentCode(branch);
  const department = await prisma.department.upsert({
    where: { tenantId_code: { tenantId: branch.tenantId, code } },
    update: { name: DEFAULT_DEPARTMENT_NAME, branchId: branch.id },
    create: {
      tenantId: branch.tenantId,
      branchId: branch.id,
      code,
      name: DEFAULT_DEPARTMENT_NAME,
      metadata: {},
    },
  });

  departmentCache.set(cacheKey, department);
  return department;
}

async function ensureDepartmentRole(tenantId: string, departmentId: string, roleDefinitionId: string) {
  const cacheKey = `${departmentId}:${roleDefinitionId}`;
  if (departmentRoleCache.has(cacheKey)) {
    return departmentRoleCache.get(cacheKey)!;
  }

  const role = await prisma.departmentRole.upsert({
    where: { departmentId_roleDefinitionId: { departmentId, roleDefinitionId } },
    update: { isDefault: true },
    create: { tenantId, departmentId, roleDefinitionId, isDefault: true },
  });

  departmentRoleCache.set(cacheKey, role);
  return role;
}

async function findUsersNeedingMigration(): Promise<LegacyUser[]> {
  return prisma.$queryRaw<LegacyUser[]>`
    SELECT u.id, u.tenant_id AS "tenantId", u.branch_id AS "branchId", u.role AS "legacyRole", u.created_at AS "createdAt"
    FROM "User" u
    WHERE u.branch_id IS NOT NULL
      AND NOT EXISTS (
        SELECT 1 FROM "UserDepartmentAssignment" uda
        WHERE uda.user_id = u.id AND uda.tenant_id = u.tenant_id AND uda.removed_at IS NULL
      )
  `;
}

async function createAssignment(user: LegacyUser, department: Department, departmentRole: DepartmentRole, dryRun: boolean) {
  const existing = await prisma.userDepartmentAssignment.findFirst({
    where: {
      tenantId: user.tenantId,
      userId: user.id,
      departmentRoleId: departmentRole.id,
      removedAt: null,
    },
  });

  if (existing) {
    console.log(`ℹ️  User ${user.id} already has active assignment in department ${department.id}. Skipping.`);
    return false;
  }

  const assignedAt = user.createdAt ? new Date(user.createdAt) : new Date();

  if (dryRun) {
    console.log(
      `DRY-RUN: Would create primary assignment for user ${user.id} in department ${department.id} with role ${departmentRole.roleDefinitionId}.`
    );
    return true;
  }

  await prisma.userDepartmentAssignment.create({
    data: {
      tenantId: user.tenantId,
      userId: user.id,
      departmentRoleId: departmentRole.id,
      departmentId: department.id,
      isPrimary: true,
      assignedAt,
    },
  });

  console.log(
    `✅ Created primary assignment for user ${user.id} in department ${department.id} with role ${departmentRole.roleDefinitionId}.`
  );
  return true;
}

function logHeader(dryRun: boolean) {
  console.log('----------------------------------------------');
  console.log('Migrating users to General Operations departments');
  console.log(`Mode: ${dryRun ? 'DRY-RUN (no changes)' : 'EXECUTE'}`);
  console.log('----------------------------------------------');
}

async function main() {
  const dryRun = process.argv.includes('--dry-run') || process.argv.includes('-n');
  logHeader(dryRun);

  const users = await findUsersNeedingMigration();
  console.log(`Found ${users.length} user(s) with branch assignment but no department memberships.`);

  if (!users.length) {
    return;
  }

  const branchIds = Array.from(new Set(users.map((u) => u.branchId).filter((id): id is string => Boolean(id))));
  const branches = await prisma.branch.findMany({ where: { id: { in: branchIds } } });
  const branchMap = new Map(branches.map((b) => [b.id, b]));

  const report = new Map<string, ReportEntry>();

  for (const user of users) {
    if (!user.branchId) {
      console.warn(`⚠️  User ${user.id} has no branchId; skipping.`);
      continue;
    }

    const branch = branchMap.get(user.branchId);
    if (!branch) {
      console.warn(`⚠️  Branch ${user.branchId} not found for user ${user.id}; skipping.`);
      continue;
    }

    console.log(`\nProcessing user ${user.id} (tenant ${user.tenantId}) in branch ${branch.code ?? branch.id}`);

    const roleDefinition = await resolveRoleDefinition(user.tenantId, user.legacyRole);
    if (!roleDefinition) {
      console.warn(`⚠️  No role definition resolved for user ${user.id}; skipping assignment.`);
      continue;
    }

    const department = await ensureDefaultDepartment(branch);
    const departmentRole = await ensureDepartmentRole(user.tenantId, department.id, roleDefinition.id);

    const created = await createAssignment(user, department, departmentRole, dryRun);
    if (created) {
      const key = `${branch.id}:${department.id}`;
      const existing = report.get(key);
      if (existing) {
        existing.count += 1;
      } else {
        report.set(key, {
          branchId: branch.id,
          branchName: branch.name,
          branchCode: branch.code,
          departmentId: department.id,
          departmentName: department.name,
          count: 1,
        });
      }
    }
  }

  if (!report.size) {
    console.log('\nNo assignments were created.');
    return;
  }

  console.log('\nMigration summary by branch and department:');
  const rows = Array.from(report.values()).map((entry) => ({
    Branch: `${entry.branchCode ?? entry.branchId} (${entry.branchName})`,
    Department: `${entry.departmentName} [${entry.departmentId}]`,
    Assignments: entry.count,
  }));
  console.table(rows);
}

main()
  .catch((error) => {
    console.error('Migration failed:', error);
    process.exitCode = 1;
  })
  .finally(async () => {
    await prisma.$disconnect();
  });
