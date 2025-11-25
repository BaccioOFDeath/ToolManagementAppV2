-- CreateTable
CREATE TABLE "Branch" (
    "id" TEXT NOT NULL,
    "tenant_id" TEXT NOT NULL,
    "code" TEXT NOT NULL,
    "name" TEXT NOT NULL,
    "metadata" JSONB DEFAULT '{}',
    "created_at" TIMESTAMP(3) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_at" TIMESTAMP(3) NOT NULL,

    CONSTRAINT "Branch_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "Department" (
    "id" TEXT NOT NULL,
    "tenant_id" TEXT NOT NULL,
    "branch_id" TEXT,
    "code" TEXT NOT NULL,
    "name" TEXT NOT NULL,
    "parent_department_id" TEXT,
    "metadata" JSONB DEFAULT '{}',
    "created_at" TIMESTAMP(3) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_at" TIMESTAMP(3) NOT NULL,

    CONSTRAINT "Department_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "RoleDefinition" (
    "id" TEXT NOT NULL,
    "tenant_id" TEXT NOT NULL,
    "key" TEXT NOT NULL,
    "display_name" TEXT NOT NULL,
    "description" TEXT,
    "scope" TEXT NOT NULL,
    "permissions" JSONB NOT NULL,
    "inherits_from_ids" TEXT[] DEFAULT ARRAY[]::TEXT[],
    "is_system" BOOLEAN NOT NULL DEFAULT false,
    "created_at" TIMESTAMP(3) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_at" TIMESTAMP(3) NOT NULL,

    CONSTRAINT "RoleDefinition_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "DepartmentRole" (
    "id" TEXT NOT NULL,
    "tenant_id" TEXT NOT NULL,
    "department_id" TEXT NOT NULL,
    "role_definition_id" TEXT NOT NULL,
    "is_default" BOOLEAN NOT NULL DEFAULT false,
    "created_at" TIMESTAMP(3) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_at" TIMESTAMP(3) NOT NULL,

    CONSTRAINT "DepartmentRole_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "UserDepartmentAssignment" (
    "id" TEXT NOT NULL,
    "tenant_id" TEXT NOT NULL,
    "user_id" TEXT NOT NULL,
    "department_role_id" TEXT NOT NULL,
    "department_id" TEXT NOT NULL,
    "is_primary" BOOLEAN NOT NULL DEFAULT false,
    "assigned_at" TIMESTAMP(3) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "removed_at" TIMESTAMP(3),

    CONSTRAINT "UserDepartmentAssignment_pkey" PRIMARY KEY ("id")
);

-- CreateIndex
CREATE INDEX "branch_tenant_idx" ON "Branch"("tenant_id");

-- CreateIndex
CREATE UNIQUE INDEX "Branch_tenant_id_code_key" ON "Branch"("tenant_id", "code");

-- CreateIndex
CREATE INDEX "department_tenant_idx" ON "Department"("tenant_id");

-- CreateIndex
CREATE INDEX "department_branch_idx" ON "Department"("branch_id");

-- CreateIndex
CREATE INDEX "department_parent_idx" ON "Department"("parent_department_id");

-- CreateIndex
CREATE UNIQUE INDEX "Department_tenant_id_code_key" ON "Department"("tenant_id", "code");

-- CreateIndex
CREATE INDEX "role_definition_tenant_idx" ON "RoleDefinition"("tenant_id");

-- CreateIndex
CREATE UNIQUE INDEX "RoleDefinition_tenant_id_key_key" ON "RoleDefinition"("tenant_id", "key");

-- CreateIndex
CREATE INDEX "department_role_tenant_idx" ON "DepartmentRole"("tenant_id");

-- CreateIndex
CREATE INDEX "department_role_department_idx" ON "DepartmentRole"("department_id");

-- CreateIndex
CREATE INDEX "department_role_definition_idx" ON "DepartmentRole"("role_definition_id");

-- CreateIndex
CREATE UNIQUE INDEX "DepartmentRole_department_id_role_definition_id_key" ON "DepartmentRole"("department_id", "role_definition_id");

-- CreateIndex
CREATE INDEX "assignment_tenant_idx" ON "UserDepartmentAssignment"("tenant_id");

-- CreateIndex
CREATE INDEX "assignment_department_idx" ON "UserDepartmentAssignment"("department_id");

-- CreateIndex
CREATE INDEX "assignment_role_idx" ON "UserDepartmentAssignment"("department_role_id");

-- CreateIndex
CREATE INDEX "assignment_user_idx" ON "UserDepartmentAssignment"("user_id");

-- CreateIndex
CREATE UNIQUE INDEX "UserDepartmentAssignment_tenant_id_user_id_department_role__key" ON "UserDepartmentAssignment"("tenant_id", "user_id", "department_role_id");

-- AddForeignKey
ALTER TABLE "Department" ADD CONSTRAINT "Department_branch_id_fkey" FOREIGN KEY ("branch_id") REFERENCES "Branch"("id") ON DELETE SET NULL ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "Department" ADD CONSTRAINT "Department_parent_department_id_fkey" FOREIGN KEY ("parent_department_id") REFERENCES "Department"("id") ON DELETE SET NULL ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "DepartmentRole" ADD CONSTRAINT "DepartmentRole_department_id_fkey" FOREIGN KEY ("department_id") REFERENCES "Department"("id") ON DELETE CASCADE ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "DepartmentRole" ADD CONSTRAINT "DepartmentRole_role_definition_id_fkey" FOREIGN KEY ("role_definition_id") REFERENCES "RoleDefinition"("id") ON DELETE CASCADE ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "UserDepartmentAssignment" ADD CONSTRAINT "UserDepartmentAssignment_department_role_id_fkey" FOREIGN KEY ("department_role_id") REFERENCES "DepartmentRole"("id") ON DELETE CASCADE ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "UserDepartmentAssignment" ADD CONSTRAINT "UserDepartmentAssignment_department_id_fkey" FOREIGN KEY ("department_id") REFERENCES "Department"("id") ON DELETE CASCADE ON UPDATE CASCADE;

