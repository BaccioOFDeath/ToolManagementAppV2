import React, { useEffect, useMemo, useState } from 'react';
import { useApolloClient, useMutation, useQuery } from '@apollo/client';
import {
  ASSIGNMENTS_QUERY,
  ASSIGN_USER_MUTATION,
  BRANCHES_QUERY,
  CREATE_DEPARTMENT_MUTATION,
  CURRENT_USER_QUERY,
  DEPARTMENTS_QUERY,
  ROLE_DEFINITIONS_QUERY,
  TERMINATE_ASSIGNMENT_MUTATION,
  TRANSFER_ASSIGNMENT_MUTATION,
  UPDATE_DEPARTMENT_MUTATION,
  AssignmentNode,
  BranchNode,
  DepartmentNode,
  RoleDefinitionNode,
  RolePermissionLoader,
} from '../graphql/organization';
import { DepartmentDetailModal } from '../components/DepartmentDetailModal';
import { UserAssignmentModal } from '../components/UserAssignmentModal';
import { RolePermissionViewer } from '../components/RolePermissionViewer';
import { useAuditLogger } from '../hooks/useAuditLogger';

const ACCESS_ROLES = new Set(['tenant_admin', 'branch_manager']);

type TabKey = 'branches' | 'departments' | 'roles' | 'assignments';

const breadcrumb = (items: string[]) => items.filter(Boolean).join(' / ');

