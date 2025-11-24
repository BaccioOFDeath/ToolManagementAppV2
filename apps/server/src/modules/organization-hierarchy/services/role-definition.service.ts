import { PrismaClient, RoleDefinition } from '@prisma/client';
import { ConflictError, ForbiddenError, NotFoundError, ValidationError } from '../../../common/exceptions';
import { CreateRoleDefinitionDto, UpdateRoleDefinitionDto } from '../dto/role-definition.dto';

export class RoleDefinitionService {
  constructor(private readonly prisma: PrismaClient) {}

  private async validateInheritance(tenantId: string, inheritsFromIds?: string[]) {
    if (!inheritsFromIds?.length) {
      return;
    }

    const parents = await this.prisma.roleDefinition.findMany({
      where: { id: { in: inheritsFromIds } },
    });

    if (parents.length !== inheritsFromIds.length) {
      throw new ValidationError('One or more parent roles do not exist.');
    }

    parents.forEach((parent) => {
      if (parent.tenantId !== tenantId && !parent.isSystem) {
        throw new ForbiddenError('Cannot inherit roles from other tenants.');
      }
    });
  }

  private ensureTenant(role: RoleDefinition | null, tenantId: string) {
    if (!role) {
      throw new NotFoundError('Role definition not found.');
    }

    if (role.tenantId !== tenantId && !role.isSystem) {
      throw new ForbiddenError('Role definition not accessible for tenant.');
    }
  }

  async createRoleDefinition(tenantId: string, dto: CreateRoleDefinitionDto) {
    const existing = await this.prisma.roleDefinition.findFirst({ where: { tenantId, key: dto.key } });
    if (existing) {
      throw new ConflictError('Role key already exists for tenant.');
    }

    await this.validateInheritance(tenantId, dto.inheritsFromIds);

    return this.prisma.roleDefinition.create({
      data: {
        tenantId,
        key: dto.key,
        displayName: dto.displayName,
        description: dto.description,
        scope: dto.scope,
        permissions: dto.permissions,
        inheritsFromIds: dto.inheritsFromIds ?? [],
        isSystem: dto.isSystem ?? false,
      },
    });
  }

  async updateRoleDefinition(id: string, tenantId: string, dto: UpdateRoleDefinitionDto) {
    const role = await this.prisma.roleDefinition.findUnique({ where: { id } });
    this.ensureTenant(role, tenantId);

    if (role!.isSystem && dto.isSystem === false) {
      throw new ForbiddenError('System roles cannot be downgraded.');
    }

    await this.validateInheritance(tenantId, dto.inheritsFromIds ?? role!.inheritsFromIds);

    return this.prisma.roleDefinition.update({
      where: { id },
      data: {
        displayName: dto.displayName ?? role!.displayName,
        description: dto.description ?? role!.description,
        scope: dto.scope ?? role!.scope,
        permissions: dto.permissions ?? (role!.permissions as string[]),
        inheritsFromIds: dto.inheritsFromIds ?? role!.inheritsFromIds,
        isSystem: dto.isSystem ?? role!.isSystem,
      },
    });
  }

  async listTenantRoles(tenantId: string) {
    return this.prisma.roleDefinition.findMany({
      where: {
        OR: [{ tenantId }, { isSystem: true }],
      },
      orderBy: { displayName: 'asc' },
    });
  }

  async resolvePermissions(roleId: string, tenantId: string) {
    const resolved = new Set<string>();
    const visited = new Set<string>();

    const traverse = async (id: string) => {
      if (visited.has(id)) {
        return;
      }
      visited.add(id);

      const role = await this.prisma.roleDefinition.findUnique({ where: { id } });
      this.ensureTenant(role, tenantId);

      (role!.permissions as string[]).forEach((permission) => resolved.add(permission));

      if (role!.inheritsFromIds?.length) {
        await Promise.all(role!.inheritsFromIds.map((parentId) => traverse(parentId)));
      }
    };

    await traverse(roleId);
    return Array.from(resolved);
  }
}
