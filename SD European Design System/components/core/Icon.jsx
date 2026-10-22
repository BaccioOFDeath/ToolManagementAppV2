import React from 'react';

/**
 * Thin wrapper over Lucide (loaded from CDN as window.lucide).
 * Enforces the brand rules: stroke-only, currentColor, capped size scale.
 */
export function Icon({ name, size = 20, strokeWidth = 2, label, style, ...rest }) {
  const ref = React.useRef(null);

  React.useEffect(() => {
    const host = ref.current;
    if (!host) return;
    let cancelled = false;
    const draw = () => {
      if (cancelled || !host) return false;
      const L = typeof window !== 'undefined' ? window.lucide : null;
      if (!L || typeof L.createIcons !== 'function') return false;
      host.innerHTML = '<i data-lucide="' + name + '"></i>';
      try {
        L.createIcons({
          nameAttr: 'data-lucide',
          attrs: { width: size, height: size, 'stroke-width': strokeWidth, stroke: 'currentColor' },
          root: host
        });
      } catch (e) { return false; }
      return !!host.querySelector('svg');
    };
    if (!draw()) {
      let tries = 0;
      const id = setInterval(() => { if (draw() || ++tries > 40) clearInterval(id); }, 100);
      return () => { cancelled = true; clearInterval(id); };
    }
    return () => { cancelled = true; };
  }, [name, size, strokeWidth]);

  return (
    <span
      ref={ref}
      role={label ? 'img' : undefined}
      aria-label={label}
      aria-hidden={label ? undefined : true}
      data-icon={name}
      style={{ display: 'inline-flex', alignItems: 'center', justifyContent: 'center', width: size + 'px', height: size + 'px', flex: '0 0 auto', color: 'inherit', ...style }}
      {...rest}
    />
  );
}
