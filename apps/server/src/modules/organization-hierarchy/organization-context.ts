import { ExecutionContext } from '@nestjs/common';
import { GqlExecutionContext } from '@nestjs/graphql';

export interface OrganizationContext {
  tenantId?: string;
  userId?: string;
  branchId?: string;
  departmentId?: string;
  requestType: string;
}

function normalizeRequestContext(request: any, requestType: string): OrganizationContext {
  const headers = request?.headers ?? {};
  const user = request?.user ?? {};

  return {
    requestType,
    userId: user.id ?? user.userId ?? request?.userId ?? headers['x-user-id'],
    tenantId: user.tenantId ?? request?.tenantId ?? headers['x-tenant-id'],
    branchId: user.branchId ?? request?.branchId ?? headers['x-branch-id'],
    departmentId: user.departmentId ?? request?.departmentId ?? headers['x-department-id'],
  };
}

export function resolveOrganizationContext(context: ExecutionContext): OrganizationContext {
  const type = context.getType<string>();

  if (type === 'graphql') {
    const gqlContext = GqlExecutionContext.create(context).getContext();
    const request = gqlContext?.req ?? gqlContext?.request ?? gqlContext;
    return normalizeRequestContext(request, 'graphql');
  }

  const request = context.switchToHttp().getRequest?.() ?? {};
  return normalizeRequestContext(request, type ?? 'unknown');
}
