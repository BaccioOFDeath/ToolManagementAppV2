import React from 'react';
import { Eyebrow } from './Eyebrow.jsx';

export function SectionHeading({ eyebrow, title, lead, level = 2, align = 'left', inverse = false, rule = true, style, ...rest }) {
  const H = 'h' + level;
  return (
    <header style={{ display: 'grid', gap: 'var(--space-3)', justifyItems: align === 'center' ? 'center' : 'start', textAlign: align, maxWidth: '760px', ...style }} {...rest}>
      {eyebrow ? <Eyebrow tone={inverse ? 'inverse' : 'muted'}>{eyebrow}</Eyebrow> : null}
      <H style={{ font: level === 1 ? 'var(--type-h1)' : level === 2 ? 'var(--type-h2)' : 'var(--type-h3)', letterSpacing: 'var(--ls-tight)', color: inverse ? 'var(--text-inverse)' : 'var(--text-primary)', margin: 0 }}>{title}</H>
      {rule ? <hr style={{ height: '4px', width: '56px', background: 'var(--brand-yellow)', border: 0, margin: 'var(--space-1) 0 0' }} /> : null}
      {lead ? <p style={{ font: 'var(--type-lead)', color: inverse ? 'rgba(255,255,255,.78)' : 'var(--text-secondary)', margin: 'var(--space-2) 0 0' }}>{lead}</p> : null}
    </header>
  );
}
