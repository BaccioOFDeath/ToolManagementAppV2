import { OrganizationContext } from './organization-context';

export type PermissionRequirement = string | ((context: OrganizationContext) => string | string[]);

export function manageBranchPermission(branchId?: string): PermissionRequirement {
  return (context: OrganizationContext) => `branch.manage:${branchId ?? context.branchId ?? '*'}`;
}

export function manageDepartmentPermission(departmentId?: string): PermissionRequirement {
  return (context: OrganizationContext) => `department.manage:${departmentId ?? context.departmentId ?? '*'}`;
}

export function assignUsersPermission(departmentId?: string): PermissionRequirement {
  return (context: OrganizationContext) => `department.assign:${departmentId ?? context.departmentId ?? '*'}`;
}

export function manageRolesPermission(): PermissionRequirement {
  return 'roles.manage';
}

export function resolvePermissionRequirements(
  requirements: PermissionRequirement[],
  context: OrganizationContext,
): string[] {
  const permissions = requirements.flatMap((requirement) => {
    const value = typeof requirement === 'function' ? requirement(context) : requirement;
    return Array.isArray(value) ? value : [value];
  });

  return Array.from(new Set(permissions.filter(Boolean)));
}
