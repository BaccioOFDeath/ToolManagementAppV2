import React from 'react';

const TONES = {
  neutral: ['var(--neutral-100)', 'var(--neutral-700)'],
  accent: ['var(--brand-yellow)', 'var(--brand-black)'],
  dark: ['var(--brand-black)', 'var(--brand-white)'],
  success: ['var(--status-success-surface)', 'var(--status-success)'],
  warning: ['var(--status-warning-surface)', 'var(--status-warning)'],
  danger: ['var(--status-danger-surface)', 'var(--status-danger)'],
  info: ['var(--status-info-surface)', 'var(--status-info)']
};

export function Badge({ children, tone = 'neutral', size = 'md', dot = false, style, ...rest }) {
  const [bg, fg] = TONES[tone];
  return (
    <span style={{
      display: 'inline-flex', alignItems: 'center', gap: '6px',
      background: bg, color: fg, borderRadius: 'var(--radius-pill)',
      padding: size === 'sm' ? '2px 8px' : '4px 12px',
      fontFamily: 'var(--font-body)', fontWeight: 'var(--fw-semibold)',
      fontSize: size === 'sm' ? 'var(--fs-2xs)' : 'var(--fs-xs)',
      letterSpacing: 'var(--ls-wide)', textTransform: 'uppercase', whiteSpace: 'nowrap', ...style
    }} {...rest}>
      {dot ? <span style={{ width: '6px', height: '6px', borderRadius: '50%', background: 'currentColor' }} /> : null}
      {children}
    </span>
  );
}
