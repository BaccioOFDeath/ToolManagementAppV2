import { IsArray, IsBoolean, IsOptional, IsString, IsUUID } from 'class-validator';

export class CreateRoleDefinitionDto {
  @IsString()
  key!: string;

  @IsString()
  displayName!: string;

  @IsString()
  scope!: string;

  @IsArray()
  permissions!: string[];

  @IsArray()
  @IsOptional()
  inheritsFromIds?: string[];

  @IsBoolean()
  @IsOptional()
  isSystem?: boolean;

  @IsString()
  @IsOptional()
  description?: string;
}

export class UpdateRoleDefinitionDto {
  @IsString()
  @IsOptional()
  displayName?: string;

  @IsString()
  @IsOptional()
  scope?: string;

  @IsArray()
  @IsOptional()
  permissions?: string[];

  @IsArray()
  @IsOptional()
  inheritsFromIds?: string[];

  @IsBoolean()
  @IsOptional()
  isSystem?: boolean;

  @IsString()
  @IsOptional()
  description?: string;
}

export class ResolvePermissionsDto {
  @IsUUID()
  tenantId!: string;

  @IsUUID()
  roleDefinitionId!: string;
}
