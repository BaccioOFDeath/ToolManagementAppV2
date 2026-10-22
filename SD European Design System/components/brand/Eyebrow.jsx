import React from 'react';

export function Eyebrow({ children, tone = 'muted', as: Tag = 'div', style, ...rest }) {
  const color = tone === 'accent' ? 'var(--brand-yellow)' : tone === 'inverse' ? 'rgba(255,255,255,.72)' : 'var(--text-secondary)';
  return (
    <Tag style={{ font: 'var(--type-eyebrow)', letterSpacing: 'var(--ls-widest)', textTransform: 'uppercase', color, ...style }} {...rest}>{children}</Tag>
  );
}
