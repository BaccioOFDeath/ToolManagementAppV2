import React from 'react';

export function Checkbox({ label, description, checked, defaultChecked, disabled, onChange, id, style, ...rest }) {
  const auto = React.useId ? React.useId() : 'cb';
  const cbId = id || auto;
  const [inner, setInner] = React.useState(!!defaultChecked);
  const on = checked !== undefined ? checked : inner;
  return (
    <label htmlFor={cbId} style={{ display: 'grid', gridTemplateColumns: '20px 1fr', gap: 'var(--space-3)', alignItems: 'start', cursor: disabled ? 'not-allowed' : 'pointer', opacity: disabled ? 0.45 : 1, ...style }}>
      <input
        id={cbId} type="checkbox" checked={on} disabled={disabled}
        onChange={(e) => { if (checked === undefined) setInner(e.target.checked); onChange && onChange(e); }}
        style={{ position: 'absolute', opacity: 0, width: 0, height: 0 }}
        {...rest}
      />
      <span aria-hidden="true" style={{
        width: '20px', height: '20px', marginTop: '2px', borderRadius: 'var(--radius-xs)',
        border: '1px solid ' + (on ? 'var(--brand-black)' : 'var(--border-strong)'),
        background: on ? 'var(--brand-black)' : 'var(--brand-white)',
        display: 'inline-flex', alignItems: 'center', justifyContent: 'center', transition: 'var(--transition-control)'
      }}>
        {on ? <span style={{ width: '5px', height: '9px', borderRight: '2px solid var(--brand-yellow)', borderBottom: '2px solid var(--brand-yellow)', transform: 'rotate(45deg) translate(-1px,-1px)' }} /> : null}
      </span>
      <span style={{ display: 'grid', gap: '2px' }}>
        <span style={{ font: 'var(--type-body)', color: 'var(--text-primary)' }}>{label}</span>
        {description ? <span style={{ font: 'var(--type-body-sm)', color: 'var(--text-muted)' }}>{description}</span> : null}
      </span>
    </label>
  );
}
