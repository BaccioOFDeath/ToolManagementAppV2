import { createParamDecorator, ExecutionContext } from '@nestjs/common';
import { GqlExecutionContext } from '@nestjs/graphql';

export interface CurrentUserContext {
  id?: string;
  tenantId?: string;
  branchId?: string;
  departmentId?: string;
  [key: string]: unknown;
}

function extractUser(request: any): CurrentUserContext {
  const user = request?.user ?? {};
  return {
    ...user,
    id: user.id ?? user.userId ?? request?.userId ?? request?.headers?.['x-user-id'],
    tenantId: user.tenantId ?? request?.tenantId ?? request?.headers?.['x-tenant-id'],
    branchId: user.branchId ?? request?.branchId ?? request?.headers?.['x-branch-id'],
    departmentId: user.departmentId ?? request?.departmentId ?? request?.headers?.['x-department-id'],
  };
}

export const CurrentUser = createParamDecorator((_data: unknown, ctx: ExecutionContext) => {
  if (ctx.getType<string>() === 'graphql') {
    const gqlContext = GqlExecutionContext.create(ctx).getContext();
    const request = gqlContext?.req ?? gqlContext?.request ?? gqlContext;
    return extractUser(request);
  }

  const request = ctx.switchToHttp().getRequest?.() ?? {};
  return extractUser(request);
});
