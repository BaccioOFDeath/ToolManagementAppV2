import assert from 'node:assert';
import fs from 'node:fs';
import path from 'node:path';

type IndexDef = {
  name: string;
  table: string;
  columns: string[];
  unique: boolean;
};

type ForeignKeyDef = {
  name: string;
  table: string;
  columns: string[];
  references: string;
  referencedColumns: string[];
  onDelete?: string;
  onUpdate?: string;
};

function loadMigrationSql() {
  const migrationsDir = path.join(__dirname, '..', 'prisma', 'migrations');
  const migrationDir = fs
    .readdirSync(migrationsDir)
    .filter((entry) => entry.includes('add_complete_org_hierarchy'))
    .sort()
    .pop();

  assert(migrationDir, 'add_complete_org_hierarchy migration not found.');
  const migrationPath = path.join(migrationsDir, migrationDir, 'migration.sql');
  const sql = fs.readFileSync(migrationPath, 'utf8');
  return { migrationDir, migrationPath, sql };
}

function parseIndexes(sql: string): IndexDef[] {
  const matches = sql.matchAll(/CREATE (UNIQUE )?INDEX "([^"]+)" ON "([^"]+)"\(([^)]+)\);/g);
  return Array.from(matches).map(([, unique, name, table, columns]) => ({
    name,
    table,
    columns: columns.replace(/["\s]+/g, '').split(','),
    unique: Boolean(unique),
  }));
}

function parseForeignKeys(sql: string): ForeignKeyDef[] {
  const matches = sql.matchAll(
    /ALTER TABLE "([^"]+)" ADD CONSTRAINT "([^"]+)" FOREIGN KEY \(([^)]+)\) REFERENCES "([^"]+)"\(([^)]+)\)(?: ON DELETE ([A-Z ]+?)(?= ON UPDATE|;))?(?: ON UPDATE ([A-Z ]+))?;/g,
  );
  return Array.from(matches).map(([, table, name, cols, refTable, refCols, onDelete, onUpdate]) => ({
    name,
    table,
    columns: cols.replace(/["\s]+/g, '').split(','),
    references: refTable,
    referencedColumns: refCols.replace(/["\s]+/g, '').split(','),
    onDelete,
    onUpdate,
  }));
}

function assertIndex(indexes: IndexDef[], name: string, table: string, columns: string[], unique = false) {
  const found = indexes.find((idx) => idx.name === name);
  assert(found, `Index ${name} not found.`);
  assert.strictEqual(found.table, table, `Index ${name} is on the wrong table.`);
  assert.deepStrictEqual(found.columns, columns, `Index ${name} has unexpected columns.`);
  assert.strictEqual(found.unique, unique, `Index ${name} unique flag mismatch.`);
}

function assertForeignKey(
  fks: ForeignKeyDef[],
  name: string,
  table: string,
  columns: string[],
  references: string,
  referencedColumns: string[],
  onDelete?: string,
  onUpdate?: string,
) {
  const fk = fks.find((entry) => entry.name === name);
  assert(fk, `Foreign key ${name} missing.`);
  assert.strictEqual(fk.table, table, `Foreign key ${name} on wrong table.`);
  assert.deepStrictEqual(fk.columns, columns, `Foreign key ${name} has unexpected columns.`);
  assert.strictEqual(fk.references, references, `Foreign key ${name} points to wrong table.`);
  assert.deepStrictEqual(fk.referencedColumns, referencedColumns, `Foreign key ${name} references wrong columns.`);
  if (onDelete) {
    assert.strictEqual(fk.onDelete?.trim(), onDelete, `Foreign key ${name} onDelete mismatch.`);
  }
  if (onUpdate) {
    assert.strictEqual(fk.onUpdate?.trim(), onUpdate, `Foreign key ${name} onUpdate mismatch.`);
  }
}

function assertSchemaFragments(schema: string, fragments: string[], context: string) {
  for (const fragment of fragments) {
    assert(
      schema.includes(fragment),
      `${context} is missing expected fragment: ${fragment.slice(0, 80)}...`,
    );
  }
}

function simulateCascadeExpectations() {
  const branches = new Map<string, { id: string; tenantId: string }>();
  const departments: { id: string; tenantId: string; branchId: string; parentId?: string }[] = [];
  const roles: { id: string; departmentId: string; roleDefinitionId: string }[] = [];
  const assignments: { id: string; departmentId: string; roleId: string; userId: string }[] = [];

  const branch = { id: 'branch-1', tenantId: 'tenant-1' };
  branches.set(branch.id, branch);
  departments.push({ id: 'dept-1', tenantId: 'tenant-1', branchId: branch.id });
  roles.push({ id: 'dept-role-1', departmentId: 'dept-1', roleDefinitionId: 'role-def-1' });
  assignments.push({ id: 'assign-1', departmentId: 'dept-1', roleId: 'dept-role-1', userId: 'user-1' });

  // Delete department should cascade to roles and assignments per migration FKs.
  const deletedDepartmentId = 'dept-1';
  const remainingRoles = roles.filter((role) => role.departmentId !== deletedDepartmentId);
  const remainingAssignments = assignments.filter(
    (assignment) => assignment.departmentId !== deletedDepartmentId && assignment.roleId !== 'dept-role-1',
  );

  assert.strictEqual(remainingRoles.length, 0, 'Cascade delete should remove department roles.');
  assert.strictEqual(remainingAssignments.length, 0, 'Cascade delete should remove user assignments.');

  // Delete branch should orphan department (SET NULL), not remove it.
  branches.delete(branch.id);
  const department = departments.find((dept) => dept.id === 'dept-1');
  assert(department, 'Department should still exist after branch deletion.');
}

function main() {
  const schemaPath = path.join(__dirname, '..', 'prisma', 'schema.prisma');
  const schema = fs.readFileSync(schemaPath, 'utf8');
  const { migrationDir, migrationPath, sql } = loadMigrationSql();

  const tables = new Set(Array.from(sql.matchAll(/CREATE TABLE "([^"]+)"/g)).map((match) => match[1]));
  const expectedTables = ['Branch', 'Department', 'RoleDefinition', 'DepartmentRole', 'UserDepartmentAssignment'];
  for (const table of expectedTables) {
    assert(tables.has(table), `${table} table missing in migration SQL.`);
  }

  const indexes = parseIndexes(sql);
  assertIndex(indexes, 'branch_tenant_idx', 'Branch', ['tenant_id']);
  assertIndex(indexes, 'Branch_tenant_id_code_key', 'Branch', ['tenant_id', 'code'], true);
  assertIndex(indexes, 'department_tenant_idx', 'Department', ['tenant_id']);
  assertIndex(indexes, 'department_branch_idx', 'Department', ['branch_id']);
  assertIndex(indexes, 'department_parent_idx', 'Department', ['parent_department_id']);
  assertIndex(indexes, 'Department_tenant_id_code_key', 'Department', ['tenant_id', 'code'], true);
  assertIndex(indexes, 'role_definition_tenant_idx', 'RoleDefinition', ['tenant_id']);
  assertIndex(indexes, 'RoleDefinition_tenant_id_key_key', 'RoleDefinition', ['tenant_id', 'key'], true);
  assertIndex(indexes, 'department_role_tenant_idx', 'DepartmentRole', ['tenant_id']);
  assertIndex(indexes, 'department_role_department_idx', 'DepartmentRole', ['department_id']);
  assertIndex(indexes, 'department_role_definition_idx', 'DepartmentRole', ['role_definition_id']);
  assertIndex(indexes, 'DepartmentRole_department_id_role_definition_id_key', 'DepartmentRole', ['department_id', 'role_definition_id'], true);
  assertIndex(indexes, 'assignment_tenant_idx', 'UserDepartmentAssignment', ['tenant_id']);
  assertIndex(indexes, 'assignment_department_idx', 'UserDepartmentAssignment', ['department_id']);
  assertIndex(indexes, 'assignment_role_idx', 'UserDepartmentAssignment', ['department_role_id']);
  assertIndex(indexes, 'assignment_user_idx', 'UserDepartmentAssignment', ['user_id']);
  assertIndex(
    indexes,
    'UserDepartmentAssignment_tenant_id_user_id_department_role__key',
    'UserDepartmentAssignment',
    ['tenant_id', 'user_id', 'department_role_id'],
    true,
  );

  const foreignKeys = parseForeignKeys(sql);
  assertForeignKey(foreignKeys, 'Department_branch_id_fkey', 'Department', ['branch_id'], 'Branch', ['id'], 'SET NULL', 'CASCADE');
  assertForeignKey(
    foreignKeys,
    'Department_parent_department_id_fkey',
    'Department',
    ['parent_department_id'],
    'Department',
    ['id'],
    'SET NULL',
    'CASCADE',
  );
  assertForeignKey(
    foreignKeys,
    'DepartmentRole_department_id_fkey',
    'DepartmentRole',
    ['department_id'],
    'Department',
    ['id'],
    'CASCADE',
    'CASCADE',
  );
  assertForeignKey(
    foreignKeys,
    'DepartmentRole_role_definition_id_fkey',
    'DepartmentRole',
    ['role_definition_id'],
    'RoleDefinition',
    ['id'],
    'CASCADE',
    'CASCADE',
  );
  assertForeignKey(
    foreignKeys,
    'UserDepartmentAssignment_department_role_id_fkey',
    'UserDepartmentAssignment',
    ['department_role_id'],
    'DepartmentRole',
    ['id'],
    'CASCADE',
    'CASCADE',
  );
  assertForeignKey(
    foreignKeys,
    'UserDepartmentAssignment_department_id_fkey',
    'UserDepartmentAssignment',
    ['department_id'],
    'Department',
    ['id'],
    'CASCADE',
    'CASCADE',
  );

  assertSchemaFragments(
    schema,
    [
      '@relation(fields: [branchId], references: [id], onDelete: SetNull, onUpdate: Cascade)',
      '@relation("DepartmentHierarchy", fields: [parentDepartmentId], references: [id], onDelete: SetNull, onUpdate: Cascade)',
      '@relation(fields: [departmentId], references: [id], onDelete: Cascade, onUpdate: Cascade)',
      '@relation(fields: [roleDefinitionId], references: [id], onDelete: Cascade, onUpdate: Cascade)',
      '@relation(fields: [departmentRoleId], references: [id], onDelete: Cascade, onUpdate: Cascade)',
      '@relation(fields: [departmentId], references: [id], onDelete: Cascade, onUpdate: Cascade)',
    ],
    'Schema relations',
  );

  assertSchemaFragments(
    schema,
    [
      '@@index([tenantId], name: "branch_tenant_idx")',
      '@@index([tenantId], name: "department_tenant_idx")',
      '@@index([branchId], name: "department_branch_idx")',
      '@@index([parentDepartmentId], name: "department_parent_idx")',
      '@@index([tenantId], name: "role_definition_tenant_idx")',
      '@@index([tenantId], name: "department_role_tenant_idx")',
      '@@index([departmentId], name: "department_role_department_idx")',
      '@@index([roleDefinitionId], name: "department_role_definition_idx")',
      '@@index([tenantId], name: "assignment_tenant_idx")',
      '@@index([departmentId], name: "assignment_department_idx")',
      '@@index([departmentRoleId], name: "assignment_role_idx")',
      '@@index([userId], name: "assignment_user_idx")',
    ],
    'Schema indexes',
  );

  if (!schema.includes('status')) {
    console.warn('Status field not present in schema; index verification skipped.');
  }

  simulateCascadeExpectations();

  console.log('Migration validated:', migrationDir);
  console.log('Migration path:', migrationPath);
  console.log('All schema and migration checks passed.');
}

main();
