import { Args, Int, Mutation, Parent, Query, ResolveField, Resolver } from '@nestjs/graphql';
import { UseGuards } from '@nestjs/common';
import { PrismaClient } from '@prisma/client';
import { UserAssignmentService } from './services/user-assignment.service';
import { AssignUserDto, TerminateAssignmentDto, TransferUserDto } from './dto/user-assignment.dto';
import { AssignmentConnection, Department, DepartmentRole, PageInfo, UserDepartmentAssignment } from './types';
import { OrganizationAccessGuard } from './organization-access.guard';
import { RequirePermission } from './require-permission.decorator';
import { assignUsersPermission } from './permissions';
import { CurrentOrganizationContext } from './current-organization-context.decorator';
import { OrganizationContext } from './organization-context';
import { CurrentUser, CurrentUserContext } from './current-user.decorator';
import { AccessControlService } from './services/access-control.service';

@Resolver(() => UserDepartmentAssignment)
@UseGuards(OrganizationAccessGuard)
export class AssignmentResolver {
  private readonly prisma: PrismaClient;
  private readonly assignmentService: UserAssignmentService;

  constructor(prisma?: PrismaClient, assignmentService?: UserAssignmentService) {
    this.prisma = prisma ?? new PrismaClient();
    const accessControl = new AccessControlService(this.prisma);
    this.assignmentService = assignmentService ?? new UserAssignmentService(this.prisma, accessControl);
  }

  private buildPageInfo<T extends { id: string }>(items: T[], limit: number): PageInfo {
    const hasNextPage = items.length > limit;
    const trimmed = hasNextPage ? items.slice(0, limit) : items;
    return { endCursor: trimmed.length ? trimmed[trimmed.length - 1].id : null, hasNextPage };
  }

  @Query(() => AssignmentConnection)
  @RequirePermission(assignUsersPermission())
  async assignments(
    @Args('userId', { type: () => String, nullable: true }) userId: string | undefined,
    @Args('departmentId', { type: () => String, nullable: true }) departmentId: string | undefined,
    @Args('cursor', { type: () => String, nullable: true }) cursor: string | undefined,
    @Args('limit', { type: () => Int, nullable: true }) limit = 25,
    @CurrentOrganizationContext() organization: OrganizationContext,
    @CurrentUser() _user: CurrentUserContext,
  ): Promise<AssignmentConnection> {
    const take = Math.min(limit, 50);
    const assignments = await this.prisma.userDepartmentAssignment.findMany({
      where: {
        tenantId: organization.tenantId!,
        ...(userId ? { userId } : {}),
        ...(departmentId ? { departmentId } : {}),
      },
      include: { department: true, departmentRole: { include: { definition: true } } },
      orderBy: { assignedAt: 'desc' },
      take: take + 1,
      ...(cursor ? { cursor: { id: cursor }, skip: 1 } : {}),
    });

    const hasNextPage = assignments.length > take;
    const trimmed = hasNextPage ? assignments.slice(0, take) : assignments;

    return {
      edges: trimmed.map((assignment) => ({ cursor: assignment.id, node: assignment as any })),
      pageInfo: this.buildPageInfo(assignments, take),
    };
  }

  @Mutation(() => UserDepartmentAssignment)
  @RequirePermission(assignUsersPermission())
  async assignUser(
    @Args('input') input: AssignUserDto,
    @CurrentOrganizationContext() organization: OrganizationContext,
    @CurrentUser() _user: CurrentUserContext,
  ) {
    return this.assignmentService.assignUser({ ...input, tenantId: organization.tenantId! });
  }

  @Mutation(() => UserDepartmentAssignment)
  @RequirePermission(assignUsersPermission())
  async transferAssignment(
    @Args('input') input: TransferUserDto,
    @CurrentOrganizationContext() organization: OrganizationContext,
    @CurrentUser() _user: CurrentUserContext,
  ) {
    return this.assignmentService.transfer({ ...input, tenantId: organization.tenantId! });
  }

  @Mutation(() => UserDepartmentAssignment)
  @RequirePermission(assignUsersPermission())
  async terminateAssignment(
    @Args('input') input: TerminateAssignmentDto,
    @CurrentOrganizationContext() organization: OrganizationContext,
    @CurrentUser() _user: CurrentUserContext,
  ) {
    return this.assignmentService.terminateAssignment({ ...input, tenantId: organization.tenantId! });
  }

  @ResolveField(() => UserDepartmentAssignment, { nullable: true })
  async primaryAssignmentForUser(@Parent() assignment: UserDepartmentAssignment, @CurrentOrganizationContext() organization: OrganizationContext) {
    return this.prisma.userDepartmentAssignment.findFirst({
      where: {
        tenantId: organization.tenantId!,
        userId: assignment.userId,
        isPrimary: true,
        removedAt: null,
      },
      include: { department: true, departmentRole: { include: { definition: true } } },
      orderBy: { assignedAt: 'desc' },
    });
  }

  @ResolveField(() => Department, { nullable: true })
  async department(@Parent() assignment: UserDepartmentAssignment, @CurrentOrganizationContext() organization: OrganizationContext) {
    return this.prisma.department.findFirst({ where: { id: assignment.departmentId, tenantId: organization.tenantId! } });
  }

  @ResolveField(() => DepartmentRole, { nullable: true })
  async departmentRole(@Parent() assignment: UserDepartmentAssignment, @CurrentOrganizationContext() organization: OrganizationContext) {
    return this.prisma.departmentRole.findFirst({
      where: { id: assignment.departmentRoleId, tenantId: organization.tenantId! },
      include: { definition: true },
    });
  }
}
