import React from 'react';

export function Tooltip({ label, placement = 'top', children, style, ...rest }) {
  const [on, setOn] = React.useState(false);
  const pos = {
    top: { bottom: '100%', left: '50%', transform: 'translate(-50%,-8px)' },
    bottom: { top: '100%', left: '50%', transform: 'translate(-50%,8px)' },
    left: { right: '100%', top: '50%', transform: 'translate(-8px,-50%)' },
    right: { left: '100%', top: '50%', transform: 'translate(8px,-50%)' }
  }[placement];
  return (
    <span style={{ position: 'relative', display: 'inline-flex', ...style }}
      onMouseEnter={() => setOn(true)} onMouseLeave={() => setOn(false)}
      onFocus={() => setOn(true)} onBlur={() => setOn(false)} {...rest}>
      {children}
      <span role="tooltip" style={{
        position: 'absolute', ...pos, zIndex: 20, pointerEvents: 'none',
        opacity: on ? 1 : 0, visibility: on ? 'visible' : 'hidden',
        transition: 'opacity var(--dur-fast) var(--ease-standard)',
        background: 'var(--brand-black)', color: 'var(--brand-white)',
        font: 'var(--type-body-sm)', padding: '6px 10px', borderRadius: 'var(--radius-sm)',
        whiteSpace: 'nowrap', boxShadow: 'var(--shadow-md)'
      }}>{label}</span>
    </span>
  );
}
