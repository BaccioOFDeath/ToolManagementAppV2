import React from 'react';

const TONES = {
  neutral: { bar: 'var(--brand-yellow)', fg: 'var(--brand-white)', bg: 'var(--brand-black)' },
  success: { bar: 'var(--status-success)', fg: 'var(--brand-white)', bg: 'var(--brand-black)' },
  warning: { bar: 'var(--status-warning)', fg: 'var(--brand-white)', bg: 'var(--brand-black)' },
  danger: { bar: 'var(--status-danger)', fg: 'var(--brand-white)', bg: 'var(--brand-black)' }
};

export function Toast({ title, message, tone = 'neutral', action, onDismiss, icon, style, ...rest }) {
  const t = TONES[tone];
  return (
    <div role="status" style={{
      display: 'flex', alignItems: 'center', gap: 'var(--space-4)', minWidth: '320px', maxWidth: '440px',
      background: t.bg, color: t.fg, borderRadius: 'var(--radius-md)', boxShadow: 'var(--shadow-lg)',
      borderLeft: '4px solid ' + t.bar, padding: 'var(--space-4) var(--space-5)', ...style
    }} {...rest}>
      {icon}
      <div style={{ flex: 1, display: 'grid', gap: '2px' }}>
        <strong style={{ font: 'var(--type-h4)', fontSize: 'var(--fs-sm)', color: 'inherit' }}>{title}</strong>
        {message ? <span style={{ font: 'var(--type-body-sm)', color: 'rgba(255,255,255,.72)' }}>{message}</span> : null}
      </div>
      {action}
      {onDismiss ? <button onClick={onDismiss} aria-label="Dismiss" style={{ border: 0, background: 'transparent', color: 'rgba(255,255,255,.65)', fontSize: '20px', lineHeight: 1, cursor: 'pointer' }}>×</button> : null}
    </div>
  );
}
