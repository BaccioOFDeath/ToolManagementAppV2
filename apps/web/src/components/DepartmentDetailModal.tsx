import React, { useEffect, useMemo, useState } from 'react';
import { DepartmentNode } from '../graphql/organization';

type BranchOption = { id: string; name: string; code: string };

type Props = {
  open: boolean;
  branches: BranchOption[];
  department?: DepartmentNode;
  onSave: (input: {
    id?: string;
    code: string;
    name: string;
    branchId?: string | null;
    parentDepartmentId?: string | null;
    metadata?: Record<string, unknown>;
  }) => Promise<void>;
  onClose: () => void;
};

export const DepartmentDetailModal: React.FC<Props> = ({ open, branches, department, onSave, onClose }) => {
  const [code, setCode] = useState('');
  const [name, setName] = useState('');
  const [branchId, setBranchId] = useState<string | undefined | null>(undefined);
  const [parentDepartmentId, setParentDepartmentId] = useState<string | undefined | null>(undefined);
  const [metadataJson, setMetadataJson] = useState('');
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (department) {
      setCode(department.code);
      setName(department.name);
      setBranchId(department.branchId ?? undefined);
      setParentDepartmentId(department.parentDepartmentId ?? undefined);
      setMetadataJson(JSON.stringify(department.metadata ?? {}, null, 2));
    } else {
      setCode('');
      setName('');
      setBranchId(undefined);
      setParentDepartmentId(undefined);
      setMetadataJson('');
    }
  }, [department]);

  const branchOptions = useMemo(() => branches.sort((a, b) => a.name.localeCompare(b.name)), [branches]);

  if (!open) {
    return null;
  }

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);

    if (!code.trim() || !name.trim()) {
      setError('Code and name are required.');
      return;
    }

    let metadata: Record<string, unknown> | undefined;
    if (metadataJson.trim()) {
      try {
        metadata = JSON.parse(metadataJson);
      } catch (parseError) {
        setError('Metadata must be valid JSON.');
        return;
      }
    }

    await onSave({
      id: department?.id,
      code: code.trim(),
      name: name.trim(),
      branchId: branchId ?? null,
      parentDepartmentId: parentDepartmentId ?? null,
      metadata,
    });
  };

  return (
    <div className="fixed inset-0 bg-black/40 flex items-center justify-center z-50">
      <div className="bg-white rounded shadow-lg w-full max-w-2xl p-6">
        <div className="flex items-center justify-between mb-4">
          <h2 className="text-lg font-semibold">{department ? 'Edit Department' : 'Create Department'}</h2>
          <button onClick={onClose} className="text-gray-500 hover:text-gray-700" aria-label="Close dialog">
            ✕
          </button>
        </div>
        <form onSubmit={handleSubmit} className="space-y-4">
          {error && <div className="text-red-600 text-sm">{error}</div>}
          <div>
            <label className="block text-sm font-medium">Code</label>
            <input
              className="mt-1 w-full rounded border px-3 py-2"
              value={code}
              onChange={(e) => setCode(e.target.value)}
              required
            />
          </div>
          <div>
            <label className="block text-sm font-medium">Name</label>
            <input
              className="mt-1 w-full rounded border px-3 py-2"
              value={name}
              onChange={(e) => setName(e.target.value)}
              required
            />
          </div>
          <div>
            <label className="block text-sm font-medium">Branch</label>
            <select
              className="mt-1 w-full rounded border px-3 py-2"
              value={branchId ?? ''}
              onChange={(e) => setBranchId(e.target.value || null)}
            >
              <option value="">Unassigned</option>
              {branchOptions.map((branch) => (
                <option key={branch.id} value={branch.id}>
                  {branch.name} ({branch.code})
                </option>
              ))}
            </select>
          </div>
          <div>
            <label className="block text-sm font-medium">Parent Department Id</label>
            <input
              className="mt-1 w-full rounded border px-3 py-2"
              value={parentDepartmentId ?? ''}
              onChange={(e) => setParentDepartmentId(e.target.value || null)}
            />
          </div>
          <div>
            <label className="block text-sm font-medium">Metadata (JSON)</label>
            <textarea
              className="mt-1 w-full rounded border px-3 py-2 font-mono text-xs"
              rows={4}
              value={metadataJson}
              onChange={(e) => setMetadataJson(e.target.value)}
              placeholder="{\n  \"type\": \"service\"\n}"
            />
          </div>
          <div className="flex justify-end space-x-3 pt-2">
            <button type="button" className="px-4 py-2 rounded border" onClick={onClose}>
              Cancel
            </button>
            <button type="submit" className="px-4 py-2 rounded bg-blue-600 text-white">
              {department ? 'Update' : 'Create'}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
};
