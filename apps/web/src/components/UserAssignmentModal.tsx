import React, { useEffect, useMemo, useState } from 'react';
import { AssignmentNode, DepartmentNode, RoleDefinitionNode } from '../graphql/organization';

type Props = {
  open: boolean;
  branches: { id: string; name: string }[];
  departments: DepartmentNode[];
  roles: RoleDefinitionNode[];
  existingAssignments: AssignmentNode[];
  mode: 'assign' | 'transfer' | 'terminate';
  onAssign: (input: { userId: string; departmentId: string; departmentRoleId: string; isPrimary: boolean }) => Promise<void>;
  onTransfer: (input: { assignmentId: string; toDepartmentRoleId: string; newPrimary?: boolean }) => Promise<void>;
  onTerminate: (input: { assignmentId: string; reason?: string }) => Promise<void>;
  onClose: () => void;
};

export const UserAssignmentModal: React.FC<Props> = ({
  open,
  branches,
  departments,
  roles,
  existingAssignments,
  mode,
  onAssign,
  onTransfer,
  onTerminate,
  onClose,
}) => {
  const [userId, setUserId] = useState('');
  const [departmentId, setDepartmentId] = useState('');
  const [departmentRoleId, setDepartmentRoleId] = useState('');
  const [assignmentId, setAssignmentId] = useState('');
  const [isPrimary, setIsPrimary] = useState(false);
  const [reason, setReason] = useState('');
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    setError(null);
  }, [mode, open]);

  const branchDepartmentMap = useMemo(() => {
    const grouped = new Map<string, DepartmentNode[]>();
    departments.forEach((dept) => {
      const branch = dept.branchId ?? 'unassigned';
      if (!grouped.has(branch)) {
        grouped.set(branch, []);
      }
      grouped.get(branch)!.push(dept);
    });
    return grouped;
  }, [departments]);

  const validateUniqueAssignment = () => {
    if (mode !== 'assign') return true;
    const duplicate = existingAssignments.find(
      (assignment) =>
        assignment.userId === userId &&
        assignment.departmentId === departmentId &&
        assignment.departmentRoleId === departmentRoleId &&
        !assignment.removedAt,
    );
    if (duplicate) {
      setError('This user already holds the selected role for the department.');
      return false;
    }
    return true;
  };

  if (!open) return null;

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);

    if (mode === 'assign') {
      if (!userId || !departmentId || !departmentRoleId) {
        setError('User, department, and role are required.');
        return;
      }
      if (!validateUniqueAssignment()) return;
      await onAssign({ userId, departmentId, departmentRoleId, isPrimary });
    }

    if (mode === 'transfer') {
      if (!assignmentId || !departmentRoleId) {
        setError('Existing assignment and destination role are required.');
        return;
      }
      await onTransfer({ assignmentId, toDepartmentRoleId: departmentRoleId, newPrimary: isPrimary });
    }

    if (mode === 'terminate') {
      if (!assignmentId) {
        setError('An assignment must be selected.');
        return;
      }
      await onTerminate({ assignmentId, reason });
    }
  };

  return (
    <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50">
      <div className="bg-white rounded shadow-xl w-full max-w-3xl p-6">
        <div className="flex items-center justify-between mb-4">
          <h2 className="text-lg font-semibold capitalize">{mode} assignment</h2>
          <button className="text-gray-600" onClick={onClose} aria-label="Close modal">
            ✕
          </button>
        </div>
        <form onSubmit={handleSubmit} className="space-y-4">
          {error && <div className="text-sm text-red-600">{error}</div>}

          {mode === 'assign' && (
            <div className="grid grid-cols-2 gap-4">
              <div>
                <label className="block text-sm font-medium">User Id</label>
                <input
                  className="mt-1 w-full rounded border px-3 py-2"
                  value={userId}
                  onChange={(e) => setUserId(e.target.value)}
                  placeholder="user-123"
                  required
                />
              </div>
              <div>
                <label className="block text-sm font-medium">Branch context</label>
                <select
                  className="mt-1 w-full rounded border px-3 py-2"
                  onChange={(e) => setDepartmentId('')}
                  defaultValue=""
                >
                  <option value="" disabled>
                    Choose a branch to filter departments
                  </option>
                  <option value="unassigned">Unassigned</option>
                  {branches.map((branch) => (
                    <option key={branch.id} value={branch.id}>
                      {branch.name}
                    </option>
                  ))}
                </select>
              </div>
              <div>
                <label className="block text-sm font-medium">Department</label>
                <select
                  className="mt-1 w-full rounded border px-3 py-2"
                  value={departmentId}
                  onChange={(e) => setDepartmentId(e.target.value)}
                  required
                >
                  <option value="" disabled>
                    Choose a department
                  </option>
                  {Array.from(branchDepartmentMap.entries()).map(([branch, branchDepts]) => (
                    <optgroup key={branch} label={branch === 'unassigned' ? 'No branch' : branch}>
                      {branchDepts.map((dept) => (
                        <option key={dept.id} value={dept.id}>
                          {dept.name}
                        </option>
                      ))}
                    </optgroup>
                  ))}
                </select>
              </div>
              <div>
                <label className="block text-sm font-medium">Role</label>
                <select
                  className="mt-1 w-full rounded border px-3 py-2"
                  value={departmentRoleId}
                  onChange={(e) => setDepartmentRoleId(e.target.value)}
                  required
                >
                  <option value="" disabled>
                    Choose a role definition
                  </option>
                  {roles.map((role) => (
                    <option key={role.id} value={role.id}>
                      {role.displayName}
                    </option>
                  ))}
                </select>
              </div>
              <div className="col-span-2 flex items-center space-x-2">
                <input id="primary" type="checkbox" checked={isPrimary} onChange={(e) => setIsPrimary(e.target.checked)} />
                <label htmlFor="primary" className="text-sm">
                  Mark as primary assignment
                </label>
              </div>
            </div>
          )}

          {mode === 'transfer' && (
            <div className="space-y-3">
              <div>
                <label className="block text-sm font-medium">Existing assignment</label>
                <select
                  className="mt-1 w-full rounded border px-3 py-2"
                  value={assignmentId}
                  onChange={(e) => setAssignmentId(e.target.value)}
                  required
                >
                  <option value="" disabled>
                    Choose assignment to transfer
                  </option>
                  {existingAssignments
                    .filter((assignment) => !assignment.removedAt)
                    .map((assignment) => (
                      <option key={assignment.id} value={assignment.id}>
                        {assignment.userId} — {assignment.department?.name ?? assignment.departmentId}
                      </option>
                    ))}
                </select>
              </div>
              <div>
                <label className="block text-sm font-medium">Destination Role</label>
                <select
                  className="mt-1 w-full rounded border px-3 py-2"
                  value={departmentRoleId}
                  onChange={(e) => setDepartmentRoleId(e.target.value)}
                  required
                >
                  <option value="" disabled>
                    Choose a role definition
                  </option>
                  {roles.map((role) => (
                    <option key={role.id} value={role.id}>
                      {role.displayName}
                    </option>
                  ))}
                </select>
              </div>
              <label className="inline-flex items-center space-x-2 text-sm">
                <input type="checkbox" checked={isPrimary} onChange={(e) => setIsPrimary(e.target.checked)} />
                <span>Mark transferred assignment as primary</span>
              </label>
            </div>
          )}

          {mode === 'terminate' && (
            <div className="space-y-3">
              <div>
                <label className="block text-sm font-medium">Assignment</label>
                <select
                  className="mt-1 w-full rounded border px-3 py-2"
                  value={assignmentId}
                  onChange={(e) => setAssignmentId(e.target.value)}
                  required
                >
                  <option value="" disabled>
                    Choose assignment to terminate
                  </option>
                  {existingAssignments
                    .filter((assignment) => !assignment.removedAt)
                    .map((assignment) => (
                      <option key={assignment.id} value={assignment.id}>
                        {assignment.userId} — {assignment.department?.name ?? assignment.departmentId}
                      </option>
                    ))}
                </select>
              </div>
              <div>
                <label className="block text-sm font-medium">Reason</label>
                <textarea
                  className="mt-1 w-full rounded border px-3 py-2"
                  rows={3}
                  value={reason}
                  onChange={(e) => setReason(e.target.value)}
                  placeholder="Provide context for audit logging"
                />
              </div>
            </div>
          )}

          <div className="flex justify-end space-x-3 pt-4">
            <button type="button" className="px-4 py-2 rounded border" onClick={onClose}>
              Cancel
            </button>
            <button type="submit" className="px-4 py-2 rounded bg-blue-600 text-white">
              Confirm
            </button>
          </div>
        </form>
      </div>
    </div>
  );
};
