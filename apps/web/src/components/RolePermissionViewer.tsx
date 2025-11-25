import React, { useMemo, useState } from 'react';
import { RoleDefinitionNode } from '../graphql/organization';

type Props = {
  roles: RoleDefinitionNode[];
  permissionMap: Record<string, string[]>;
};

type RoleTreeNode = RoleDefinitionNode & { children: RoleTreeNode[] };

function buildRoleTree(roles: RoleDefinitionNode[]): RoleTreeNode[] {
  const roleMap = new Map<string, RoleTreeNode>();
  roles.forEach((role) => roleMap.set(role.id, { ...role, children: [] }));

  const roots: RoleTreeNode[] = [];
  roleMap.forEach((role) => {
    if (role.inheritsFromIds.length === 0) {
      roots.push(role);
      return;
    }

    role.inheritsFromIds.forEach((parentId) => {
      const parent = roleMap.get(parentId);
      if (parent) {
        parent.children.push(role);
      }
    });
  });

  return roots;
}

const RoleNode: React.FC<{ node: RoleTreeNode; permissionMap: Record<string, string[]> }> = ({ node, permissionMap }) => {
  const [expanded, setExpanded] = useState(true);
  const directPermissions = permissionMap[node.id] ?? node.permissions;
  const inheritedPermissions = new Set<string>();
  node.inheritsFromIds.forEach((parentId) => {
    (permissionMap[parentId] ?? []).forEach((perm) => inheritedPermissions.add(perm));
  });

  return (
    <div className="border rounded p-3 mb-3">
      <div className="flex items-center justify-between">
        <div>
          <p className="font-semibold">{node.displayName}</p>
          <p className="text-xs text-gray-600">Key: {node.key}</p>
          <p className="text-xs text-gray-600">Scope: {node.scope}</p>
        </div>
        <button className="text-blue-600 text-sm" onClick={() => setExpanded((v) => !v)}>
          {expanded ? 'Collapse' : 'Expand'}
        </button>
      </div>
      {expanded && (
        <div className="mt-2 space-y-2">
          <div>
            <p className="text-sm font-medium">Permissions</p>
            <ul className="list-disc list-inside text-sm">
              {directPermissions.map((permission) => {
                const isInherited = inheritedPermissions.has(permission) && !node.permissions.includes(permission);
                return (
                  <li key={permission} className={isInherited ? 'text-amber-700' : ''}>
                    {permission}
                    {isInherited && <span className="ml-1 text-xs text-amber-700">(inherited)</span>}
                  </li>
                );
              })}
              {directPermissions.length === 0 && <li className="text-gray-500">No permissions assigned.</li>}
            </ul>
          </div>
          {node.children.length > 0 && (
            <div className="pl-3 border-l">
              <p className="text-sm font-medium mb-1">Inherited by</p>
              {node.children.map((child) => (
                <RoleNode key={child.id} node={child} permissionMap={permissionMap} />
              ))}
            </div>
          )}
        </div>
      )}
    </div>
  );
};

export const RolePermissionViewer: React.FC<Props> = ({ roles, permissionMap }) => {
  const roots = useMemo(() => buildRoleTree(roles), [roles]);

  if (!roles.length) {
    return <p className="text-sm text-gray-500">No roles are available for this scope.</p>;
  }

  return (
    <div className="space-y-3">
      {roots.map((role) => (
        <RoleNode key={role.id} node={role} permissionMap={permissionMap} />
      ))}
    </div>
  );
};
