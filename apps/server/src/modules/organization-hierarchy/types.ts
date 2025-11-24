import { Field, GraphQLISODateTime, ID, Int, ObjectType } from '@nestjs/graphql';

@ObjectType()
export class PageInfo {
  @Field({ nullable: true })
  endCursor?: string | null;

  @Field()
  hasNextPage!: boolean;
}

@ObjectType()
export class Department {
  @Field(() => ID)
  id!: string;

  @Field()
  tenantId!: string;

  @Field({ nullable: true })
  branchId?: string | null;

  @Field()
  code!: string;

  @Field()
  name!: string;

  @Field({ nullable: true })
  parentDepartmentId?: string | null;

  @Field({ nullable: true })
  metadata?: Record<string, unknown> | null;

  @Field(() => GraphQLISODateTime)
  createdAt!: Date;

  @Field(() => GraphQLISODateTime)
  updatedAt!: Date;

  @Field(() => [DepartmentRole], { nullable: true })
  roles?: DepartmentRole[];

  @Field(() => [Department], { nullable: true })
  children?: Department[];

  @Field(() => [UserDepartmentAssignment], { nullable: true })
  assignments?: UserDepartmentAssignment[];

  @Field(() => Int, { nullable: true })
  staffCount?: number;

  @Field(() => UserDepartmentAssignment, { nullable: true })
  primaryAssignment?: UserDepartmentAssignment | null;
}

@ObjectType()
export class RoleDefinition {
  @Field(() => ID)
  id!: string;

  @Field()
  tenantId!: string;

  @Field()
  key!: string;

  @Field()
  displayName!: string;

  @Field({ nullable: true })
  description?: string | null;

  @Field()
  scope!: string;

  @Field(() => [String])
  permissions!: string[];

  @Field(() => [String])
  inheritsFromIds!: string[];

  @Field()
  isSystem!: boolean;

  @Field(() => GraphQLISODateTime)
  createdAt!: Date;

  @Field(() => GraphQLISODateTime)
  updatedAt!: Date;

  @Field(() => [DepartmentRole], { nullable: true })
  departmentRoles?: DepartmentRole[];
}

@ObjectType()
export class DepartmentRole {
  @Field(() => ID)
  id!: string;

  @Field()
  tenantId!: string;

  @Field()
  departmentId!: string;

  @Field()
  roleDefinitionId!: string;

  @Field()
  isDefault!: boolean;

  @Field(() => GraphQLISODateTime)
  createdAt!: Date;

  @Field(() => GraphQLISODateTime)
  updatedAt!: Date;

  @Field(() => Department, { nullable: true })
  department?: Department;

  @Field(() => RoleDefinition, { nullable: true })
  definition?: RoleDefinition;

  @Field(() => [UserDepartmentAssignment], { nullable: true })
  assignments?: UserDepartmentAssignment[];
}

@ObjectType()
export class UserDepartmentAssignment {
  @Field(() => ID)
  id!: string;

  @Field()
  tenantId!: string;

  @Field()
  userId!: string;

  @Field()
  departmentRoleId!: string;

  @Field()
  departmentId!: string;

  @Field()
  isPrimary!: boolean;

  @Field(() => GraphQLISODateTime)
  assignedAt!: Date;

  @Field(() => GraphQLISODateTime, { nullable: true })
  removedAt?: Date | null;

  @Field(() => DepartmentRole, { nullable: true })
  departmentRole?: DepartmentRole;

  @Field(() => Department, { nullable: true })
  department?: Department;
}

@ObjectType()
export class DepartmentEdge {
  @Field(() => String)
  cursor!: string;

  @Field(() => Department)
  node!: Department;
}

@ObjectType()
export class DepartmentConnection {
  @Field(() => [DepartmentEdge])
  edges!: DepartmentEdge[];

  @Field(() => PageInfo)
  pageInfo!: PageInfo;
}

@ObjectType()
export class AssignmentEdge {
  @Field(() => String)
  cursor!: string;

  @Field(() => UserDepartmentAssignment)
  node!: UserDepartmentAssignment;
}

@ObjectType()
export class AssignmentConnection {
  @Field(() => [AssignmentEdge])
  edges!: AssignmentEdge[];

  @Field(() => PageInfo)
  pageInfo!: PageInfo;
}

@ObjectType()
export class RoleDefinitionEdge {
  @Field(() => String)
  cursor!: string;

  @Field(() => RoleDefinition)
  node!: RoleDefinition;
}

@ObjectType()
export class RoleDefinitionConnection {
  @Field(() => [RoleDefinitionEdge])
  edges!: RoleDefinitionEdge[];

  @Field(() => PageInfo)
  pageInfo!: PageInfo;
}
