import React from 'react';

export function Tag({ children, selected = false, onRemove, onClick, style, ...rest }) {
  const [h, setH] = React.useState(false);
  return (
    <span
      onClick={onClick}
      onMouseEnter={() => setH(true)} onMouseLeave={() => setH(false)}
      style={{
        display: 'inline-flex', alignItems: 'center', gap: 'var(--space-2)',
        padding: '6px 12px', borderRadius: 'var(--radius-sm)',
        border: '1px solid ' + (selected ? 'var(--brand-black)' : 'var(--border-default)'),
        background: selected ? 'var(--brand-black)' : (h && onClick ? 'var(--neutral-100)' : 'var(--brand-white)'),
        color: selected ? 'var(--brand-white)' : 'var(--text-primary)',
        font: 'var(--type-body-sm)', fontWeight: 'var(--fw-semibold)',
        cursor: onClick ? 'pointer' : 'default', transition: 'var(--transition-control)', ...style
      }}
      {...rest}
    >
      {children}
      {onRemove ? (
        <button onClick={(e) => { e.stopPropagation(); onRemove(e); }} aria-label="Remove"
          style={{ border: 0, background: 'transparent', color: 'inherit', cursor: 'pointer', padding: 0, lineHeight: 1, fontSize: '15px' }}>×</button>
      ) : null}
    </span>
  );
}
