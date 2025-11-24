import { Injectable } from '@nestjs/common';
import { Prisma, PrismaClient } from '@prisma/client';
import { AssignManagerDto, CreateDepartmentDto, DepartmentFilterDto, UpdateDepartmentDto } from '../dto/department.dto';
import { ConflictError, NotFoundError, ValidationError } from '../../../common/exceptions';

@Injectable()
export class DepartmentService {
  constructor(private readonly prisma: PrismaClient) {}

  private ensureTenantMatch<T extends { tenantId: string }>(entity: T | null, tenantId: string, entityName: string) {
    if (!entity) {
      throw new NotFoundError(`${entityName} not found for tenant.`);
    }

    if (entity.tenantId !== tenantId) {
      throw new ValidationError(`${entityName} does not belong to tenant.`);
    }
  }

  async createDepartment(tenantId: string, dto: CreateDepartmentDto) {
    const existing = await this.prisma.department.findFirst({
      where: { tenantId, code: dto.code },
    });
    if (existing) {
      throw new ConflictError('Department code already exists for tenant.');
    }

    if (dto.parentDepartmentId) {
      const parent = await this.prisma.department.findUnique({ where: { id: dto.parentDepartmentId } });
      this.ensureTenantMatch(parent, tenantId, 'Parent department');
    }

    if (dto.branchId) {
      const branch = await this.prisma.branch.findUnique({ where: { id: dto.branchId } });
      this.ensureTenantMatch(branch, tenantId, 'Branch');
    }

    const metadata = {
      ...(dto.metadata ?? {}),
      ...(dto.type ? { type: dto.type } : {}),
    };

    return this.prisma.department.create({
      data: {
        tenantId,
        code: dto.code,
        name: dto.name,
        branchId: dto.branchId,
        parentDepartmentId: dto.parentDepartmentId,
        metadata,
      },
    });
  }

  async getDepartmentById(id: string, tenantId: string) {
    const department = await this.prisma.department.findUnique({
      where: { id },
      include: { branch: true, children: true },
    });
    this.ensureTenantMatch(department, tenantId, 'Department');
    return department;
  }

  async listDepartments(filter: DepartmentFilterDto) {
    const where: Prisma.DepartmentWhereInput = {
      tenantId: filter.tenantId,
      ...(filter.branchId ? { branchId: filter.branchId } : {}),
      ...(filter.type
        ? {
            metadata: {
              path: ['type'],
              equals: filter.type,
            },
          }
        : {}),
    };

    return this.prisma.department.findMany({
      where,
      include: filter.includeChildren ? { children: true } : undefined,
      orderBy: { createdAt: 'desc' },
    });
  }

  async updateDepartment(id: string, tenantId: string, dto: UpdateDepartmentDto) {
    const department = await this.prisma.department.findUnique({ where: { id } });
    this.ensureTenantMatch(department, tenantId, 'Department');

    if (dto.code && dto.code !== department!.code) {
      const conflict = await this.prisma.department.findFirst({
        where: { tenantId, code: dto.code, NOT: { id } },
      });
      if (conflict) {
        throw new ConflictError('Department code already exists for tenant.');
      }
    }

    if (dto.branchId) {
      const branch = await this.prisma.branch.findUnique({ where: { id: dto.branchId } });
      this.ensureTenantMatch(branch, tenantId, 'Branch');
    }

    if (dto.parentDepartmentId) {
      const parent = await this.prisma.department.findUnique({ where: { id: dto.parentDepartmentId } });
      this.ensureTenantMatch(parent, tenantId, 'Parent department');
    }

    const metadata = {
      ...(department!.metadata as Record<string, unknown>),
      ...(dto.metadata ?? {}),
      ...(dto.type ? { type: dto.type } : {}),
    };

    return this.prisma.department.update({
      where: { id },
      data: {
        ...dto,
        metadata,
      },
    });
  }

  async deleteDepartment(id: string, tenantId: string) {
    const department = await this.prisma.department.findUnique({
      where: { id },
      include: { children: true, assignments: true },
    });
    this.ensureTenantMatch(department, tenantId, 'Department');

    if (department!.children.length) {
      throw new ConflictError('Cannot delete department with child departments.');
    }

    if (department!.assignments.some((assignment) => !assignment.removedAt)) {
      throw new ConflictError('Cannot delete department with active assignments.');
    }

    await this.prisma.departmentRole.deleteMany({ where: { departmentId: id } });
    return this.prisma.department.delete({ where: { id } });
  }

  async assignManager(tenantId: string, dto: AssignManagerDto) {
    const department = await this.prisma.department.findUnique({ where: { id: dto.departmentId } });
    this.ensureTenantMatch(department, tenantId, 'Department');

    const updatedMetadata = {
      ...(department!.metadata as Record<string, unknown>),
      managerUserId: dto.managerUserId,
    };

    return this.prisma.department.update({
      where: { id: dto.departmentId },
      data: { metadata: updatedMetadata },
    });
  }
}
