import React from 'react';

const S = { sm: 32, md: 40, lg: 48 };

export function IconButton({ icon, label, variant = 'ghost', size = 'md', shape = 'circle', disabled = false, style, ...rest }) {
  const [h, setH] = React.useState(false);
  const tones = {
    ghost: { background: 'transparent', color: 'var(--brand-black)', border: '1px solid transparent', hover: 'var(--neutral-100)' },
    solid: { background: 'var(--brand-black)', color: 'var(--brand-white)', border: '1px solid var(--brand-black)', hover: 'var(--neutral-700)' },
    accent: { background: 'var(--brand-yellow)', color: 'var(--brand-black)', border: '1px solid var(--brand-yellow)', hover: 'var(--yellow-500)' },
    outline: { background: 'var(--brand-white)', color: 'var(--brand-black)', border: '1px solid var(--neutral-300)', hover: 'var(--neutral-100)' }
  }[variant];
  return (
    <button
      aria-label={label} disabled={disabled}
      onMouseEnter={() => setH(true)} onMouseLeave={() => setH(false)}
      style={{
        width: S[size] + 'px', height: S[size] + 'px', display: 'inline-flex', alignItems: 'center', justifyContent: 'center',
        borderRadius: shape === 'circle' ? 'var(--radius-circle)' : 'var(--radius-sm)',
        background: h && !disabled ? tones.hover : tones.background, color: tones.color, border: tones.border,
        cursor: disabled ? 'not-allowed' : 'pointer', opacity: disabled ? 0.45 : 1, padding: 0,
        transition: 'var(--transition-control)', ...style
      }}
      {...rest}
    >{icon}</button>
  );
}
