import React from 'react';

export function Card({ children, media, mediaAlt = '', title, meta, footer, variant = 'default', interactive = false, padding = 'var(--space-6)', style, ...rest }) {
  const [h, setH] = React.useState(false);
  const tones = {
    default: { background: 'var(--surface-card)', border: '1px solid var(--border-default)', color: 'var(--text-primary)' },
    subtle: { background: 'var(--surface-subtle)', border: '1px solid transparent', color: 'var(--text-primary)' },
    inverse: { background: 'var(--surface-inverse)', border: '1px solid var(--border-inverse)', color: 'var(--text-inverse)' },
    accent: { background: 'var(--surface-accent)', border: '1px solid var(--brand-yellow)', color: 'var(--brand-black)' }
  }[variant];
  return (
    <div
      onMouseEnter={() => setH(true)} onMouseLeave={() => setH(false)}
      style={{
        borderRadius: media ? 'var(--radius-lg)' : 'var(--radius-md)', overflow: 'hidden',
        display: 'flex', flexDirection: 'column',
        transition: 'var(--transition-surface), border-color var(--dur-fast) var(--ease-standard)',
        transform: interactive && h ? 'var(--hover-lift)' : 'none',
        boxShadow: interactive && h ? 'var(--shadow-sm)' : 'var(--shadow-none)',
        cursor: interactive ? 'pointer' : undefined, ...tones, ...style
      }}
      {...rest}
    >
      {media ? <img src={media} alt={mediaAlt} style={{ width: '100%', aspectRatio: '4 / 3', objectFit: 'cover', background: 'var(--neutral-100)' }} /> : null}
      <div style={{ padding, display: 'flex', flexDirection: 'column', gap: 'var(--space-2)', flex: 1 }}>
        {meta ? <div style={{ font: 'var(--type-label)', letterSpacing: 'var(--ls-wide)', textTransform: 'uppercase', color: variant === 'inverse' ? 'rgba(255,255,255,.6)' : 'var(--text-muted)' }}>{meta}</div> : null}
        {title ? <h3 style={{ font: 'var(--type-h4)', color: 'inherit', margin: 0 }}>{title}</h3> : null}
        {children ? <div style={{ font: 'var(--type-body-sm)', color: variant === 'inverse' ? 'rgba(255,255,255,.75)' : 'var(--text-secondary)' }}>{children}</div> : null}
        {footer ? <div style={{ marginTop: 'auto', paddingTop: 'var(--space-4)' }}>{footer}</div> : null}
      </div>
    </div>
  );
}
