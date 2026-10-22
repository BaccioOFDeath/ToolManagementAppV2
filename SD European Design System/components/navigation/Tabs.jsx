import React from 'react';

export function Tabs({ items = [], value, defaultValue, onChange, variant = 'underline', style, ...rest }) {
  const [inner, setInner] = React.useState(defaultValue ?? (items[0] && items[0].value));
  const active = value !== undefined ? value : inner;
  const pick = (v) => { if (value === undefined) setInner(v); onChange && onChange(v); };
  const underline = variant === 'underline';
  return (
    <div role="tablist" style={{ display: 'flex', gap: underline ? 'var(--space-6)' : 'var(--space-2)', borderBottom: underline ? '1px solid var(--border-default)' : 'none', ...style }} {...rest}>
      {items.map((it) => {
        const on = it.value === active;
        return (
          <button key={it.value} role="tab" aria-selected={on} onClick={() => pick(it.value)}
            style={{
              display: 'inline-flex', alignItems: 'center', gap: 'var(--space-2)',
              border: underline ? 0 : '1px solid ' + (on ? 'var(--brand-black)' : 'var(--border-default)'),
              borderBottom: underline ? '3px solid ' + (on ? 'var(--brand-yellow)' : 'transparent') : undefined,
              background: underline ? 'transparent' : (on ? 'var(--brand-black)' : 'var(--brand-white)'),
              color: underline ? (on ? 'var(--text-primary)' : 'var(--text-muted)') : (on ? 'var(--brand-white)' : 'var(--text-primary)'),
              borderRadius: underline ? 0 : 'var(--radius-sm)',
              padding: underline ? '0 0 12px' : '8px 16px', marginBottom: underline ? '-1px' : 0,
              font: 'var(--type-button)', fontSize: 'var(--fs-sm)', letterSpacing: 'var(--ls-wide)',
              textTransform: 'uppercase', cursor: 'pointer', transition: 'var(--transition-control)'
            }}>
            {it.icon}{it.label}
            {it.count !== undefined ? <span style={{ font: 'var(--type-body-sm)', opacity: .6 }}>({it.count})</span> : null}
          </button>
        );
      })}
    </div>
  );
}
