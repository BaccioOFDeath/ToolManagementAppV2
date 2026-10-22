import React from 'react';

const SIZES = {
  sm: { padding: '0 14px', height: '34px', fontSize: '12px' },
  md: { padding: '0 20px', height: '44px', fontSize: '14px' },
  lg: { padding: '0 28px', height: '54px', fontSize: '16px' }
};

const TONES = {
  primary: { background: 'var(--brand-yellow)', color: 'var(--brand-black)', border: '1px solid var(--brand-yellow)' },
  secondary: { background: 'var(--brand-black)', color: 'var(--brand-white)', border: '1px solid var(--brand-black)' },
  outline: { background: 'transparent', color: 'var(--brand-black)', border: '1px solid var(--neutral-300)' },
  ghost: { background: 'transparent', color: 'var(--brand-black)', border: '1px solid transparent' },
  inverse: { background: 'var(--brand-white)', color: 'var(--brand-black)', border: '1px solid var(--brand-white)' }
};

const HOVER = {
  primary: { background: 'var(--yellow-500)', borderColor: 'var(--yellow-500)' },
  secondary: { background: 'var(--neutral-700)', borderColor: 'var(--neutral-700)' },
  outline: { background: 'var(--neutral-100)', borderColor: 'var(--neutral-400)' },
  ghost: { background: 'var(--neutral-100)' },
  inverse: { background: 'var(--neutral-200)', borderColor: 'var(--neutral-200)' }
};

export function Button({ children, variant = 'primary', size = 'md', iconLeft, iconRight, fullWidth = false, disabled = false, as, href, style, ...rest }) {
  const [h, setH] = React.useState(false);
  const [p, setP] = React.useState(false);
  const Tag = as || (href ? 'a' : 'button');
  const base = {
    display: 'inline-flex', alignItems: 'center', justifyContent: 'center', gap: 'var(--space-2)',
    width: fullWidth ? '100%' : undefined,
    fontFamily: 'var(--font-display)', fontWeight: 'var(--fw-bold)', letterSpacing: '0.02em',
    borderRadius: 'var(--radius-sm)', cursor: disabled ? 'not-allowed' : 'pointer',
    opacity: disabled ? 0.45 : 1, textDecoration: 'none', whiteSpace: 'nowrap',
    transition: 'var(--transition-control), transform var(--dur-instant) var(--ease-standard)',
    transform: p && !disabled ? 'var(--press-scale)' : 'none',
    ...SIZES[size], ...TONES[variant],
    ...(h && !disabled ? HOVER[variant] : null),
    ...style
  };
  return (
    <Tag
      href={href} disabled={Tag === 'button' ? disabled : undefined} style={base}
      onMouseEnter={() => setH(true)} onMouseLeave={() => { setH(false); setP(false); }}
      onMouseDown={() => setP(true)} onMouseUp={() => setP(false)}
      {...rest}
    >
      {iconLeft}{children}{iconRight}
    </Tag>
  );
}
