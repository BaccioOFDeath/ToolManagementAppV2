import { PrismaClient } from '@prisma/client';
import Redis from 'ioredis';
import { ForbiddenError, NotFoundError } from '../../../common/exceptions';
import { RoleDefinitionService } from './role-definition.service';

export class AccessControlService {
  private readonly roleDefinitionService: RoleDefinitionService;

  constructor(private readonly prisma: PrismaClient, private readonly redis?: Redis) {
    this.roleDefinitionService = new RoleDefinitionService(prisma);
  }

  private cacheKey(tenantId: string, userId: string) {
    return `ac:permissions:${tenantId}:${userId}`;
  }

  async invalidateUserPermissionsCache(tenantId: string, userId: string) {
    if (!this.redis) return;
    await this.redis.del(this.cacheKey(tenantId, userId));
  }

  async getEffectivePermissions(tenantId: string, userId: string) {
    const key = this.cacheKey(tenantId, userId);
    if (this.redis) {
      const cached = await this.redis.get(key);
      if (cached) {
        return JSON.parse(cached) as string[];
      }
    }

    const assignments = await this.prisma.userDepartmentAssignment.findMany({
      where: { tenantId, userId, removedAt: null },
      include: {
        departmentRole: {
          include: {
            definition: true,
            department: true,
          },
        },
      },
    });

    if (!assignments.length) {
      throw new NotFoundError('User has no active assignments.');
    }

    const permissionSet = new Set<string>();
    const resolvedRoles = new Map<string, string[]>();

    for (const assignment of assignments) {
      const roleId = assignment.departmentRole.roleDefinitionId;
      if (!resolvedRoles.has(roleId)) {
        const permissions = await this.roleDefinitionService.resolvePermissions(roleId, tenantId);
        resolvedRoles.set(roleId, permissions);
      }
      resolvedRoles.get(roleId)?.forEach((p) => permissionSet.add(p));
    }

    const permissions = Array.from(permissionSet);
    if (this.redis) {
      await this.redis.set(key, JSON.stringify(permissions), 'EX', 300);
    }

    return permissions;
  }

  async ensurePermission(tenantId: string, userId: string, permission: string) {
    const permissions = await this.getEffectivePermissions(tenantId, userId);
    if (!permissions.includes(permission)) {
      throw new ForbiddenError('User does not have required permission.');
    }
    return true;
  }

  async getScopedDepartments(tenantId: string, userId: string, branchId?: string) {
    const assignments = await this.prisma.userDepartmentAssignment.findMany({
      where: {
        tenantId,
        userId,
        removedAt: null,
        ...(branchId ? { department: { branchId } } : {}),
      },
      include: { department: true },
    });

    return assignments.map((assignment) => assignment.department);
  }
}
