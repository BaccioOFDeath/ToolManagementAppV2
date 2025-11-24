import { CanActivate, ExecutionContext, Injectable } from '@nestjs/common';
import { Reflector } from '@nestjs/core';
import { AccessControlService } from './services/access-control.service';
import { ForbiddenError } from '../../common/exceptions';
import { OrganizationContext, resolveOrganizationContext } from './organization-context';
import { PermissionRequirement, resolvePermissionRequirements } from './permissions';
import { ORGANIZATION_PERMISSION_METADATA_KEY } from './require-permission.decorator';

@Injectable()
export class OrganizationAccessGuard implements CanActivate {
  constructor(private readonly reflector: Reflector, private readonly accessControlService: AccessControlService) {}

  private log(event: string, detail: Record<string, unknown>) {
    const payload = { event, at: new Date().toISOString(), ...detail };
    // Replace with structured logger when available
    console.info('[org-access]', JSON.stringify(payload));
  }

  private assertContext(context: OrganizationContext) {
    if (!context.userId || !context.tenantId) {
      this.log('context.missing', { context });
      throw new ForbiddenError('Missing user or tenant information for permission validation.');
    }
  }

  private getPermissions(context: ExecutionContext): PermissionRequirement[] {
    return (
      this.reflector.getAllAndOverride<PermissionRequirement[]>(ORGANIZATION_PERMISSION_METADATA_KEY, [
        context.getHandler(),
        context.getClass(),
      ]) ?? []
    );
  }

  async canActivate(context: ExecutionContext): Promise<boolean> {
    const requiredPermissions = this.getPermissions(context);

    if (!requiredPermissions.length) {
      this.log('permission.skipped', { reason: 'no requirements', handler: context.getHandler()?.name });
      return true;
    }

    const organizationContext = resolveOrganizationContext(context);
    this.assertContext(organizationContext);

    const permissions = resolvePermissionRequirements(requiredPermissions, organizationContext);
    this.log('permission.check.start', {
      permissions,
      requestType: organizationContext.requestType,
      handler: context.getHandler()?.name,
      userId: organizationContext.userId,
      tenantId: organizationContext.tenantId,
      branchId: organizationContext.branchId,
      departmentId: organizationContext.departmentId,
    });

    try {
      for (const permission of permissions) {
        await this.accessControlService.ensurePermission(
          organizationContext.tenantId!,
          organizationContext.userId!,
          permission,
        );
        this.log('permission.check.success', { permission });
      }
      return true;
    } catch (error) {
      this.log('permission.check.failure', {
        error: error instanceof Error ? error.message : String(error),
        permissions,
        userId: organizationContext.userId,
        tenantId: organizationContext.tenantId,
      });
      throw error;
    }
  }
}
