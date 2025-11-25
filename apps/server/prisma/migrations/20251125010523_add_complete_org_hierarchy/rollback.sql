-- Rollback for add_complete_org_hierarchy
-- Drops organizational hierarchy tables and cascades all related records.
-- Data impact: all branch, department, role, and assignment records will be permanently removed.

DROP TABLE IF EXISTS "UserDepartmentAssignment";
DROP TABLE IF EXISTS "DepartmentRole";
DROP TABLE IF EXISTS "Department";
DROP TABLE IF EXISTS "RoleDefinition";
DROP TABLE IF EXISTS "Branch";
