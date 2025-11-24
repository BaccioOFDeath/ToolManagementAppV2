import { IsArray, IsBoolean, IsDateString, IsOptional, IsString, IsUUID } from 'class-validator';

export class AssignUserDto {
  @IsUUID()
  userId!: string;

  @IsUUID()
  departmentRoleId!: string;

  @IsUUID()
  departmentId!: string;

  @IsUUID()
  tenantId!: string;

  @IsBoolean()
  @IsOptional()
  isPrimary?: boolean;
}

export class TransferUserDto {
  @IsUUID()
  userId!: string;

  @IsUUID()
  fromDepartmentRoleId!: string;

  @IsUUID()
  toDepartmentRoleId!: string;

  @IsUUID()
  fromDepartmentId!: string;

  @IsUUID()
  toDepartmentId!: string;

  @IsUUID()
  tenantId!: string;
}

export class TerminateAssignmentDto {
  @IsUUID()
  assignmentId!: string;

  @IsUUID()
  tenantId!: string;

  @IsDateString()
  @IsOptional()
  removedAt?: string;
}

export class ListAssignmentsDto {
  @IsUUID()
  tenantId!: string;

  @IsUUID()
  @IsOptional()
  userId?: string;

  @IsUUID()
  @IsOptional()
  departmentId?: string;
}

export class StaffLookupDto {
  @IsUUID()
  tenantId!: string;

  @IsArray()
  @IsOptional()
  departmentIds?: string[];

  @IsUUID()
  @IsOptional()
  branchId?: string;
}
