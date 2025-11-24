-- Draft migration for organizational hierarchy and access control

-- Branch captures physical or logical sites for tenant operations
CREATE TABLE IF NOT EXISTS "Branch" (
  "id" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  "tenant_id" TEXT NOT NULL,
  "code" TEXT NOT NULL,
  "name" TEXT NOT NULL,
  "metadata" JSONB DEFAULT '{}'::jsonb,
  "created_at" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  "updated_at" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  CONSTRAINT "branch_code_per_tenant" UNIQUE ("tenant_id", "code")
);

-- Department supports nested hierarchy under a branch
CREATE TABLE IF NOT EXISTS "Department" (
  "id" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  "tenant_id" TEXT NOT NULL,
  "branch_id" UUID,
  "code" TEXT NOT NULL,
  "name" TEXT NOT NULL,
  "parent_department_id" UUID,
  "metadata" JSONB DEFAULT '{}'::jsonb,
  "created_at" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  "updated_at" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  CONSTRAINT "department_code_per_tenant" UNIQUE ("tenant_id", "code"),
  CONSTRAINT "department_parent_fk" FOREIGN KEY ("parent_department_id") REFERENCES "Department"("id") ON DELETE SET NULL ON UPDATE CASCADE,
  CONSTRAINT "department_branch_fk" FOREIGN KEY ("branch_id") REFERENCES "Branch"("id") ON DELETE SET NULL ON UPDATE CASCADE
);

-- RoleDefinition captures reusable permission bundles with inheritance
CREATE TABLE IF NOT EXISTS "RoleDefinition" (
  "id" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  "tenant_id" TEXT NOT NULL,
  "key" TEXT NOT NULL,
  "display_name" TEXT NOT NULL,
  "description" TEXT,
  "scope" TEXT NOT NULL,
  "permissions" JSONB NOT NULL,
  "inherits_from_ids" UUID[] NOT NULL DEFAULT '{}',
  "is_system" BOOLEAN NOT NULL DEFAULT FALSE,
  "created_at" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  "updated_at" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  CONSTRAINT "role_key_per_tenant" UNIQUE ("tenant_id", "key")
);

-- DepartmentRole associates role definitions to department contexts
CREATE TABLE IF NOT EXISTS "DepartmentRole" (
  "id" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  "tenant_id" TEXT NOT NULL,
  "department_id" UUID NOT NULL,
  "role_definition_id" UUID NOT NULL,
  "is_default" BOOLEAN NOT NULL DEFAULT FALSE,
  "created_at" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  "updated_at" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  CONSTRAINT "department_role_per_definition" UNIQUE ("department_id", "role_definition_id"),
  CONSTRAINT "department_role_department_fk" FOREIGN KEY ("department_id") REFERENCES "Department"("id") ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT "department_role_definition_fk" FOREIGN KEY ("role_definition_id") REFERENCES "RoleDefinition"("id") ON DELETE CASCADE ON UPDATE CASCADE
);

-- UserDepartmentAssignment links users to roles within departments
CREATE TABLE IF NOT EXISTS "UserDepartmentAssignment" (
  "id" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  "tenant_id" TEXT NOT NULL,
  "user_id" TEXT NOT NULL,
  "department_role_id" UUID NOT NULL,
  "department_id" UUID NOT NULL,
  "is_primary" BOOLEAN NOT NULL DEFAULT FALSE,
  "assigned_at" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  "removed_at" TIMESTAMPTZ,
  CONSTRAINT "user_role_per_tenant" UNIQUE ("tenant_id", "user_id", "department_role_id"),
  CONSTRAINT "assignment_role_fk" FOREIGN KEY ("department_role_id") REFERENCES "DepartmentRole"("id") ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT "assignment_department_fk" FOREIGN KEY ("department_id") REFERENCES "Department"("id") ON DELETE CASCADE ON UPDATE CASCADE
);

-- Trigger to maintain updated_at timestamps
CREATE OR REPLACE FUNCTION set_current_timestamp_updated_at()
RETURNS TRIGGER AS $$
BEGIN
  NEW."updated_at" = NOW();
  RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER department_set_updated_at BEFORE UPDATE ON "Department"
FOR EACH ROW EXECUTE PROCEDURE set_current_timestamp_updated_at();

CREATE TRIGGER role_definition_set_updated_at BEFORE UPDATE ON "RoleDefinition"
FOR EACH ROW EXECUTE PROCEDURE set_current_timestamp_updated_at();

CREATE TRIGGER department_role_set_updated_at BEFORE UPDATE ON "DepartmentRole"
FOR EACH ROW EXECUTE PROCEDURE set_current_timestamp_updated_at();

CREATE TRIGGER user_department_assignment_set_updated_at BEFORE UPDATE ON "UserDepartmentAssignment"
FOR EACH ROW EXECUTE PROCEDURE set_current_timestamp_updated_at();
