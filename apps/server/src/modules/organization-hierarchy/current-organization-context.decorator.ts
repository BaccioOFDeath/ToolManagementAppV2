import { createParamDecorator, ExecutionContext } from '@nestjs/common';
import { OrganizationContext, resolveOrganizationContext } from './organization-context';

export const CurrentOrganizationContext = createParamDecorator(
  (_data: unknown, context: ExecutionContext): OrganizationContext => resolveOrganizationContext(context),
);
