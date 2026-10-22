import React from 'react';

export function Select({ label, hint, error, options = [], id, size = 'md', style, ...rest }) {
  const auto = React.useId ? React.useId() : 'sel';
  const selId = id || auto;
  const [focus, setFocus] = React.useState(false);
  const h = size === 'sm' ? 36 : size === 'lg' ? 54 : 44;
  return (
    <div style={{ display: 'grid', gap: 'var(--space-2)', ...style }}>
      {label ? <label htmlFor={selId} style={{ font: 'var(--type-label)', letterSpacing: 'var(--ls-wide)', textTransform: 'uppercase', color: 'var(--text-secondary)' }}>{label}</label> : null}
      <div style={{ position: 'relative' }}>
        <select
          id={selId} onFocus={() => setFocus(true)} onBlur={() => setFocus(false)}
          style={{
            width: '100%', height: h + 'px', padding: '0 40px 0 12px', appearance: 'none',
            background: 'var(--brand-white)', borderRadius: 'var(--radius-sm)',
            border: '1px solid ' + (error ? 'var(--status-danger)' : focus ? 'var(--brand-black)' : 'var(--border-default)'),
            boxShadow: focus && !error ? 'var(--ring-focus)' : 'none',
            font: 'var(--type-body)', color: 'var(--text-primary)', outline: 'none',
            transition: 'var(--transition-control)', cursor: 'pointer'
          }}
          {...rest}
        >
          {options.map((o) => {
            const v = typeof o === 'string' ? o : o.value;
            const l = typeof o === 'string' ? o : o.label;
            return <option key={v} value={v}>{l}</option>;
          })}
        </select>
        <span aria-hidden="true" style={{ position: 'absolute', right: '14px', top: '50%', width: '9px', height: '9px', borderRight: '2px solid var(--brand-black)', borderBottom: '2px solid var(--brand-black)', transform: 'translateY(-70%) rotate(45deg)', pointerEvents: 'none' }} />
      </div>
      {error ? <span style={{ font: 'var(--type-body-sm)', color: 'var(--status-danger)' }}>{error}</span>
        : hint ? <span style={{ font: 'var(--type-body-sm)', color: 'var(--text-muted)' }}>{hint}</span> : null}
    </div>
  );
}
