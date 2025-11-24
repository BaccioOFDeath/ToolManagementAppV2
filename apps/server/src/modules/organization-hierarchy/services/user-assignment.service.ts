import { Injectable } from '@nestjs/common';
import { PrismaClient } from '@prisma/client';
import { AccessControlService } from './access-control.service';
import { AssignUserDto, ListAssignmentsDto, StaffLookupDto, TerminateAssignmentDto, TransferUserDto } from '../dto/user-assignment.dto';
import { ConflictError, NotFoundError, ValidationError } from '../../../common/exceptions';

@Injectable()
export class UserAssignmentService {
  constructor(private readonly prisma: PrismaClient, private readonly accessControl: AccessControlService) {}

  private logAudit(action: string, detail: Record<string, unknown>) {
    const payload = { action, ...detail, at: new Date().toISOString() };
    // Replace with proper logger integration when available
    console.info('[audit]', JSON.stringify(payload));
  }

  private async assertDepartmentRole(tenantId: string, departmentRoleId: string) {
    const departmentRole = await this.prisma.departmentRole.findUnique({
      where: { id: departmentRoleId },
      include: { department: true },
    });

    if (!departmentRole) {
      throw new NotFoundError('Department role not found.');
    }

    if (departmentRole.tenantId !== tenantId || departmentRole.department.tenantId !== tenantId) {
      throw new ValidationError('Department role does not belong to tenant.');
    }

    return departmentRole;
  }

  private async assertAssignmentExists(assignmentId: string, tenantId: string) {
    const assignment = await this.prisma.userDepartmentAssignment.findUnique({
      where: { id: assignmentId },
      include: { departmentRole: true },
    });

    if (!assignment || assignment.tenantId !== tenantId) {
      throw new NotFoundError('Assignment not found for tenant.');
    }

    return assignment;
  }

  async assignUser(dto: AssignUserDto) {
    const departmentRole = await this.assertDepartmentRole(dto.tenantId, dto.departmentRoleId);

    if (departmentRole.departmentId !== dto.departmentId) {
      throw new ValidationError('Department role does not belong to the selected department.');
    }

    const existing = await this.prisma.userDepartmentAssignment.findFirst({
      where: {
        tenantId: dto.tenantId,
        userId: dto.userId,
        departmentRoleId: dto.departmentRoleId,
        removedAt: null,
      },
    });

    if (existing) {
      throw new ConflictError('User already has this active assignment.');
    }

    if (dto.isPrimary) {
      const primary = await this.prisma.userDepartmentAssignment.findFirst({
        where: { tenantId: dto.tenantId, userId: dto.userId, isPrimary: true, removedAt: null },
      });
      if (primary) {
        throw new ConflictError('User already has a primary assignment.');
      }
    }

    const assignment = await this.prisma.userDepartmentAssignment.create({
      data: {
        tenantId: dto.tenantId,
        userId: dto.userId,
        departmentRoleId: dto.departmentRoleId,
        departmentId: dto.departmentId,
        isPrimary: dto.isPrimary ?? false,
      },
    });

    this.logAudit('assignment.created', { assignmentId: assignment.id, userId: dto.userId, tenantId: dto.tenantId });
    await this.accessControl.invalidateUserPermissionsCache(dto.tenantId, dto.userId);
    return assignment;
  }

  async transfer(dto: TransferUserDto) {
    const fromRole = await this.assertDepartmentRole(dto.tenantId, dto.fromDepartmentRoleId);
    const toRole = await this.assertDepartmentRole(dto.tenantId, dto.toDepartmentRoleId);

    if (fromRole.departmentId !== dto.fromDepartmentId || toRole.departmentId !== dto.toDepartmentId) {
      throw new ValidationError('Department role mismatch during transfer.');
    }

    return this.prisma.$transaction(async (tx) => {
      const activeAssignment = await tx.userDepartmentAssignment.findFirst({
        where: {
          tenantId: dto.tenantId,
          userId: dto.userId,
          departmentRoleId: dto.fromDepartmentRoleId,
          departmentId: dto.fromDepartmentId,
          removedAt: null,
        },
      });

      if (!activeAssignment) {
        throw new NotFoundError('Active assignment to transfer was not found.');
      }

      await tx.userDepartmentAssignment.update({
        where: { id: activeAssignment.id },
        data: { removedAt: new Date() },
      });

      const newAssignment = await tx.userDepartmentAssignment.create({
        data: {
          tenantId: dto.tenantId,
          userId: dto.userId,
          departmentRoleId: dto.toDepartmentRoleId,
          departmentId: dto.toDepartmentId,
          isPrimary: activeAssignment.isPrimary,
        },
      });

      this.logAudit('assignment.transferred', {
        userId: dto.userId,
        fromAssignmentId: activeAssignment.id,
        toAssignmentId: newAssignment.id,
        tenantId: dto.tenantId,
      });

      await this.accessControl.invalidateUserPermissionsCache(dto.tenantId, dto.userId);
      return newAssignment;
    });
  }

  async listAssignments(dto: ListAssignmentsDto) {
    return this.prisma.userDepartmentAssignment.findMany({
      where: {
        tenantId: dto.tenantId,
        ...(dto.userId ? { userId: dto.userId } : {}),
        ...(dto.departmentId ? { departmentId: dto.departmentId } : {}),
      },
      include: {
        departmentRole: { include: { definition: true } },
        department: true,
      },
      orderBy: { assignedAt: 'desc' },
    });
  }

  async lookupStaff(dto: StaffLookupDto) {
    const assignments = await this.prisma.userDepartmentAssignment.findMany({
      where: {
        tenantId: dto.tenantId,
        removedAt: null,
        ...(dto.departmentIds?.length ? { departmentId: { in: dto.departmentIds } } : {}),
        ...(dto.branchId ? { department: { branchId: dto.branchId } } : {}),
      },
      include: { department: true, departmentRole: true },
    });

    return assignments.map((assignment) => ({
      userId: assignment.userId,
      departmentId: assignment.departmentId,
      departmentRoleId: assignment.departmentRoleId,
      branchId: assignment.department.branchId,
      isPrimary: assignment.isPrimary,
    }));
  }

  async terminateAssignment(dto: TerminateAssignmentDto) {
    const assignment = await this.assertAssignmentExists(dto.assignmentId, dto.tenantId);
    if (assignment.removedAt) {
      throw new ConflictError('Assignment already terminated.');
    }

    const removedAt = dto.removedAt ? new Date(dto.removedAt) : new Date();

    const terminated = await this.prisma.userDepartmentAssignment.update({
      where: { id: dto.assignmentId },
      data: { removedAt },
    });

    this.logAudit('assignment.terminated', { assignmentId: dto.assignmentId, tenantId: dto.tenantId });
    await this.accessControl.invalidateUserPermissionsCache(dto.tenantId, terminated.userId);
    return terminated;
  }
}
