import React from 'react';

export function Radio({ label, description, name, value, checked, disabled, onChange, id, style, ...rest }) {
  const auto = React.useId ? React.useId() : 'rd';
  const rId = id || auto;
  return (
    <label htmlFor={rId} style={{ display: 'grid', gridTemplateColumns: '20px 1fr', gap: 'var(--space-3)', alignItems: 'start', cursor: disabled ? 'not-allowed' : 'pointer', opacity: disabled ? 0.45 : 1, ...style }}>
      <input id={rId} type="radio" name={name} value={value} checked={checked} disabled={disabled} onChange={onChange}
        style={{ position: 'absolute', opacity: 0, width: 0, height: 0 }} {...rest} />
      <span aria-hidden="true" style={{
        width: '20px', height: '20px', marginTop: '2px', borderRadius: 'var(--radius-circle)',
        border: '1px solid ' + (checked ? 'var(--brand-black)' : 'var(--border-strong)'),
        background: 'var(--brand-white)', display: 'inline-flex', alignItems: 'center', justifyContent: 'center',
        transition: 'var(--transition-control)'
      }}>
        {checked ? <span style={{ width: '10px', height: '10px', borderRadius: '50%', background: 'var(--brand-black)' }} /> : null}
      </span>
      <span style={{ display: 'grid', gap: '2px' }}>
        <span style={{ font: 'var(--type-body)', color: 'var(--text-primary)' }}>{label}</span>
        {description ? <span style={{ font: 'var(--type-body-sm)', color: 'var(--text-muted)' }}>{description}</span> : null}
      </span>
    </label>
  );
}
