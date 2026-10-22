import React from 'react';

export function Input({ label, hint, error, prefix, suffix, id, size = 'md', mono = false, style, ...rest }) {
  const auto = React.useId ? React.useId() : 'in';
  const inputId = id || auto;
  const [focus, setFocus] = React.useState(false);
  const h = size === 'sm' ? 36 : size === 'lg' ? 54 : 44;
  return (
    <div style={{ display: 'grid', gap: 'var(--space-2)', ...style }}>
      {label ? <label htmlFor={inputId} style={{ font: 'var(--type-label)', letterSpacing: 'var(--ls-wide)', textTransform: 'uppercase', color: 'var(--text-secondary)' }}>{label}</label> : null}
      <div style={{
        display: 'flex', alignItems: 'center', gap: 'var(--space-2)', height: h + 'px', padding: '0 12px',
        background: 'var(--brand-white)', borderRadius: 'var(--radius-sm)',
        border: '1px solid ' + (error ? 'var(--status-danger)' : focus ? 'var(--brand-black)' : 'var(--border-default)'),
        boxShadow: focus && !error ? 'var(--ring-focus)' : 'none',
        transition: 'var(--transition-control)', color: 'var(--text-muted)'
      }}>
        {prefix}
        <input
          id={inputId} onFocus={() => setFocus(true)} onBlur={() => setFocus(false)}
          style={{
            flex: 1, minWidth: 0, border: 0, outline: 'none', background: 'transparent',
            font: mono ? 'var(--type-mono)' : 'var(--type-body)', color: 'var(--text-primary)',
            textTransform: mono ? 'uppercase' : 'none', letterSpacing: mono ? '0.06em' : 'normal'
          }}
          {...rest}
        />
        {suffix}
      </div>
      {error ? <span style={{ font: 'var(--type-body-sm)', color: 'var(--status-danger)' }}>{error}</span>
        : hint ? <span style={{ font: 'var(--type-body-sm)', color: 'var(--text-muted)' }}>{hint}</span> : null}
    </div>
  );
}
