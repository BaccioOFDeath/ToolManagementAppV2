import { ApolloClient, NormalizedCacheObject, gql } from '@apollo/client';

export const CURRENT_USER_QUERY = gql`
  query CurrentUser {
    currentUser {
      id
      email
      displayName
      tenantId
      roles
      branchId
    }
  }
`;

export const BRANCHES_QUERY = gql`
  query Branches($cursor: String, $limit: Int) {
    branches(cursor: $cursor, limit: $limit) {
      edges {
        cursor
        node {
          id
          code
          name
          metadata
        }
      }
      pageInfo {
        endCursor
        hasNextPage
      }
    }
  }
`;

export const DEPARTMENTS_QUERY = gql`
  query Departments($branchId: String, $type: String, $cursor: String, $limit: Int) {
    departments(branchId: $branchId, type: $type, cursor: $cursor, limit: $limit) {
      edges {
        cursor
        node {
          id
          code
          name
          branchId
          parentDepartmentId
          metadata
          staffCount
          primaryAssignment {
            id
            userId
            departmentRoleId
            assignedAt
          }
        }
      }
      pageInfo {
        endCursor
        hasNextPage
      }
    }
  }
`;

export const DEPARTMENT_QUERY = gql`
  query Department($id: String!) {
    department(id: $id) {
      id
      code
      name
      branchId
      parentDepartmentId
      metadata
      staffCount
      assignments {
        id
        userId
        departmentRoleId
        isPrimary
        assignedAt
      }
      roles {
        id
        roleDefinitionId
        isDefault
      }
    }
  }
`;

export const ROLE_DEFINITIONS_QUERY = gql`
  query RoleDefinitions($cursor: String, $limit: Int) {
    roleDefinitions(cursor: $cursor, limit: $limit) {
      edges {
        cursor
        node {
          id
          key
          displayName
          description
          scope
          permissions
          inheritsFromIds
          isSystem
        }
      }
      pageInfo {
        endCursor
        hasNextPage
      }
    }
  }
`;

export const RESOLVE_ROLE_PERMISSIONS_QUERY = gql`
  query ResolveRolePermissions($roleDefinitionId: String!) {
    resolveRolePermissions(roleDefinitionId: $roleDefinitionId)
  }
`;

export const ASSIGNMENTS_QUERY = gql`
  query Assignments($userId: String, $departmentId: String, $cursor: String, $limit: Int) {
    assignments(userId: $userId, departmentId: $departmentId, cursor: $cursor, limit: $limit) {
      edges {
        cursor
        node {
          id
          tenantId
          userId
          departmentId
          departmentRoleId
          isPrimary
          assignedAt
          removedAt
          department {
            id
            name
            branchId
          }
          departmentRole {
            id
            definition {
              id
              displayName
              permissions
              inheritsFromIds
            }
          }
        }
      }
      pageInfo {
        endCursor
        hasNextPage
      }
    }
  }
`;

export const CREATE_DEPARTMENT_MUTATION = gql`
  mutation CreateDepartment($input: CreateDepartmentDto!) {
    createDepartment(input: $input) {
      id
      code
      name
      branchId
      metadata
    }
  }
`;

export const UPDATE_DEPARTMENT_MUTATION = gql`
  mutation UpdateDepartment($id: String!, $input: UpdateDepartmentDto!) {
    updateDepartment(id: $id, input: $input) {
      id
      code
      name
      branchId
      metadata
    }
  }
`;

export const ASSIGN_USER_MUTATION = gql`
  mutation AssignUser($input: AssignUserDto!) {
    assignUser(input: $input) {
      id
      userId
      departmentId
      departmentRoleId
      isPrimary
      assignedAt
    }
  }
`;

export const TRANSFER_ASSIGNMENT_MUTATION = gql`
  mutation TransferAssignment($input: TransferUserDto!) {
    transferAssignment(input: $input) {
      id
      userId
      departmentId
      departmentRoleId
      isPrimary
      assignedAt
    }
  }
`;

export const TERMINATE_ASSIGNMENT_MUTATION = gql`
  mutation TerminateAssignment($input: TerminateAssignmentDto!) {
    terminateAssignment(input: $input) {
      id
      removedAt
    }
  }
`;

export const CREATE_AUDIT_LOG_MUTATION = gql`
  mutation CreateAuditEvent($action: String!, $entityId: String!, $details: JSON) {
    createAuditEvent(action: $action, entityId: $entityId, details: $details) {
      id
    }
  }
`;

export class RolePermissionLoader {
  private cache = new Map<string, Promise<string[]>>();

  constructor(private client: ApolloClient<NormalizedCacheObject>) {}

  load(roleDefinitionId: string): Promise<string[]> {
    if (!this.cache.has(roleDefinitionId)) {
      const promise = this.client
        .query<{ resolveRolePermissions: string[] }>({
          query: RESOLVE_ROLE_PERMISSIONS_QUERY,
          variables: { roleDefinitionId },
          fetchPolicy: 'network-only',
        })
        .then((result) => result.data.resolveRolePermissions);
      this.cache.set(roleDefinitionId, promise);
    }

    return this.cache.get(roleDefinitionId)!;
  }

  async loadMany(roleDefinitionIds: string[]): Promise<Record<string, string[]>> {
    const entries = await Promise.all(
      roleDefinitionIds.map(async (id) => [id, await this.load(id)] as const),
    );
    return Object.fromEntries(entries);
  }
}

export type BranchNode = {
  id: string;
  code: string;
  name: string;
  metadata?: Record<string, unknown> | null;
};

export type DepartmentNode = {
  id: string;
  code: string;
  name: string;
  branchId?: string | null;
  parentDepartmentId?: string | null;
  metadata?: Record<string, unknown> | null;
  staffCount?: number | null;
  primaryAssignment?: {
    id: string;
    userId: string;
    departmentRoleId: string;
    assignedAt: string;
  } | null;
};

export type RoleDefinitionNode = {
  id: string;
  key: string;
  displayName: string;
  description?: string | null;
  scope: string;
  permissions: string[];
  inheritsFromIds: string[];
  isSystem: boolean;
};

export type AssignmentNode = {
  id: string;
  tenantId: string;
  userId: string;
  departmentId: string;
  departmentRoleId: string;
  isPrimary: boolean;
  assignedAt: string;
  removedAt?: string | null;
  department?: { id: string; name: string; branchId?: string | null } | null;
  departmentRole?: { id: string; definition?: RoleDefinitionNode | null } | null;
};