export const OrganizationPage: React.FC = () => {
  const client = useApolloClient();
  const { logAction } = useAuditLogger();
  const [activeTab, setActiveTab] = useState<TabKey>('branches');
  const [branchFilter, setBranchFilter] = useState<string | null>(null);
  const [departmentSearch, setDepartmentSearch] = useState('');
  const [assignmentModalMode, setAssignmentModalMode] = useState<'assign' | 'transfer' | 'terminate' | null>(null);
  const [selectedDepartment, setSelectedDepartment] = useState<DepartmentNode | undefined>(undefined);
  const [assignmentDepartmentFilter, setAssignmentDepartmentFilter] = useState<string | null>(null);
  const [permissionMap, setPermissionMap] = useState<Record<string, string[]>>({});
  const [optimisticNotice, setOptimisticNotice] = useState<string | null>(null);

  const { data: userData, loading: userLoading, error: userError } = useQuery(CURRENT_USER_QUERY);
  const allowed = useMemo(() => {
    const roles = userData?.currentUser?.roles ?? [];
    return roles.some((role: string) => ACCESS_ROLES.has(role));
  }, [userData]);

  const branchesQuery = useQuery(BRANCHES_QUERY, { variables: { limit: 20 }, skip: !allowed });
  const departmentsQuery = useQuery(DEPARTMENTS_QUERY, {
    variables: { branchId: branchFilter ?? undefined, limit: 25 },
    skip: !allowed,
  });
  const rolesQuery = useQuery(ROLE_DEFINITIONS_QUERY, { variables: { limit: 50 }, skip: !allowed });
  const assignmentsQuery = useQuery(ASSIGNMENTS_QUERY, {
    variables: { departmentId: assignmentDepartmentFilter ?? undefined, limit: 50 },
    skip: !allowed,
  });

  const [createDepartment] = useMutation(CREATE_DEPARTMENT_MUTATION);
  const [updateDepartment] = useMutation(UPDATE_DEPARTMENT_MUTATION);
  const [assignUser] = useMutation(ASSIGN_USER_MUTATION);
  const [transferAssignment] = useMutation(TRANSFER_ASSIGNMENT_MUTATION);
  const [terminateAssignment] = useMutation(TERMINATE_ASSIGNMENT_MUTATION);

  const permissionLoader = useMemo(() => new RolePermissionLoader(client), [client]);

  const branches: BranchNode[] = useMemo(
    () => branchesQuery.data?.branches?.edges?.map((edge: any) => edge.node) ?? [],
    [branchesQuery.data],
  );
  const departments: DepartmentNode[] = useMemo(() => {
    const nodes = departmentsQuery.data?.departments?.edges?.map((edge: any) => edge.node) ?? [];
    if (!departmentSearch.trim()) return nodes;
    const term = departmentSearch.toLowerCase();
    return nodes.filter((dept: DepartmentNode) =>
      [dept.name, dept.code, dept.metadata?.type as string].some((value) =>
        typeof value === 'string' ? value.toLowerCase().includes(term) : false,
      ),
    );
  }, [departmentsQuery.data, departmentSearch]);

  const roles: RoleDefinitionNode[] = useMemo(
    () => rolesQuery.data?.roleDefinitions?.edges?.map((edge: any) => edge.node) ?? [],
    [rolesQuery.data],
  );

  const assignments: AssignmentNode[] = useMemo(() => {
    const nodes: AssignmentNode[] = assignmentsQuery.data?.assignments?.edges?.map((edge: any) => edge.node) ?? [];
    if (!branchFilter) return nodes;
    return nodes.filter((assignment) => assignment.department?.branchId === branchFilter);
  }, [assignmentsQuery.data, branchFilter]);

  useEffect(() => {
    if (!roles.length) return;
    permissionLoader.loadMany(roles.map((role) => role.id)).then((map) => setPermissionMap(map));
  }, [roles, permissionLoader]);

  const breadcrumbs = breadcrumb(['Organization', branchFilter ? `Branch ${branchFilter}` : '', selectedDepartment?.name ?? '']);

  const handleDepartmentSave = async (input: {
    id?: string;
    code: string;
    name: string;
    branchId?: string | null;
    parentDepartmentId?: string | null;
    metadata?: Record<string, unknown>;
  }) => {
    setOptimisticNotice('Saving changes...');
    if (input.id) {
      await updateDepartment({
        variables: { id: input.id, input },
        optimisticResponse: {
          updateDepartment: {
            __typename: 'Department',
            ...input,
            id: input.id,
            tenantId: userData?.currentUser?.tenantId ?? 'tenant-optimistic',
            createdAt: new Date().toISOString(),
            updatedAt: new Date().toISOString(),
          },
        },
        refetchQueries: [{ query: DEPARTMENTS_QUERY, variables: { branchId: branchFilter ?? undefined, limit: 25 } }],
      });
      await logAction({ action: 'department.update', entityId: input.id, details: input });
    } else {
      const tempId = `temp-${Date.now()}`;
      await createDepartment({
        variables: { input },
        optimisticResponse: {
          createDepartment: {
            __typename: 'Department',
            id: tempId,
            tenantId: userData?.currentUser?.tenantId ?? 'tenant-optimistic',
            createdAt: new Date().toISOString(),
            updatedAt: new Date().toISOString(),
            ...input,
          },
        },
        update: (cache, { data }) => {
          const newDept = data?.createDepartment;
          if (!newDept) return;
          const existing = cache.readQuery<any>({
            query: DEPARTMENTS_QUERY,
            variables: { branchId: branchFilter ?? undefined, limit: 25 },
          });
          if (!existing?.departments) return;
          const edges = existing.departments.edges ?? [];
          cache.writeQuery({
            query: DEPARTMENTS_QUERY,
            variables: { branchId: branchFilter ?? undefined, limit: 25 },
            data: {
              departments: {
                ...existing.departments,
                edges: [{ cursor: newDept.id, node: newDept }, ...edges],
              },
            },
          });
        },
      });
      await logAction({ action: 'department.create', entityId: tempId, details: input });
    }
    setOptimisticNotice(null);
    setSelectedDepartment(undefined);
  };

  const handleAssignmentAction = async (
    payload:
      | { type: 'assign'; input: { userId: string; departmentId: string; departmentRoleId: string; isPrimary: boolean } }
      | { type: 'transfer'; input: { assignmentId: string; toDepartmentRoleId: string; newPrimary?: boolean } }
      | { type: 'terminate'; input: { assignmentId: string; reason?: string } },
  ) => {
    setOptimisticNotice('Updating assignment...');
    if (payload.type === 'assign') {
      await assignUser({
        variables: { input: payload.input },
        optimisticResponse: {
          assignUser: {
            __typename: 'UserDepartmentAssignment',
            id: `temp-${Date.now()}`,
            tenantId: userData?.currentUser?.tenantId ?? 'tenant-optimistic',
            removedAt: null,
            ...payload.input,
            assignedAt: new Date().toISOString(),
          },
        },
        refetchQueries: [{ query: ASSIGNMENTS_QUERY, variables: { departmentId: branchFilter ?? undefined, limit: 50 } }],
      });
      await logAction({ action: 'assignment.assign', entityId: payload.input.userId, details: payload.input });
    }

    if (payload.type === 'transfer') {
      await transferAssignment({
        variables: { input: payload.input },
        optimisticResponse: {
          transferAssignment: {
            __typename: 'UserDepartmentAssignment',
            id: payload.input.assignmentId,
            tenantId: userData?.currentUser?.tenantId ?? 'tenant-optimistic',
            removedAt: null,
            assignedAt: new Date().toISOString(),
            departmentId: '',
            departmentRoleId: payload.input.toDepartmentRoleId,
            isPrimary: Boolean(payload.input.newPrimary),
            userId: '',
          },
        },
        refetchQueries: [{ query: ASSIGNMENTS_QUERY, variables: { departmentId: branchFilter ?? undefined, limit: 50 } }],
      });
      await logAction({ action: 'assignment.transfer', entityId: payload.input.assignmentId, details: payload.input });
    }

    if (payload.type === 'terminate') {
      await terminateAssignment({
        variables: { input: payload.input },
        optimisticResponse: {
          terminateAssignment: {
            __typename: 'UserDepartmentAssignment',
            id: payload.input.assignmentId,
            removedAt: new Date().toISOString(),
          },
        },
        refetchQueries: [{ query: ASSIGNMENTS_QUERY, variables: { departmentId: branchFilter ?? undefined, limit: 50 } }],
      });
      await logAction({ action: 'assignment.terminate', entityId: payload.input.assignmentId, details: payload.input });
    }

    setAssignmentModalMode(null);
    setOptimisticNotice(null);
  };

  const loading = userLoading || branchesQuery.loading || departmentsQuery.loading || rolesQuery.loading || assignmentsQuery.loading;
  const errors = userError || branchesQuery.error || departmentsQuery.error || rolesQuery.error || assignmentsQuery.error;

  if (loading) {
    return <div className="p-6">Loading organization data...</div>;
  }

  if (!allowed) {
    return <div className="p-6 text-red-700">Access restricted to tenant admins and branch managers.</div>;
  }

  return (
    <div className="p-6 space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-semibold">Organization</h1>
          <p className="text-sm text-gray-600">{breadcrumbs || 'Organization hierarchy and assignments'}</p>
        </div>
        <div className="space-x-2">
          {optimisticNotice && <span className="text-xs text-amber-700">{optimisticNotice}</span>}
          {errors && <span className="text-xs text-red-600">{(errors as Error).message}</span>}
        </div>
      </div>

      <div className="flex space-x-3 border-b">
        {(
          [
            { key: 'branches', label: 'Branches' },
            { key: 'departments', label: 'Departments' },
            { key: 'roles', label: 'Roles' },
            { key: 'assignments', label: 'User Assignments' },
          ] as { key: TabKey; label: string }[]
        ).map((tab) => (
          <button
            key={tab.key}
            className={`pb-2 px-1 text-sm ${activeTab === tab.key ? 'border-b-2 border-blue-600 font-semibold' : 'text-gray-600'}`}
            onClick={() => setActiveTab(tab.key)}
          >
            {tab.label}
          </button>
        ))}
      </div>

      {activeTab === 'branches' && (
        <div className="space-y-3">
          <div className="flex items-center justify-between">
            <h2 className="text-lg font-semibold">Branches</h2>
            <button className="text-sm text-blue-600" onClick={() => branchesQuery.refetch()}>Refresh</button>
          </div>
          <table className="min-w-full text-sm border rounded">
            <thead className="bg-gray-50">
              <tr>
                <th className="text-left px-3 py-2">Code</th>
                <th className="text-left px-3 py-2">Name</th>
                <th className="text-left px-3 py-2">Metadata</th>
              </tr>
            </thead>
            <tbody>
              {branches.map((branch) => (
                <tr key={branch.id} className="border-t hover:bg-gray-50">
                  <td className="px-3 py-2 font-mono">{branch.code}</td>
                  <td className="px-3 py-2">{branch.name}</td>
                  <td className="px-3 py-2 text-xs text-gray-600">{branch.metadata ? JSON.stringify(branch.metadata) : '—'}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}

      {activeTab === 'departments' && (
        <div className="space-y-4">
          <div className="flex items-center justify-between">
            <div className="space-y-1">
              <h2 className="text-lg font-semibold">Departments</h2>
              <div className="flex items-center space-x-3">
                <label className="text-sm">
                  Branch context:
                  <select
                    className="ml-2 rounded border px-2 py-1 text-sm"
                    value={branchFilter ?? ''}
                    onChange={(e) => setBranchFilter(e.target.value || null)}
                  >
                    <option value="">All branches</option>
                    {branches.map((branch) => (
                      <option key={branch.id} value={branch.id}>
                        {branch.name}
                      </option>
                    ))}
                  </select>
                </label>
                <input
                  className="rounded border px-2 py-1 text-sm"
                  placeholder="Search departments"
                  value={departmentSearch}
                  onChange={(e) => setDepartmentSearch(e.target.value)}
                />
              </div>
            </div>
            <button
              className="px-3 py-2 rounded bg-blue-600 text-white text-sm"
              onClick={() => setSelectedDepartment({ code: '', name: '', id: '', metadata: {} })}
            >
              New Department
            </button>
          </div>
          <table className="min-w-full text-sm border rounded">
            <thead className="bg-gray-50">
              <tr>
                <th className="text-left px-3 py-2">Code</th>
                <th className="text-left px-3 py-2">Name</th>
                <th className="text-left px-3 py-2">Branch</th>
                <th className="text-left px-3 py-2">Staff</th>
                <th className="text-left px-3 py-2">Primary Assignment</th>
                <th className="text-left px-3 py-2">Actions</th>
              </tr>
            </thead>
            <tbody>
              {departments.map((dept) => (
                <tr key={dept.id || dept.code} className="border-t hover:bg-gray-50">
                  <td className="px-3 py-2 font-mono">{dept.code}</td>
                  <td className="px-3 py-2">{dept.name}</td>
                  <td className="px-3 py-2">{branches.find((b) => b.id === dept.branchId)?.name ?? '—'}</td>
                  <td className="px-3 py-2">{dept.staffCount ?? '—'}</td>
                  <td className="px-3 py-2 text-xs text-gray-700">
                    {dept.primaryAssignment ? `${dept.primaryAssignment.userId} (${dept.primaryAssignment.departmentRoleId})` : '—'}
                  </td>
                  <td className="px-3 py-2 space-x-2">
                    <button className="text-blue-600 text-sm" onClick={() => setSelectedDepartment(dept)}>
                      Edit
                    </button>
                    <button
                      className="text-sm text-green-600"
                      onClick={() => {
                        setAssignmentModalMode('assign');
                        setSelectedDepartment(dept);
                      }}
                    >
                      Assign User
                    </button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}

      {activeTab === 'roles' && (
        <div className="space-y-3">
          <div className="flex items-center justify-between">
            <h2 className="text-lg font-semibold">Roles</h2>
            <button className="text-sm text-blue-600" onClick={() => rolesQuery.refetch()}>
              Refresh
            </button>
          </div>
          <RolePermissionViewer roles={roles} permissionMap={permissionMap} />
        </div>
      )}

      {activeTab === 'assignments' && (
        <div className="space-y-3">
          <div className="flex items-center justify-between">
            <h2 className="text-lg font-semibold">User Assignments</h2>
            <div className="space-x-2 flex items-center">
              <label className="text-sm flex items-center space-x-2">
                <span>Department</span>
                <select
                  className="rounded border px-2 py-1 text-sm"
                  value={assignmentDepartmentFilter ?? ''}
                  onChange={(e) => setAssignmentDepartmentFilter(e.target.value || null)}
                >
                  <option value="">All</option>
                  {departments.map((dept) => (
                    <option key={dept.id} value={dept.id}>
                      {dept.name}
                    </option>
                  ))}
                </select>
              </label>
              <button className="px-3 py-2 text-sm rounded bg-blue-600 text-white" onClick={() => setAssignmentModalMode('assign')}>
                New Assignment
              </button>
              <button className="text-sm text-blue-600" onClick={() => assignmentsQuery.refetch()}>
                Refresh
              </button>
            </div>
          </div>
          <table className="min-w-full text-sm border rounded">
            <thead className="bg-gray-50">
              <tr>
                <th className="text-left px-3 py-2">User</th>
                <th className="text-left px-3 py-2">Department</th>
                <th className="text-left px-3 py-2">Role</th>
                <th className="text-left px-3 py-2">Primary</th>
                <th className="text-left px-3 py-2">Status</th>
                <th className="text-left px-3 py-2">Actions</th>
              </tr>
            </thead>
            <tbody>
              {assignments.map((assignment) => (
                <tr key={assignment.id} className="border-t hover:bg-gray-50">
                  <td className="px-3 py-2">{assignment.userId}</td>
                  <td className="px-3 py-2">{assignment.department?.name ?? assignment.departmentId}</td>
                  <td className="px-3 py-2">{assignment.departmentRole?.definition?.displayName ?? assignment.departmentRoleId}</td>
                  <td className="px-3 py-2">{assignment.isPrimary ? 'Yes' : 'No'}</td>
                  <td className="px-3 py-2">{assignment.removedAt ? 'Inactive' : 'Active'}</td>
                  <td className="px-3 py-2 space-x-2">
                    <button className="text-sm text-blue-600" onClick={() => setAssignmentModalMode('transfer')}>
                      Transfer
                    </button>
                    <button className="text-sm text-amber-700" onClick={() => setAssignmentModalMode('terminate')}>
                      Terminate
                    </button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}

      {(selectedDepartment || assignmentModalMode) && (
        <>
          <DepartmentDetailModal
            open={Boolean(selectedDepartment && activeTab === 'departments')}
            branches={branches.map((b) => ({ id: b.id, name: b.name, code: b.code }))}
            department={selectedDepartment && selectedDepartment.id ? selectedDepartment : undefined}
            onClose={() => setSelectedDepartment(undefined)}
            onSave={handleDepartmentSave}
          />
          <UserAssignmentModal
            open={Boolean(assignmentModalMode)}
            branches={branches.map((b) => ({ id: b.id, name: b.name }))}
            departments={departments}
            roles={roles}
            existingAssignments={assignments}
            mode={assignmentModalMode ?? 'assign'}
            onAssign={(input) => handleAssignmentAction({ type: 'assign', input })}
            onTransfer={(input) => handleAssignmentAction({ type: 'transfer', input })}
            onTerminate={(input) => handleAssignmentAction({ type: 'terminate', input })}
            onClose={() => setAssignmentModalMode(null)}
          />
        </>
      )}
    </div>
  );
};

export default OrganizationPage;
