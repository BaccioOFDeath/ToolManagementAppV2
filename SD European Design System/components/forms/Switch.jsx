import React from 'react';

export function Switch({ label, checked, defaultChecked, disabled, onChange, id, style, ...rest }) {
  const auto = React.useId ? React.useId() : 'sw';
  const sId = id || auto;
  const [inner, setInner] = React.useState(!!defaultChecked);
  const on = checked !== undefined ? checked : inner;
  return (
    <label htmlFor={sId} style={{ display: 'inline-flex', alignItems: 'center', gap: 'var(--space-3)', cursor: disabled ? 'not-allowed' : 'pointer', opacity: disabled ? 0.45 : 1, ...style }}>
      <input id={sId} type="checkbox" role="switch" checked={on} disabled={disabled}
        onChange={(e) => { if (checked === undefined) setInner(e.target.checked); onChange && onChange(e); }}
        style={{ position: 'absolute', opacity: 0, width: 0, height: 0 }} {...rest} />
      <span aria-hidden="true" style={{
        width: '44px', height: '24px', borderRadius: 'var(--radius-pill)', padding: '2px',
        background: on ? 'var(--brand-black)' : 'var(--neutral-300)',
        display: 'inline-flex', alignItems: 'center', transition: 'background-color var(--dur-base) var(--ease-standard)'
      }}>
        <span style={{
          width: '20px', height: '20px', borderRadius: '50%',
          background: on ? 'var(--brand-yellow)' : 'var(--brand-white)',
          transform: on ? 'translateX(20px)' : 'translateX(0)',
          transition: 'transform var(--dur-base) var(--ease-standard), background-color var(--dur-base) var(--ease-standard)',
          boxShadow: 'var(--shadow-xs)'
        }} />
      </span>
      {label ? <span style={{ font: 'var(--type-body)' }}>{label}</span> : null}
    </label>
  );
}
