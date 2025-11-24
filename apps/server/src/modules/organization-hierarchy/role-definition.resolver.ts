import { Args, Int, Mutation, Parent, Query, ResolveField, Resolver } from '@nestjs/graphql';
import { UseGuards } from '@nestjs/common';
import { PrismaClient } from '@prisma/client';
import { RequirePermission } from './require-permission.decorator';
import { manageRolesPermission } from './permissions';
import { RoleDefinitionService } from './services/role-definition.service';
import { CreateRoleDefinitionDto, UpdateRoleDefinitionDto } from './dto/role-definition.dto';
import { CurrentOrganizationContext } from './current-organization-context.decorator';
import { OrganizationContext } from './organization-context';
import { RoleDefinition, RoleDefinitionConnection, PageInfo } from './types';
import { CurrentUser, CurrentUserContext } from './current-user.decorator';
import { OrganizationAccessGuard } from './organization-access.guard';

@Resolver(() => RoleDefinition)
@UseGuards(OrganizationAccessGuard)
export class RoleDefinitionResolver {
  private readonly prisma: PrismaClient;
  private readonly roleService: RoleDefinitionService;

  constructor(prisma?: PrismaClient, roleService?: RoleDefinitionService) {
    this.prisma = prisma ?? new PrismaClient();
    this.roleService = roleService ?? new RoleDefinitionService(this.prisma);
  }

  private buildPageInfo<T extends { id: string }>(items: T[], limit: number): PageInfo {
    const hasNextPage = items.length > limit;
    const trimmed = hasNextPage ? items.slice(0, limit) : items;
    return {
      endCursor: trimmed.length ? trimmed[trimmed.length - 1].id : null,
      hasNextPage,
    };
  }

  @Query(() => RoleDefinitionConnection)
  @RequirePermission(manageRolesPermission())
  async roleDefinitions(
    @Args('cursor', { type: () => String, nullable: true }) cursor: string | undefined,
    @Args('limit', { type: () => Int, nullable: true }) limit = 25,
    @CurrentOrganizationContext() organization: OrganizationContext,
    @CurrentUser() _user: CurrentUserContext,
  ): Promise<RoleDefinitionConnection> {
    const take = Math.min(limit, 50);
    const roles = await this.prisma.roleDefinition.findMany({
      where: { OR: [{ tenantId: organization.tenantId! }, { isSystem: true }] },
      orderBy: { displayName: 'asc' },
      take: take + 1,
      ...(cursor ? { cursor: { id: cursor }, skip: 1 } : {}),
    });

    const hasNextPage = roles.length > take;
    const trimmed = hasNextPage ? roles.slice(0, take) : roles;

    return {
      edges: trimmed.map((role) => ({ cursor: role.id, node: role as any })),
      pageInfo: this.buildPageInfo(roles, take),
    };
  }

  @Mutation(() => RoleDefinition)
  @RequirePermission(manageRolesPermission())
  async createRoleDefinition(
    @Args('input') input: CreateRoleDefinitionDto,
    @CurrentOrganizationContext() organization: OrganizationContext,
    @CurrentUser() _user: CurrentUserContext,
  ) {
    return this.roleService.createRoleDefinition(organization.tenantId!, input);
  }

  @Mutation(() => RoleDefinition)
  @RequirePermission(manageRolesPermission())
  async updateRoleDefinition(
    @Args('id', { type: () => String }) id: string,
    @Args('input') input: UpdateRoleDefinitionDto,
    @CurrentOrganizationContext() organization: OrganizationContext,
    @CurrentUser() _user: CurrentUserContext,
  ) {
    return this.roleService.updateRoleDefinition(id, organization.tenantId!, input);
  }

  @Query(() => [String])
  @RequirePermission(manageRolesPermission())
  async resolveRolePermissions(
    @Args('roleDefinitionId', { type: () => String }) roleDefinitionId: string,
    @CurrentOrganizationContext() organization: OrganizationContext,
    @CurrentUser() _user: CurrentUserContext,
  ) {
    return this.roleService.resolvePermissions(roleDefinitionId, organization.tenantId!);
  }

  @ResolveField(() => [RoleDefinition], { nullable: true })
  async inheritsFrom(@CurrentOrganizationContext() organization: OrganizationContext, @Parent() role: RoleDefinition) {
    if (!role.inheritsFromIds?.length) {
      return [];
    }

    return this.prisma.roleDefinition.findMany({
      where: {
        id: { in: role.inheritsFromIds },
        OR: [{ tenantId: organization.tenantId! }, { isSystem: true }],
      },
    });
  }
}
