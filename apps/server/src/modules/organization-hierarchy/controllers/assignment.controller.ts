import { Body, Controller, Get, Param, Patch, Post, Query, UseGuards } from '@nestjs/common';
import { PrismaClient } from '@prisma/client';
import { UserAssignmentService } from '../services/user-assignment.service';
import { AssignUserDto, TerminateAssignmentDto, TransferUserDto, StaffLookupDto } from '../dto/user-assignment.dto';
import { RequirePermission } from '../require-permission.decorator';
import { assignUsersPermission } from '../permissions';
import { OrganizationAccessGuard } from '../organization-access.guard';
import { CurrentOrganizationContext } from '../current-organization-context.decorator';
import { OrganizationContext } from '../organization-context';
import { CurrentUser, CurrentUserContext } from '../current-user.decorator';
import { AccessControlService } from '../services/access-control.service';

@Controller('assignments')
@UseGuards(OrganizationAccessGuard)
export class AssignmentController {
  private readonly prisma: PrismaClient;
  private readonly assignmentService: UserAssignmentService;

  constructor(prisma?: PrismaClient, assignmentService?: UserAssignmentService) {
    this.prisma = prisma ?? new PrismaClient();
    const accessControl = new AccessControlService(this.prisma);
    this.assignmentService = assignmentService ?? new UserAssignmentService(this.prisma, accessControl);
  }

  @Get()
  @RequirePermission(assignUsersPermission())
  async list(
    @Query('userId') userId: string | undefined,
    @Query('departmentId') departmentId: string | undefined,
    @Query('cursor') cursor: string | undefined,
    @Query('limit') limit = '25',
    @CurrentOrganizationContext() organization: OrganizationContext,
    @CurrentUser() _user: CurrentUserContext,
  ) {
    const take = Math.min(parseInt(limit, 10) || 25, 50);
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
    return { data: trimmed, pageInfo: { endCursor: trimmed.at(-1)?.id ?? null, hasNextPage } };
  }

  @Post()
  @RequirePermission(assignUsersPermission())
  async assign(
    @Body() body: AssignUserDto,
    @CurrentOrganizationContext() organization: OrganizationContext,
    @CurrentUser() _user: CurrentUserContext,
  ) {
    return this.assignmentService.assignUser({ ...body, tenantId: organization.tenantId! });
  }

  @Post('transfer')
  @RequirePermission(assignUsersPermission())
  async transfer(
    @Body() body: TransferUserDto,
    @CurrentOrganizationContext() organization: OrganizationContext,
    @CurrentUser() _user: CurrentUserContext,
  ) {
    return this.assignmentService.transfer({ ...body, tenantId: organization.tenantId! });
  }

  @Patch(':id/terminate')
  @RequirePermission(assignUsersPermission())
  async terminate(
    @Param('id') id: string,
    @Body() body: Omit<TerminateAssignmentDto, 'assignmentId' | 'tenantId'>,
    @CurrentOrganizationContext() organization: OrganizationContext,
    @CurrentUser() _user: CurrentUserContext,
  ) {
    return this.assignmentService.terminateAssignment({ ...body, assignmentId: id, tenantId: organization.tenantId! });
  }

  @Post('staff-lookup')
  @RequirePermission(assignUsersPermission())
  async lookup(
    @Body() body: StaffLookupDto,
    @CurrentOrganizationContext() organization: OrganizationContext,
    @CurrentUser() _user: CurrentUserContext,
  ) {
    return this.assignmentService.lookupStaff({ ...body, tenantId: organization.tenantId! });
  }
}
