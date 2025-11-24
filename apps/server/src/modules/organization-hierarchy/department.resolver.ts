import { Args, Int, Mutation, Parent, Query, ResolveField, Resolver } from '@nestjs/graphql';
import { UseGuards } from '@nestjs/common';
import { PrismaClient } from '@prisma/client';
import { DepartmentService } from './services/department.service';
import { RequirePermission } from './require-permission.decorator';
import { manageDepartmentPermission } from './permissions';
import { CreateDepartmentDto, DepartmentFilterDto, UpdateDepartmentDto, AssignManagerDto } from './dto/department.dto';
import { Department, DepartmentConnection, PageInfo, UserDepartmentAssignment } from './types';
import { OrganizationAccessGuard } from './organization-access.guard';
import { CurrentOrganizationContext } from './current-organization-context.decorator';
import { OrganizationContext } from './organization-context';
import { UserDepartmentAssignment as AssignmentModel } from '@prisma/client';
import { CurrentUser } from './current-user.decorator';
import { CurrentUserContext } from './current-user.decorator';

@Resolver(() => Department)
@UseGuards(OrganizationAccessGuard)
export class DepartmentResolver {
  private readonly prisma: PrismaClient;
  private readonly departmentService: DepartmentService;

  constructor(prisma?: PrismaClient, departmentService?: DepartmentService) {
    this.prisma = prisma ?? new PrismaClient();
    this.departmentService = departmentService ?? new DepartmentService(this.prisma);
  }

  private buildPageInfo<T extends { id: string }>(items: T[], limit: number): PageInfo {
    const hasNextPage = items.length > limit;
    const trimmed = hasNextPage ? items.slice(0, limit) : items;
    return {
      endCursor: trimmed.length ? trimmed[trimmed.length - 1].id : null,
      hasNextPage,
    };
  }

  private wrapConnection<T extends { id: string }>(
    items: T[],
    limit: number,
    mapNode: (item: T) => any,
  ): { edges: { cursor: string; node: any }[]; pageInfo: PageInfo } {
    const pageInfo = this.buildPageInfo(items, limit);
    const trimmed = pageInfo.hasNextPage ? items.slice(0, limit) : items;
    return {
      edges: trimmed.map((item) => ({ cursor: item.id, node: mapNode(item) })),
      pageInfo,
    };
  }

  @Query(() => Department)
  @RequirePermission(manageDepartmentPermission())
  async department(
    @Args('id', { type: () => String }) id: string,
    @CurrentOrganizationContext() organization: OrganizationContext,
    @CurrentUser() _user: CurrentUserContext,
  ) {
    return this.departmentService.getDepartmentById(id, organization.tenantId!);
  }

  @Query(() => DepartmentConnection)
  @RequirePermission(manageDepartmentPermission())
  async departments(
    @Args('branchId', { type: () => String, nullable: true }) branchId: string | undefined,
    @Args('type', { type: () => String, nullable: true }) type: DepartmentFilterDto['type'],
    @Args('cursor', { type: () => String, nullable: true }) cursor: string | undefined,
    @Args('limit', { type: () => Int, nullable: true }) limit = 25,
    @CurrentOrganizationContext() organization: OrganizationContext,
    @CurrentUser() _user: CurrentUserContext,
  ): Promise<DepartmentConnection> {
    const take = Math.min(limit, 50);

    const where = {
      tenantId: organization.tenantId!,
      ...(branchId ? { branchId } : {}),
      ...(type ? { metadata: { path: ['type'], equals: type } } : {}),
    } as const;

    const departments = await this.prisma.department.findMany({
      where,
      include: { children: true, roles: true, assignments: true },
      orderBy: { createdAt: 'desc' },
      take: take + 1,
      ...(cursor ? { cursor: { id: cursor }, skip: 1 } : {}),
    });

    return this.wrapConnection(departments, take, (dept) => dept as unknown as Department);
  }

  @Mutation(() => Department)
  @RequirePermission(manageDepartmentPermission())
  async createDepartment(
    @Args('input') input: CreateDepartmentDto,
    @CurrentOrganizationContext() organization: OrganizationContext,
    @CurrentUser() _user: CurrentUserContext,
  ) {
    return this.departmentService.createDepartment(organization.tenantId!, input);
  }

  @Mutation(() => Department)
  @RequirePermission(manageDepartmentPermission())
  async updateDepartment(
    @Args('id', { type: () => String }) id: string,
    @Args('input') input: UpdateDepartmentDto,
    @CurrentOrganizationContext() organization: OrganizationContext,
    @CurrentUser() _user: CurrentUserContext,
  ) {
    return this.departmentService.updateDepartment(id, organization.tenantId!, input);
  }

  @Mutation(() => Boolean)
  @RequirePermission(manageDepartmentPermission())
  async deleteDepartment(
    @Args('id', { type: () => String }) id: string,
    @CurrentOrganizationContext() organization: OrganizationContext,
    @CurrentUser() _user: CurrentUserContext,
  ) {
    await this.departmentService.deleteDepartment(id, organization.tenantId!);
    return true;
  }

  @Mutation(() => Department)
  @RequirePermission(manageDepartmentPermission())
  async assignDepartmentManager(
    @Args('input') input: AssignManagerDto,
    @CurrentOrganizationContext() organization: OrganizationContext,
    @CurrentUser() _user: CurrentUserContext,
  ) {
    return this.departmentService.assignManager(organization.tenantId!, input);
  }

  @ResolveField(() => Number, { nullable: true })
  async staffCount(@Parent() department: Department, @CurrentOrganizationContext() organization: OrganizationContext) {
    return this.prisma.userDepartmentAssignment.count({
      where: { tenantId: organization.tenantId!, departmentId: department.id, removedAt: null },
    });
  }

  @ResolveField(() => UserDepartmentAssignment, { nullable: true })
  async primaryAssignment(
    @Parent() department: Department,
    @CurrentOrganizationContext() organization: OrganizationContext,
  ): Promise<AssignmentModel | null> {
    return this.prisma.userDepartmentAssignment.findFirst({
      where: { tenantId: organization.tenantId!, departmentId: department.id, isPrimary: true, removedAt: null },
      orderBy: { assignedAt: 'desc' },
      include: { departmentRole: true, department: true },
    });
  }
}
