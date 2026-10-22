import React from 'react';

export function Dialog({ open, title, description, children, footer, onClose, width = 520, style, ...rest }) {
  React.useEffect(() => {
    if (!open) return;
    const k = (e) => { if (e.key === 'Escape' && onClose) onClose(); };
    window.addEventListener('keydown', k);
    return () => window.removeEventListener('keydown', k);
  }, [open, onClose]);
  if (!open) return null;
  return (
    <div onClick={onClose} style={{ position: 'fixed', inset: 0, background: 'var(--surface-overlay)', display: 'grid', placeItems: 'center', padding: 'var(--space-6)', zIndex: 1000, animation: 'none' }}>
      <div role="dialog" aria-modal="true" aria-label={typeof title === 'string' ? title : undefined}
        onClick={(e) => e.stopPropagation()}
        style={{
          width: '100%', maxWidth: width + 'px', background: 'var(--surface-card)',
          borderRadius: 'var(--radius-md)', boxShadow: 'var(--shadow-overlay)', overflow: 'hidden', ...style
        }} {...rest}>
        <div style={{ padding: 'var(--space-6)', display: 'grid', gap: 'var(--space-3)' }}>
          <div style={{ display: 'flex', alignItems: 'start', gap: 'var(--space-4)' }}>
            <h2 style={{ font: 'var(--type-h3)', margin: 0, flex: 1 }}>{title}</h2>
            <button onClick={onClose} aria-label="Close" style={{ border: 0, background: 'transparent', fontSize: '22px', lineHeight: 1, cursor: 'pointer', color: 'var(--text-muted)' }}>×</button>
          </div>
          {description ? <p style={{ font: 'var(--type-body)', color: 'var(--text-secondary)' }}>{description}</p> : null}
          {children}
        </div>
        {footer ? <div style={{ padding: 'var(--space-4) var(--space-6)', background: 'var(--surface-subtle)', display: 'flex', justifyContent: 'flex-end', gap: 'var(--space-3)' }}>{footer}</div> : null}
      </div>
    </div>
  );
}
