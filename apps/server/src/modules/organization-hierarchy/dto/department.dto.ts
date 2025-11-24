import { IsBoolean, IsEnum, IsObject, IsOptional, IsString, IsUUID } from 'class-validator';

export enum DepartmentType {
  OPERATIONS = 'operations',
  SALES = 'sales',
  SERVICE = 'service',
  ADMIN = 'admin',
}

export class CreateDepartmentDto {
  @IsString()
  code!: string;

  @IsString()
  name!: string;

  @IsUUID()
  @IsOptional()
  branchId?: string;

  @IsUUID()
  @IsOptional()
  parentDepartmentId?: string;

  @IsEnum(DepartmentType)
  @IsOptional()
  type?: DepartmentType;

  @IsObject()
  @IsOptional()
  metadata?: Record<string, unknown>;
}

export class UpdateDepartmentDto {
  @IsString()
  @IsOptional()
  code?: string;

  @IsString()
  @IsOptional()
  name?: string;

  @IsUUID()
  @IsOptional()
  branchId?: string;

  @IsUUID()
  @IsOptional()
  parentDepartmentId?: string;

  @IsEnum(DepartmentType)
  @IsOptional()
  type?: DepartmentType;

  @IsObject()
  @IsOptional()
  metadata?: Record<string, unknown>;
}

export class AssignManagerDto {
  @IsUUID()
  departmentId!: string;

  @IsUUID()
  managerUserId!: string;
}

export class DepartmentFilterDto {
  @IsUUID()
  tenantId!: string;

  @IsUUID()
  @IsOptional()
  branchId?: string;

  @IsEnum(DepartmentType)
  @IsOptional()
  type?: DepartmentType;

  @IsBoolean()
  @IsOptional()
  includeChildren?: boolean;
}
