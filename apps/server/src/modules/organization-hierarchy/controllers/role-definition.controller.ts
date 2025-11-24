import { Body, Controller, Get, Param, Patch, Post, Query, UseGuards } from '@nestjs/common';
import { PrismaClient } from '@prisma/client';
import { RoleDefinitionService } from '../services/role-definition.service';
import { CreateRoleDefinitionDto, UpdateRoleDefinitionDto } from '../dto/role-definition.dto';
import { RequirePermission } from '../require-permission.decorator';
import { manageRolesPermission } from '../permissions';
import { OrganizationAccessGuard } from '../organization-access.guard';
import { CurrentOrganizationContext } from '../current-organization-context.decorator';
import { OrganizationContext } from '../organization-context';
import { CurrentUser, CurrentUserContext } from '../current-user.decorator';

@Controller('role-definitions')
@UseGuards(OrganizationAccessGuard)
export class RoleDefinitionController {
  private readonly prisma: PrismaClient;
  private readonly roleService: RoleDefinitionService;

  constructor(prisma?: PrismaClient, roleService?: RoleDefinitionService) {
    this.prisma = prisma ?? new PrismaClient();
    this.roleService = roleService ?? new RoleDefinitionService(this.prisma);
  }

  @Get()
  @RequirePermission(manageRolesPermission())
  async list(
    @Query('cursor') cursor: string | undefined,
    @Query('limit') limit = '25',
    @CurrentOrganizationContext() organization: OrganizationContext,
    @CurrentUser() _user: CurrentUserContext,
  ) {
    const take = Math.min(parseInt(limit, 10) || 25, 50);
    const roles = await this.prisma.roleDefinition.findMany({
      where: { OR: [{ tenantId: organization.tenantId! }, { isSystem: true }] },
      orderBy: { displayName: 'asc' },
      take: take + 1,
      ...(cursor ? { cursor: { id: cursor }, skip: 1 } : {}),
    });

    const hasNextPage = roles.length > take;
    const trimmed = hasNextPage ? roles.slice(0, take) : roles;
    return { data: trimmed, pageInfo: { endCursor: trimmed.at(-1)?.id ?? null, hasNextPage } };
  }

  @Post()
  @RequirePermission(manageRolesPermission())
  async create(
    @Body() body: CreateRoleDefinitionDto,
    @CurrentOrganizationContext() organization: OrganizationContext,
    @CurrentUser() _user: CurrentUserContext,
  ) {
    return this.roleService.createRoleDefinition(organization.tenantId!, body);
  }

  @Patch(':id')
  @RequirePermission(manageRolesPermission())
  async update(
    @Param('id') id: string,
    @Body() body: UpdateRoleDefinitionDto,
    @CurrentOrganizationContext() organization: OrganizationContext,
    @CurrentUser() _user: CurrentUserContext,
  ) {
    return this.roleService.updateRoleDefinition(id, organization.tenantId!, body);
  }

  @Get(':id/permissions')
  @RequirePermission(manageRolesPermission())
  async resolvePermissions(
    @Param('id') roleDefinitionId: string,
    @CurrentOrganizationContext() organization: OrganizationContext,
    @CurrentUser() _user: CurrentUserContext,
  ) {
    return this.roleService.resolvePermissions(roleDefinitionId, organization.tenantId!);
  }
}
