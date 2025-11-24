import { Body, Controller, Delete, Get, Param, Patch, Post, Query, UseGuards } from '@nestjs/common';
import { PrismaClient } from '@prisma/client';
import { DepartmentService } from '../services/department.service';
import { CreateDepartmentDto, UpdateDepartmentDto, AssignManagerDto, DepartmentFilterDto } from '../dto/department.dto';
import { RequirePermission } from '../require-permission.decorator';
import { manageDepartmentPermission } from '../permissions';
import { OrganizationAccessGuard } from '../organization-access.guard';
import { CurrentOrganizationContext } from '../current-organization-context.decorator';
import { OrganizationContext } from '../organization-context';
import { CurrentUser } from '../current-user.decorator';
import { CurrentUserContext } from '../current-user.decorator';

@Controller('departments')
@UseGuards(OrganizationAccessGuard)
export class DepartmentController {
  private readonly prisma: PrismaClient;
  private readonly departmentService: DepartmentService;

  constructor(prisma?: PrismaClient, departmentService?: DepartmentService) {
    this.prisma = prisma ?? new PrismaClient();
    this.departmentService = departmentService ?? new DepartmentService(this.prisma);
  }

  @Get()
  @RequirePermission(manageDepartmentPermission())
  async list(
    @Query('branchId') branchId: string | undefined,
    @Query('type') type: DepartmentFilterDto['type'],
    @Query('cursor') cursor: string | undefined,
    @Query('limit') limit = '25',
    @CurrentOrganizationContext() organization: OrganizationContext,
    @CurrentUser() _user: CurrentUserContext,
  ) {
    const take = Math.min(parseInt(limit, 10) || 25, 50);
    const departments = await this.prisma.department.findMany({
      where: {
        tenantId: organization.tenantId!,
        ...(branchId ? { branchId } : {}),
        ...(type ? { metadata: { path: ['type'], equals: type } } : {}),
      },
      orderBy: { createdAt: 'desc' },
      take: take + 1,
      ...(cursor ? { cursor: { id: cursor }, skip: 1 } : {}),
    });

    const hasNextPage = departments.length > take;
    const trimmed = hasNextPage ? departments.slice(0, take) : departments;
    return { data: trimmed, pageInfo: { endCursor: trimmed.at(-1)?.id ?? null, hasNextPage } };
  }

  @Get(':id')
  @RequirePermission(manageDepartmentPermission())
  async getById(
    @Param('id') id: string,
    @CurrentOrganizationContext() organization: OrganizationContext,
    @CurrentUser() _user: CurrentUserContext,
  ) {
    return this.departmentService.getDepartmentById(id, organization.tenantId!);
  }

  @Post()
  @RequirePermission(manageDepartmentPermission())
  async create(
    @Body() body: CreateDepartmentDto,
    @CurrentOrganizationContext() organization: OrganizationContext,
    @CurrentUser() _user: CurrentUserContext,
  ) {
    return this.departmentService.createDepartment(organization.tenantId!, body);
  }

  @Patch(':id')
  @RequirePermission(manageDepartmentPermission())
  async update(
    @Param('id') id: string,
    @Body() body: UpdateDepartmentDto,
    @CurrentOrganizationContext() organization: OrganizationContext,
    @CurrentUser() _user: CurrentUserContext,
  ) {
    return this.departmentService.updateDepartment(id, organization.tenantId!, body);
  }

  @Delete(':id')
  @RequirePermission(manageDepartmentPermission())
  async delete(
    @Param('id') id: string,
    @CurrentOrganizationContext() organization: OrganizationContext,
    @CurrentUser() _user: CurrentUserContext,
  ) {
    await this.departmentService.deleteDepartment(id, organization.tenantId!);
    return { success: true };
  }

  @Post(':id/manager')
  @RequirePermission(manageDepartmentPermission())
  async assignManager(
    @Param('id') id: string,
    @Body() body: Omit<AssignManagerDto, 'departmentId'>,
    @CurrentOrganizationContext() organization: OrganizationContext,
    @CurrentUser() _user: CurrentUserContext,
  ) {
    return this.departmentService.assignManager(organization.tenantId!, { ...body, departmentId: id });
  }
}
