import { SetMetadata } from '@nestjs/common';
import { PermissionRequirement } from './permissions';

export const ORGANIZATION_PERMISSION_METADATA_KEY = 'organization:required-permissions';

export const RequirePermission = (...permissions: PermissionRequirement[]) =>
  SetMetadata(ORGANIZATION_PERMISSION_METADATA_KEY, permissions);
