import { useCallback } from 'react';
import { useMutation } from '@apollo/client';
import { CREATE_AUDIT_LOG_MUTATION } from '../graphql/organization';

type AuditPayload = {
  action: string;
  entityId: string;
  details?: Record<string, unknown>;
};

export function useAuditLogger() {
  const [createAuditEvent] = useMutation(CREATE_AUDIT_LOG_MUTATION);

  const logAction = useCallback(
    async ({ action, entityId, details }: AuditPayload) => {
      try {
        await createAuditEvent({
          variables: { action, entityId, details },
        });
      } catch (error) {
        // Audit logging should never block the UX; errors are swallowed but surfaced for observability.
        console.warn('Audit logging failed', error);
      }
    },
    [createAuditEvent],
  );

  return { logAction };
}
