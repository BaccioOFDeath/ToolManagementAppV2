/* @ds-bundle: {"format":4,"namespace":"SDEuropeanDesignSystem_614674","components":[{"name":"Eyebrow","sourcePath":"components/brand/Eyebrow.jsx"},{"name":"Logo","sourcePath":"components/brand/Logo.jsx"},{"name":"SectionHeading","sourcePath":"components/brand/SectionHeading.jsx"},{"name":"Badge","sourcePath":"components/core/Badge.jsx"},{"name":"Button","sourcePath":"components/core/Button.jsx"},{"name":"Card","sourcePath":"components/core/Card.jsx"},{"name":"Icon","sourcePath":"components/core/Icon.jsx"},{"name":"IconButton","sourcePath":"components/core/IconButton.jsx"},{"name":"Tag","sourcePath":"components/core/Tag.jsx"},{"name":"Dialog","sourcePath":"components/feedback/Dialog.jsx"},{"name":"Toast","sourcePath":"components/feedback/Toast.jsx"},{"name":"Tooltip","sourcePath":"components/feedback/Tooltip.jsx"},{"name":"Checkbox","sourcePath":"components/forms/Checkbox.jsx"},{"name":"Input","sourcePath":"components/forms/Input.jsx"},{"name":"Radio","sourcePath":"components/forms/Radio.jsx"},{"name":"Select","sourcePath":"components/forms/Select.jsx"},{"name":"Switch","sourcePath":"components/forms/Switch.jsx"},{"name":"Tabs","sourcePath":"components/navigation/Tabs.jsx"}],"sourceHashes":{"components/brand/Eyebrow.jsx":"3afa33fbd894","components/brand/Logo.jsx":"6633eb64e523","components/brand/SectionHeading.jsx":"c486b87a205f","components/core/Badge.jsx":"aa36c3666f00","components/core/Button.jsx":"9980cc8c515a","components/core/Card.jsx":"88d75ce2b636","components/core/Icon.jsx":"ac6c481bb52d","components/core/IconButton.jsx":"46fa0bbdbcde","components/core/Tag.jsx":"6653f7ac7274","components/feedback/Dialog.jsx":"7a9895323268","components/feedback/Toast.jsx":"cb6ddce92bf9","components/feedback/Tooltip.jsx":"7083b3b0445a","components/forms/Checkbox.jsx":"b07e5b6a22f5","components/forms/Input.jsx":"d5ec152f7004","components/forms/Radio.jsx":"63b53bdef0e9","components/forms/Select.jsx":"5bb3821f6b0f","components/forms/Switch.jsx":"fb8352617b68","components/navigation/Tabs.jsx":"794a4868a73c","ui_kits/parts-shop/CartDrawer.jsx":"ee7134729e3d","ui_kits/parts-shop/CollectionScreen.jsx":"a142f77a223a","ui_kits/parts-shop/ProductCard.jsx":"2fd85f456473","ui_kits/parts-shop/ProductScreen.jsx":"64aca3a06a73","ui_kits/parts-shop/ShopChrome.jsx":"67666b76cd6e","ui_kits/parts-shop/StorefrontScreen.jsx":"5b6a67121b99","ui_kits/parts-shop/products.js":"7d6fca821c3c","ui_kits/website/BookingScreen.jsx":"b1758527e170","ui_kits/website/Chrome.jsx":"23393370a777","ui_kits/website/DismantlingScreen.jsx":"14d76fde9ddd","ui_kits/website/HomeScreen.jsx":"eb48b2ae53d5","ui_kits/website/ServicingScreen.jsx":"807da3f6eb5b"},"inlinedExternals":[],"unexposedExports":[]} */

(() => {

const __ds_ns = (window.SDEuropeanDesignSystem_614674 = window.SDEuropeanDesignSystem_614674 || {});

const __ds_scope = {};

(__ds_ns.__errors = __ds_ns.__errors || []);

// components/brand/Eyebrow.jsx
try { (() => {
function _extends() { return _extends = Object.assign ? Object.assign.bind() : function (n) { for (var e = 1; e < arguments.length; e++) { var t = arguments[e]; for (var r in t) ({}).hasOwnProperty.call(t, r) && (n[r] = t[r]); } return n; }, _extends.apply(null, arguments); }
function Eyebrow({
  children,
  tone = 'muted',
  as: Tag = 'div',
  style,
  ...rest
}) {
  const color = tone === 'accent' ? 'var(--brand-yellow)' : tone === 'inverse' ? 'rgba(255,255,255,.72)' : 'var(--text-secondary)';
  return /*#__PURE__*/React.createElement(Tag, _extends({
    style: {
      font: 'var(--type-eyebrow)',
      letterSpacing: 'var(--ls-widest)',
      textTransform: 'uppercase',
      color,
      ...style
    }
  }, rest), children);
}
Object.assign(__ds_scope, { Eyebrow });
})(); } catch (e) { __ds_ns.__errors.push({ path: "components/brand/Eyebrow.jsx", error: String((e && e.message) || e) }); }

// components/brand/Logo.jsx
try { (() => {
function _extends() { return _extends = Object.assign ? Object.assign.bind() : function (n) { for (var e = 1; e < arguments.length; e++) { var t = arguments[e]; for (var r in t) ({}).hasOwnProperty.call(t, r) && (n[r] = t[r]); } return n; }, _extends.apply(null, arguments); }
const SRC = {
  'lockup-black': 'logo-horizontal-black.png',
  'lockup-white': 'logo-horizontal-white.png',
  'symbol': 'symbol-black-yellow.png'
};
function Logo({
  variant = 'lockup-black',
  height = 40,
  assetBase = '/assets',
  href,
  className,
  style,
  ...rest
}) {
  const isSymbol = variant === 'symbol';
  const img = /*#__PURE__*/React.createElement("img", {
    src: assetBase + '/' + SRC[variant],
    alt: "SD European",
    style: {
      height: height + 'px',
      width: 'auto',
      display: 'block'
    }
  });
  const wrap = {
    display: 'inline-block',
    padding: isSymbol ? 0 : `0 ${Math.round(height * 0.18)}px`,
    border: 0,
    ...style
  };
  if (href) return /*#__PURE__*/React.createElement("a", _extends({
    href: href,
    className: className,
    style: {
      ...wrap,
      borderBottom: 0
    }
  }, rest), img);
  return /*#__PURE__*/React.createElement("span", _extends({
    className: className,
    style: wrap
  }, rest), img);
}
Object.assign(__ds_scope, { Logo });
})(); } catch (e) { __ds_ns.__errors.push({ path: "components/brand/Logo.jsx", error: String((e && e.message) || e) }); }

// components/brand/SectionHeading.jsx
try { (() => {
function _extends() { return _extends = Object.assign ? Object.assign.bind() : function (n) { for (var e = 1; e < arguments.length; e++) { var t = arguments[e]; for (var r in t) ({}).hasOwnProperty.call(t, r) && (n[r] = t[r]); } return n; }, _extends.apply(null, arguments); }
function SectionHeading({
  eyebrow,
  title,
  lead,
  level = 2,
  align = 'left',
  inverse = false,
  rule = true,
  style,
  ...rest
}) {
  const H = 'h' + level;
  return /*#__PURE__*/React.createElement("header", _extends({
    style: {
      display: 'grid',
      gap: 'var(--space-3)',
      justifyItems: align === 'center' ? 'center' : 'start',
      textAlign: align,
      maxWidth: '760px',
      ...style
    }
  }, rest), eyebrow ? /*#__PURE__*/React.createElement(__ds_scope.Eyebrow, {
    tone: inverse ? 'inverse' : 'muted'
  }, eyebrow) : null, /*#__PURE__*/React.createElement(H, {
    style: {
      font: level === 1 ? 'var(--type-h1)' : level === 2 ? 'var(--type-h2)' : 'var(--type-h3)',
      letterSpacing: 'var(--ls-tight)',
      color: inverse ? 'var(--text-inverse)' : 'var(--text-primary)',
      margin: 0
    }
  }, title), rule ? /*#__PURE__*/React.createElement("hr", {
    style: {
      height: '4px',
      width: '56px',
      background: 'var(--brand-yellow)',
      border: 0,
      margin: 'var(--space-1) 0 0'
    }
  }) : null, lead ? /*#__PURE__*/React.createElement("p", {
    style: {
      font: 'var(--type-lead)',
      color: inverse ? 'rgba(255,255,255,.78)' : 'var(--text-secondary)',
      margin: 'var(--space-2) 0 0'
    }
  }, lead) : null);
}
Object.assign(__ds_scope, { SectionHeading });
})(); } catch (e) { __ds_ns.__errors.push({ path: "components/brand/SectionHeading.jsx", error: String((e && e.message) || e) }); }

// components/core/Badge.jsx
try { (() => {
function _extends() { return _extends = Object.assign ? Object.assign.bind() : function (n) { for (var e = 1; e < arguments.length; e++) { var t = arguments[e]; for (var r in t) ({}).hasOwnProperty.call(t, r) && (n[r] = t[r]); } return n; }, _extends.apply(null, arguments); }
const TONES = {
  neutral: ['var(--neutral-100)', 'var(--neutral-700)'],
  accent: ['var(--brand-yellow)', 'var(--brand-black)'],
  dark: ['var(--brand-black)', 'var(--brand-white)'],
  success: ['var(--status-success-surface)', 'var(--status-success)'],
  warning: ['var(--status-warning-surface)', 'var(--status-warning)'],
  danger: ['var(--status-danger-surface)', 'var(--status-danger)'],
  info: ['var(--status-info-surface)', 'var(--status-info)']
};
function Badge({
  children,
  tone = 'neutral',
  size = 'md',
  dot = false,
  style,
  ...rest
}) {
  const [bg, fg] = TONES[tone];
  return /*#__PURE__*/React.createElement("span", _extends({
    style: {
      display: 'inline-flex',
      alignItems: 'center',
      gap: '6px',
      background: bg,
      color: fg,
      borderRadius: 'var(--radius-pill)',
      padding: size === 'sm' ? '2px 8px' : '4px 12px',
      fontFamily: 'var(--font-body)',
      fontWeight: 'var(--fw-semibold)',
      fontSize: size === 'sm' ? 'var(--fs-2xs)' : 'var(--fs-xs)',
      letterSpacing: 'var(--ls-wide)',
      textTransform: 'uppercase',
      whiteSpace: 'nowrap',
      ...style
    }
  }, rest), dot ? /*#__PURE__*/React.createElement("span", {
    style: {
      width: '6px',
      height: '6px',
      borderRadius: '50%',
      background: 'currentColor'
    }
  }) : null, children);
}
Object.assign(__ds_scope, { Badge });
})(); } catch (e) { __ds_ns.__errors.push({ path: "components/core/Badge.jsx", error: String((e && e.message) || e) }); }

// components/core/Button.jsx
try { (() => {
function _extends() { return _extends = Object.assign ? Object.assign.bind() : function (n) { for (var e = 1; e < arguments.length; e++) { var t = arguments[e]; for (var r in t) ({}).hasOwnProperty.call(t, r) && (n[r] = t[r]); } return n; }, _extends.apply(null, arguments); }
const SIZES = {
  sm: {
    padding: '0 14px',
    height: '34px',
    fontSize: '12px'
  },
  md: {
    padding: '0 20px',
    height: '44px',
    fontSize: '14px'
  },
  lg: {
    padding: '0 28px',
    height: '54px',
    fontSize: '16px'
  }
};
const TONES = {
  primary: {
    background: 'var(--brand-yellow)',
    color: 'var(--brand-black)',
    border: '1px solid var(--brand-yellow)'
  },
  secondary: {
    background: 'var(--brand-black)',
    color: 'var(--brand-white)',
    border: '1px solid var(--brand-black)'
  },
  outline: {
    background: 'transparent',
    color: 'var(--brand-black)',
    border: '1px solid var(--neutral-300)'
  },
  ghost: {
    background: 'transparent',
    color: 'var(--brand-black)',
    border: '1px solid transparent'
  },
  inverse: {
    background: 'var(--brand-white)',
    color: 'var(--brand-black)',
    border: '1px solid var(--brand-white)'
  }
};
const HOVER = {
  primary: {
    background: 'var(--yellow-500)',
    borderColor: 'var(--yellow-500)'
  },
  secondary: {
    background: 'var(--neutral-700)',
    borderColor: 'var(--neutral-700)'
  },
  outline: {
    background: 'var(--neutral-100)',
    borderColor: 'var(--neutral-400)'
  },
  ghost: {
    background: 'var(--neutral-100)'
  },
  inverse: {
    background: 'var(--neutral-200)',
    borderColor: 'var(--neutral-200)'
  }
};
function Button({
  children,
  variant = 'primary',
  size = 'md',
  iconLeft,
  iconRight,
  fullWidth = false,
  disabled = false,
  as,
  href,
  style,
  ...rest
}) {
  const [h, setH] = React.useState(false);
  const [p, setP] = React.useState(false);
  const Tag = as || (href ? 'a' : 'button');
  const base = {
    display: 'inline-flex',
    alignItems: 'center',
    justifyContent: 'center',
    gap: 'var(--space-2)',
    width: fullWidth ? '100%' : undefined,
    fontFamily: 'var(--font-display)',
    fontWeight: 'var(--fw-bold)',
    letterSpacing: '0.02em',
    borderRadius: 'var(--radius-sm)',
    cursor: disabled ? 'not-allowed' : 'pointer',
    opacity: disabled ? 0.45 : 1,
    textDecoration: 'none',
    whiteSpace: 'nowrap',
    transition: 'var(--transition-control), transform var(--dur-instant) var(--ease-standard)',
    transform: p && !disabled ? 'var(--press-scale)' : 'none',
    ...SIZES[size],
    ...TONES[variant],
    ...(h && !disabled ? HOVER[variant] : null),
    ...style
  };
  return /*#__PURE__*/React.createElement(Tag, _extends({
    href: href,
    disabled: Tag === 'button' ? disabled : undefined,
    style: base,
    onMouseEnter: () => setH(true),
    onMouseLeave: () => {
      setH(false);
      setP(false);
    },
    onMouseDown: () => setP(true),
    onMouseUp: () => setP(false)
  }, rest), iconLeft, children, iconRight);
}
Object.assign(__ds_scope, { Button });
})(); } catch (e) { __ds_ns.__errors.push({ path: "components/core/Button.jsx", error: String((e && e.message) || e) }); }

// components/core/Card.jsx
try { (() => {
function _extends() { return _extends = Object.assign ? Object.assign.bind() : function (n) { for (var e = 1; e < arguments.length; e++) { var t = arguments[e]; for (var r in t) ({}).hasOwnProperty.call(t, r) && (n[r] = t[r]); } return n; }, _extends.apply(null, arguments); }
function Card({
  children,
  media,
  mediaAlt = '',
  title,
  meta,
  footer,
  variant = 'default',
  interactive = false,
  padding = 'var(--space-6)',
  style,
  ...rest
}) {
  const [h, setH] = React.useState(false);
  const tones = {
    default: {
      background: 'var(--surface-card)',
      border: '1px solid var(--border-default)',
      color: 'var(--text-primary)'
    },
    subtle: {
      background: 'var(--surface-subtle)',
      border: '1px solid transparent',
      color: 'var(--text-primary)'
    },
    inverse: {
      background: 'var(--surface-inverse)',
      border: '1px solid var(--border-inverse)',
      color: 'var(--text-inverse)'
    },
    accent: {
      background: 'var(--surface-accent)',
      border: '1px solid var(--brand-yellow)',
      color: 'var(--brand-black)'
    }
  }[variant];
  return /*#__PURE__*/React.createElement("div", _extends({
    onMouseEnter: () => setH(true),
    onMouseLeave: () => setH(false),
    style: {
      borderRadius: media ? 'var(--radius-lg)' : 'var(--radius-md)',
      overflow: 'hidden',
      display: 'flex',
      flexDirection: 'column',
      transition: 'var(--transition-surface), border-color var(--dur-fast) var(--ease-standard)',
      transform: interactive && h ? 'var(--hover-lift)' : 'none',
      boxShadow: interactive && h ? 'var(--shadow-sm)' : 'var(--shadow-none)',
      cursor: interactive ? 'pointer' : undefined,
      ...tones,
      ...style
    }
  }, rest), media ? /*#__PURE__*/React.createElement("img", {
    src: media,
    alt: mediaAlt,
    style: {
      width: '100%',
      aspectRatio: '4 / 3',
      objectFit: 'cover',
      background: 'var(--neutral-100)'
    }
  }) : null, /*#__PURE__*/React.createElement("div", {
    style: {
      padding,
      display: 'flex',
      flexDirection: 'column',
      gap: 'var(--space-2)',
      flex: 1
    }
  }, meta ? /*#__PURE__*/React.createElement("div", {
    style: {
      font: 'var(--type-label)',
      letterSpacing: 'var(--ls-wide)',
      textTransform: 'uppercase',
      color: variant === 'inverse' ? 'rgba(255,255,255,.6)' : 'var(--text-muted)'
    }
  }, meta) : null, title ? /*#__PURE__*/React.createElement("h3", {
    style: {
      font: 'var(--type-h4)',
      color: 'inherit',
      margin: 0
    }
  }, title) : null, children ? /*#__PURE__*/React.createElement("div", {
    style: {
      font: 'var(--type-body-sm)',
      color: variant === 'inverse' ? 'rgba(255,255,255,.75)' : 'var(--text-secondary)'
    }
  }, children) : null, footer ? /*#__PURE__*/React.createElement("div", {
    style: {
      marginTop: 'auto',
      paddingTop: 'var(--space-4)'
    }
  }, footer) : null));
}
Object.assign(__ds_scope, { Card });
})(); } catch (e) { __ds_ns.__errors.push({ path: "components/core/Card.jsx", error: String((e && e.message) || e) }); }

// components/core/Icon.jsx
try { (() => {
function _extends() { return _extends = Object.assign ? Object.assign.bind() : function (n) { for (var e = 1; e < arguments.length; e++) { var t = arguments[e]; for (var r in t) ({}).hasOwnProperty.call(t, r) && (n[r] = t[r]); } return n; }, _extends.apply(null, arguments); }
/**
 * Thin wrapper over Lucide (loaded from CDN as window.lucide).
 * Enforces the brand rules: stroke-only, currentColor, capped size scale.
 */
function Icon({
  name,
  size = 20,
  strokeWidth = 2,
  label,
  style,
  ...rest
}) {
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
          attrs: {
            width: size,
            height: size,
            'stroke-width': strokeWidth,
            stroke: 'currentColor'
          },
          root: host
        });
      } catch (e) {
        return false;
      }
      return !!host.querySelector('svg');
    };
    if (!draw()) {
      let tries = 0;
      const id = setInterval(() => {
        if (draw() || ++tries > 40) clearInterval(id);
      }, 100);
      return () => {
        cancelled = true;
        clearInterval(id);
      };
    }
    return () => {
      cancelled = true;
    };
  }, [name, size, strokeWidth]);
  return /*#__PURE__*/React.createElement("span", _extends({
    ref: ref,
    role: label ? 'img' : undefined,
    "aria-label": label,
    "aria-hidden": label ? undefined : true,
    "data-icon": name,
    style: {
      display: 'inline-flex',
      alignItems: 'center',
      justifyContent: 'center',
      width: size + 'px',
      height: size + 'px',
      flex: '0 0 auto',
      color: 'inherit',
      ...style
    }
  }, rest));
}
Object.assign(__ds_scope, { Icon });
})(); } catch (e) { __ds_ns.__errors.push({ path: "components/core/Icon.jsx", error: String((e && e.message) || e) }); }

// components/core/IconButton.jsx
try { (() => {
function _extends() { return _extends = Object.assign ? Object.assign.bind() : function (n) { for (var e = 1; e < arguments.length; e++) { var t = arguments[e]; for (var r in t) ({}).hasOwnProperty.call(t, r) && (n[r] = t[r]); } return n; }, _extends.apply(null, arguments); }
const S = {
  sm: 32,
  md: 40,
  lg: 48
};
function IconButton({
  icon,
  label,
  variant = 'ghost',
  size = 'md',
  shape = 'circle',
  disabled = false,
  style,
  ...rest
}) {
  const [h, setH] = React.useState(false);
  const tones = {
    ghost: {
      background: 'transparent',
      color: 'var(--brand-black)',
      border: '1px solid transparent',
      hover: 'var(--neutral-100)'
    },
    solid: {
      background: 'var(--brand-black)',
      color: 'var(--brand-white)',
      border: '1px solid var(--brand-black)',
      hover: 'var(--neutral-700)'
    },
    accent: {
      background: 'var(--brand-yellow)',
      color: 'var(--brand-black)',
      border: '1px solid var(--brand-yellow)',
      hover: 'var(--yellow-500)'
    },
    outline: {
      background: 'var(--brand-white)',
      color: 'var(--brand-black)',
      border: '1px solid var(--neutral-300)',
      hover: 'var(--neutral-100)'
    }
  }[variant];
  return /*#__PURE__*/React.createElement("button", _extends({
    "aria-label": label,
    disabled: disabled,
    onMouseEnter: () => setH(true),
    onMouseLeave: () => setH(false),
    style: {
      width: S[size] + 'px',
      height: S[size] + 'px',
      display: 'inline-flex',
      alignItems: 'center',
      justifyContent: 'center',
      borderRadius: shape === 'circle' ? 'var(--radius-circle)' : 'var(--radius-sm)',
      background: h && !disabled ? tones.hover : tones.background,
      color: tones.color,
      border: tones.border,
      cursor: disabled ? 'not-allowed' : 'pointer',
      opacity: disabled ? 0.45 : 1,
      padding: 0,
      transition: 'var(--transition-control)',
      ...style
    }
  }, rest), icon);
}
Object.assign(__ds_scope, { IconButton });
})(); } catch (e) { __ds_ns.__errors.push({ path: "components/core/IconButton.jsx", error: String((e && e.message) || e) }); }

// components/core/Tag.jsx
try { (() => {
function _extends() { return _extends = Object.assign ? Object.assign.bind() : function (n) { for (var e = 1; e < arguments.length; e++) { var t = arguments[e]; for (var r in t) ({}).hasOwnProperty.call(t, r) && (n[r] = t[r]); } return n; }, _extends.apply(null, arguments); }
function Tag({
  children,
  selected = false,
  onRemove,
  onClick,
  style,
  ...rest
}) {
  const [h, setH] = React.useState(false);
  return /*#__PURE__*/React.createElement("span", _extends({
    onClick: onClick,
    onMouseEnter: () => setH(true),
    onMouseLeave: () => setH(false),
    style: {
      display: 'inline-flex',
      alignItems: 'center',
      gap: 'var(--space-2)',
      padding: '6px 12px',
      borderRadius: 'var(--radius-sm)',
      border: '1px solid ' + (selected ? 'var(--brand-black)' : 'var(--border-default)'),
      background: selected ? 'var(--brand-black)' : h && onClick ? 'var(--neutral-100)' : 'var(--brand-white)',
      color: selected ? 'var(--brand-white)' : 'var(--text-primary)',
      font: 'var(--type-body-sm)',
      fontWeight: 'var(--fw-semibold)',
      cursor: onClick ? 'pointer' : 'default',
      transition: 'var(--transition-control)',
      ...style
    }
  }, rest), children, onRemove ? /*#__PURE__*/React.createElement("button", {
    onClick: e => {
      e.stopPropagation();
      onRemove(e);
    },
    "aria-label": "Remove",
    style: {
      border: 0,
      background: 'transparent',
      color: 'inherit',
      cursor: 'pointer',
      padding: 0,
      lineHeight: 1,
      fontSize: '15px'
    }
  }, "\xD7") : null);
}
Object.assign(__ds_scope, { Tag });
})(); } catch (e) { __ds_ns.__errors.push({ path: "components/core/Tag.jsx", error: String((e && e.message) || e) }); }

// components/feedback/Dialog.jsx
try { (() => {
function _extends() { return _extends = Object.assign ? Object.assign.bind() : function (n) { for (var e = 1; e < arguments.length; e++) { var t = arguments[e]; for (var r in t) ({}).hasOwnProperty.call(t, r) && (n[r] = t[r]); } return n; }, _extends.apply(null, arguments); }
function Dialog({
  open,
  title,
  description,
  children,
  footer,
  onClose,
  width = 520,
  style,
  ...rest
}) {
  React.useEffect(() => {
    if (!open) return;
    const k = e => {
      if (e.key === 'Escape' && onClose) onClose();
    };
    window.addEventListener('keydown', k);
    return () => window.removeEventListener('keydown', k);
  }, [open, onClose]);
  if (!open) return null;
  return /*#__PURE__*/React.createElement("div", {
    onClick: onClose,
    style: {
      position: 'fixed',
      inset: 0,
      background: 'var(--surface-overlay)',
      display: 'grid',
      placeItems: 'center',
      padding: 'var(--space-6)',
      zIndex: 1000,
      animation: 'none'
    }
  }, /*#__PURE__*/React.createElement("div", _extends({
    role: "dialog",
    "aria-modal": "true",
    "aria-label": typeof title === 'string' ? title : undefined,
    onClick: e => e.stopPropagation(),
    style: {
      width: '100%',
      maxWidth: width + 'px',
      background: 'var(--surface-card)',
      borderRadius: 'var(--radius-md)',
      boxShadow: 'var(--shadow-overlay)',
      overflow: 'hidden',
      ...style
    }
  }, rest), /*#__PURE__*/React.createElement("div", {
    style: {
      padding: 'var(--space-6)',
      display: 'grid',
      gap: 'var(--space-3)'
    }
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      display: 'flex',
      alignItems: 'start',
      gap: 'var(--space-4)'
    }
  }, /*#__PURE__*/React.createElement("h2", {
    style: {
      font: 'var(--type-h3)',
      margin: 0,
      flex: 1
    }
  }, title), /*#__PURE__*/React.createElement("button", {
    onClick: onClose,
    "aria-label": "Close",
    style: {
      border: 0,
      background: 'transparent',
      fontSize: '22px',
      lineHeight: 1,
      cursor: 'pointer',
      color: 'var(--text-muted)'
    }
  }, "\xD7")), description ? /*#__PURE__*/React.createElement("p", {
    style: {
      font: 'var(--type-body)',
      color: 'var(--text-secondary)'
    }
  }, description) : null, children), footer ? /*#__PURE__*/React.createElement("div", {
    style: {
      padding: 'var(--space-4) var(--space-6)',
      background: 'var(--surface-subtle)',
      display: 'flex',
      justifyContent: 'flex-end',
      gap: 'var(--space-3)'
    }
  }, footer) : null));
}
Object.assign(__ds_scope, { Dialog });
})(); } catch (e) { __ds_ns.__errors.push({ path: "components/feedback/Dialog.jsx", error: String((e && e.message) || e) }); }

// components/feedback/Toast.jsx
try { (() => {
function _extends() { return _extends = Object.assign ? Object.assign.bind() : function (n) { for (var e = 1; e < arguments.length; e++) { var t = arguments[e]; for (var r in t) ({}).hasOwnProperty.call(t, r) && (n[r] = t[r]); } return n; }, _extends.apply(null, arguments); }
const TONES = {
  neutral: {
    bar: 'var(--brand-yellow)',
    fg: 'var(--brand-white)',
    bg: 'var(--brand-black)'
  },
  success: {
    bar: 'var(--status-success)',
    fg: 'var(--brand-white)',
    bg: 'var(--brand-black)'
  },
  warning: {
    bar: 'var(--status-warning)',
    fg: 'var(--brand-white)',
    bg: 'var(--brand-black)'
  },
  danger: {
    bar: 'var(--status-danger)',
    fg: 'var(--brand-white)',
    bg: 'var(--brand-black)'
  }
};
function Toast({
  title,
  message,
  tone = 'neutral',
  action,
  onDismiss,
  icon,
  style,
  ...rest
}) {
  const t = TONES[tone];
  return /*#__PURE__*/React.createElement("div", _extends({
    role: "status",
    style: {
      display: 'flex',
      alignItems: 'center',
      gap: 'var(--space-4)',
      minWidth: '320px',
      maxWidth: '440px',
      background: t.bg,
      color: t.fg,
      borderRadius: 'var(--radius-md)',
      boxShadow: 'var(--shadow-lg)',
      borderLeft: '4px solid ' + t.bar,
      padding: 'var(--space-4) var(--space-5)',
      ...style
    }
  }, rest), icon, /*#__PURE__*/React.createElement("div", {
    style: {
      flex: 1,
      display: 'grid',
      gap: '2px'
    }
  }, /*#__PURE__*/React.createElement("strong", {
    style: {
      font: 'var(--type-h4)',
      fontSize: 'var(--fs-sm)',
      color: 'inherit'
    }
  }, title), message ? /*#__PURE__*/React.createElement("span", {
    style: {
      font: 'var(--type-body-sm)',
      color: 'rgba(255,255,255,.72)'
    }
  }, message) : null), action, onDismiss ? /*#__PURE__*/React.createElement("button", {
    onClick: onDismiss,
    "aria-label": "Dismiss",
    style: {
      border: 0,
      background: 'transparent',
      color: 'rgba(255,255,255,.65)',
      fontSize: '20px',
      lineHeight: 1,
      cursor: 'pointer'
    }
  }, "\xD7") : null);
}
Object.assign(__ds_scope, { Toast });
})(); } catch (e) { __ds_ns.__errors.push({ path: "components/feedback/Toast.jsx", error: String((e && e.message) || e) }); }

// components/feedback/Tooltip.jsx
try { (() => {
function _extends() { return _extends = Object.assign ? Object.assign.bind() : function (n) { for (var e = 1; e < arguments.length; e++) { var t = arguments[e]; for (var r in t) ({}).hasOwnProperty.call(t, r) && (n[r] = t[r]); } return n; }, _extends.apply(null, arguments); }
function Tooltip({
  label,
  placement = 'top',
  children,
  style,
  ...rest
}) {
  const [on, setOn] = React.useState(false);
  const pos = {
    top: {
      bottom: '100%',
      left: '50%',
      transform: 'translate(-50%,-8px)'
    },
    bottom: {
      top: '100%',
      left: '50%',
      transform: 'translate(-50%,8px)'
    },
    left: {
      right: '100%',
      top: '50%',
      transform: 'translate(-8px,-50%)'
    },
    right: {
      left: '100%',
      top: '50%',
      transform: 'translate(8px,-50%)'
    }
  }[placement];
  return /*#__PURE__*/React.createElement("span", _extends({
    style: {
      position: 'relative',
      display: 'inline-flex',
      ...style
    },
    onMouseEnter: () => setOn(true),
    onMouseLeave: () => setOn(false),
    onFocus: () => setOn(true),
    onBlur: () => setOn(false)
  }, rest), children, /*#__PURE__*/React.createElement("span", {
    role: "tooltip",
    style: {
      position: 'absolute',
      ...pos,
      zIndex: 20,
      pointerEvents: 'none',
      opacity: on ? 1 : 0,
      visibility: on ? 'visible' : 'hidden',
      transition: 'opacity var(--dur-fast) var(--ease-standard)',
      background: 'var(--brand-black)',
      color: 'var(--brand-white)',
      font: 'var(--type-body-sm)',
      padding: '6px 10px',
      borderRadius: 'var(--radius-sm)',
      whiteSpace: 'nowrap',
      boxShadow: 'var(--shadow-md)'
    }
  }, label));
}
Object.assign(__ds_scope, { Tooltip });
})(); } catch (e) { __ds_ns.__errors.push({ path: "components/feedback/Tooltip.jsx", error: String((e && e.message) || e) }); }

// components/forms/Checkbox.jsx
try { (() => {
function _extends() { return _extends = Object.assign ? Object.assign.bind() : function (n) { for (var e = 1; e < arguments.length; e++) { var t = arguments[e]; for (var r in t) ({}).hasOwnProperty.call(t, r) && (n[r] = t[r]); } return n; }, _extends.apply(null, arguments); }
function Checkbox({
  label,
  description,
  checked,
  defaultChecked,
  disabled,
  onChange,
  id,
  style,
  ...rest
}) {
  const auto = React.useId ? React.useId() : 'cb';
  const cbId = id || auto;
  const [inner, setInner] = React.useState(!!defaultChecked);
  const on = checked !== undefined ? checked : inner;
  return /*#__PURE__*/React.createElement("label", {
    htmlFor: cbId,
    style: {
      display: 'grid',
      gridTemplateColumns: '20px 1fr',
      gap: 'var(--space-3)',
      alignItems: 'start',
      cursor: disabled ? 'not-allowed' : 'pointer',
      opacity: disabled ? 0.45 : 1,
      ...style
    }
  }, /*#__PURE__*/React.createElement("input", _extends({
    id: cbId,
    type: "checkbox",
    checked: on,
    disabled: disabled,
    onChange: e => {
      if (checked === undefined) setInner(e.target.checked);
      onChange && onChange(e);
    },
    style: {
      position: 'absolute',
      opacity: 0,
      width: 0,
      height: 0
    }
  }, rest)), /*#__PURE__*/React.createElement("span", {
    "aria-hidden": "true",
    style: {
      width: '20px',
      height: '20px',
      marginTop: '2px',
      borderRadius: 'var(--radius-xs)',
      border: '1px solid ' + (on ? 'var(--brand-black)' : 'var(--border-strong)'),
      background: on ? 'var(--brand-black)' : 'var(--brand-white)',
      display: 'inline-flex',
      alignItems: 'center',
      justifyContent: 'center',
      transition: 'var(--transition-control)'
    }
  }, on ? /*#__PURE__*/React.createElement("span", {
    style: {
      width: '5px',
      height: '9px',
      borderRight: '2px solid var(--brand-yellow)',
      borderBottom: '2px solid var(--brand-yellow)',
      transform: 'rotate(45deg) translate(-1px,-1px)'
    }
  }) : null), /*#__PURE__*/React.createElement("span", {
    style: {
      display: 'grid',
      gap: '2px'
    }
  }, /*#__PURE__*/React.createElement("span", {
    style: {
      font: 'var(--type-body)',
      color: 'var(--text-primary)'
    }
  }, label), description ? /*#__PURE__*/React.createElement("span", {
    style: {
      font: 'var(--type-body-sm)',
      color: 'var(--text-muted)'
    }
  }, description) : null));
}
Object.assign(__ds_scope, { Checkbox });
})(); } catch (e) { __ds_ns.__errors.push({ path: "components/forms/Checkbox.jsx", error: String((e && e.message) || e) }); }

// components/forms/Input.jsx
try { (() => {
function _extends() { return _extends = Object.assign ? Object.assign.bind() : function (n) { for (var e = 1; e < arguments.length; e++) { var t = arguments[e]; for (var r in t) ({}).hasOwnProperty.call(t, r) && (n[r] = t[r]); } return n; }, _extends.apply(null, arguments); }
function Input({
  label,
  hint,
  error,
  prefix,
  suffix,
  id,
  size = 'md',
  mono = false,
  style,
  ...rest
}) {
  const auto = React.useId ? React.useId() : 'in';
  const inputId = id || auto;
  const [focus, setFocus] = React.useState(false);
  const h = size === 'sm' ? 36 : size === 'lg' ? 54 : 44;
  return /*#__PURE__*/React.createElement("div", {
    style: {
      display: 'grid',
      gap: 'var(--space-2)',
      ...style
    }
  }, label ? /*#__PURE__*/React.createElement("label", {
    htmlFor: inputId,
    style: {
      font: 'var(--type-label)',
      letterSpacing: 'var(--ls-wide)',
      textTransform: 'uppercase',
      color: 'var(--text-secondary)'
    }
  }, label) : null, /*#__PURE__*/React.createElement("div", {
    style: {
      display: 'flex',
      alignItems: 'center',
      gap: 'var(--space-2)',
      height: h + 'px',
      padding: '0 12px',
      background: 'var(--brand-white)',
      borderRadius: 'var(--radius-sm)',
      border: '1px solid ' + (error ? 'var(--status-danger)' : focus ? 'var(--brand-black)' : 'var(--border-default)'),
      boxShadow: focus && !error ? 'var(--ring-focus)' : 'none',
      transition: 'var(--transition-control)',
      color: 'var(--text-muted)'
    }
  }, prefix, /*#__PURE__*/React.createElement("input", _extends({
    id: inputId,
    onFocus: () => setFocus(true),
    onBlur: () => setFocus(false),
    style: {
      flex: 1,
      minWidth: 0,
      border: 0,
      outline: 'none',
      background: 'transparent',
      font: mono ? 'var(--type-mono)' : 'var(--type-body)',
      color: 'var(--text-primary)',
      textTransform: mono ? 'uppercase' : 'none',
      letterSpacing: mono ? '0.06em' : 'normal'
    }
  }, rest)), suffix), error ? /*#__PURE__*/React.createElement("span", {
    style: {
      font: 'var(--type-body-sm)',
      color: 'var(--status-danger)'
    }
  }, error) : hint ? /*#__PURE__*/React.createElement("span", {
    style: {
      font: 'var(--type-body-sm)',
      color: 'var(--text-muted)'
    }
  }, hint) : null);
}
Object.assign(__ds_scope, { Input });
})(); } catch (e) { __ds_ns.__errors.push({ path: "components/forms/Input.jsx", error: String((e && e.message) || e) }); }

// components/forms/Radio.jsx
try { (() => {
function _extends() { return _extends = Object.assign ? Object.assign.bind() : function (n) { for (var e = 1; e < arguments.length; e++) { var t = arguments[e]; for (var r in t) ({}).hasOwnProperty.call(t, r) && (n[r] = t[r]); } return n; }, _extends.apply(null, arguments); }
function Radio({
  label,
  description,
  name,
  value,
  checked,
  disabled,
  onChange,
  id,
  style,
  ...rest
}) {
  const auto = React.useId ? React.useId() : 'rd';
  const rId = id || auto;
  return /*#__PURE__*/React.createElement("label", {
    htmlFor: rId,
    style: {
      display: 'grid',
      gridTemplateColumns: '20px 1fr',
      gap: 'var(--space-3)',
      alignItems: 'start',
      cursor: disabled ? 'not-allowed' : 'pointer',
      opacity: disabled ? 0.45 : 1,
      ...style
    }
  }, /*#__PURE__*/React.createElement("input", _extends({
    id: rId,
    type: "radio",
    name: name,
    value: value,
    checked: checked,
    disabled: disabled,
    onChange: onChange,
    style: {
      position: 'absolute',
      opacity: 0,
      width: 0,
      height: 0
    }
  }, rest)), /*#__PURE__*/React.createElement("span", {
    "aria-hidden": "true",
    style: {
      width: '20px',
      height: '20px',
      marginTop: '2px',
      borderRadius: 'var(--radius-circle)',
      border: '1px solid ' + (checked ? 'var(--brand-black)' : 'var(--border-strong)'),
      background: 'var(--brand-white)',
      display: 'inline-flex',
      alignItems: 'center',
      justifyContent: 'center',
      transition: 'var(--transition-control)'
    }
  }, checked ? /*#__PURE__*/React.createElement("span", {
    style: {
      width: '10px',
      height: '10px',
      borderRadius: '50%',
      background: 'var(--brand-black)'
    }
  }) : null), /*#__PURE__*/React.createElement("span", {
    style: {
      display: 'grid',
      gap: '2px'
    }
  }, /*#__PURE__*/React.createElement("span", {
    style: {
      font: 'var(--type-body)',
      color: 'var(--text-primary)'
    }
  }, label), description ? /*#__PURE__*/React.createElement("span", {
    style: {
      font: 'var(--type-body-sm)',
      color: 'var(--text-muted)'
    }
  }, description) : null));
}
Object.assign(__ds_scope, { Radio });
})(); } catch (e) { __ds_ns.__errors.push({ path: "components/forms/Radio.jsx", error: String((e && e.message) || e) }); }

// components/forms/Select.jsx
try { (() => {
function _extends() { return _extends = Object.assign ? Object.assign.bind() : function (n) { for (var e = 1; e < arguments.length; e++) { var t = arguments[e]; for (var r in t) ({}).hasOwnProperty.call(t, r) && (n[r] = t[r]); } return n; }, _extends.apply(null, arguments); }
function Select({
  label,
  hint,
  error,
  options = [],
  id,
  size = 'md',
  style,
  ...rest
}) {
  const auto = React.useId ? React.useId() : 'sel';
  const selId = id || auto;
  const [focus, setFocus] = React.useState(false);
  const h = size === 'sm' ? 36 : size === 'lg' ? 54 : 44;
  return /*#__PURE__*/React.createElement("div", {
    style: {
      display: 'grid',
      gap: 'var(--space-2)',
      ...style
    }
  }, label ? /*#__PURE__*/React.createElement("label", {
    htmlFor: selId,
    style: {
      font: 'var(--type-label)',
      letterSpacing: 'var(--ls-wide)',
      textTransform: 'uppercase',
      color: 'var(--text-secondary)'
    }
  }, label) : null, /*#__PURE__*/React.createElement("div", {
    style: {
      position: 'relative'
    }
  }, /*#__PURE__*/React.createElement("select", _extends({
    id: selId,
    onFocus: () => setFocus(true),
    onBlur: () => setFocus(false),
    style: {
      width: '100%',
      height: h + 'px',
      padding: '0 40px 0 12px',
      appearance: 'none',
      background: 'var(--brand-white)',
      borderRadius: 'var(--radius-sm)',
      border: '1px solid ' + (error ? 'var(--status-danger)' : focus ? 'var(--brand-black)' : 'var(--border-default)'),
      boxShadow: focus && !error ? 'var(--ring-focus)' : 'none',
      font: 'var(--type-body)',
      color: 'var(--text-primary)',
      outline: 'none',
      transition: 'var(--transition-control)',
      cursor: 'pointer'
    }
  }, rest), options.map(o => {
    const v = typeof o === 'string' ? o : o.value;
    const l = typeof o === 'string' ? o : o.label;
    return /*#__PURE__*/React.createElement("option", {
      key: v,
      value: v
    }, l);
  })), /*#__PURE__*/React.createElement("span", {
    "aria-hidden": "true",
    style: {
      position: 'absolute',
      right: '14px',
      top: '50%',
      width: '9px',
      height: '9px',
      borderRight: '2px solid var(--brand-black)',
      borderBottom: '2px solid var(--brand-black)',
      transform: 'translateY(-70%) rotate(45deg)',
      pointerEvents: 'none'
    }
  })), error ? /*#__PURE__*/React.createElement("span", {
    style: {
      font: 'var(--type-body-sm)',
      color: 'var(--status-danger)'
    }
  }, error) : hint ? /*#__PURE__*/React.createElement("span", {
    style: {
      font: 'var(--type-body-sm)',
      color: 'var(--text-muted)'
    }
  }, hint) : null);
}
Object.assign(__ds_scope, { Select });
})(); } catch (e) { __ds_ns.__errors.push({ path: "components/forms/Select.jsx", error: String((e && e.message) || e) }); }

// components/forms/Switch.jsx
try { (() => {
function _extends() { return _extends = Object.assign ? Object.assign.bind() : function (n) { for (var e = 1; e < arguments.length; e++) { var t = arguments[e]; for (var r in t) ({}).hasOwnProperty.call(t, r) && (n[r] = t[r]); } return n; }, _extends.apply(null, arguments); }
function Switch({
  label,
  checked,
  defaultChecked,
  disabled,
  onChange,
  id,
  style,
  ...rest
}) {
  const auto = React.useId ? React.useId() : 'sw';
  const sId = id || auto;
  const [inner, setInner] = React.useState(!!defaultChecked);
  const on = checked !== undefined ? checked : inner;
  return /*#__PURE__*/React.createElement("label", {
    htmlFor: sId,
    style: {
      display: 'inline-flex',
      alignItems: 'center',
      gap: 'var(--space-3)',
      cursor: disabled ? 'not-allowed' : 'pointer',
      opacity: disabled ? 0.45 : 1,
      ...style
    }
  }, /*#__PURE__*/React.createElement("input", _extends({
    id: sId,
    type: "checkbox",
    role: "switch",
    checked: on,
    disabled: disabled,
    onChange: e => {
      if (checked === undefined) setInner(e.target.checked);
      onChange && onChange(e);
    },
    style: {
      position: 'absolute',
      opacity: 0,
      width: 0,
      height: 0
    }
  }, rest)), /*#__PURE__*/React.createElement("span", {
    "aria-hidden": "true",
    style: {
      width: '44px',
      height: '24px',
      borderRadius: 'var(--radius-pill)',
      padding: '2px',
      background: on ? 'var(--brand-black)' : 'var(--neutral-300)',
      display: 'inline-flex',
      alignItems: 'center',
      transition: 'background-color var(--dur-base) var(--ease-standard)'
    }
  }, /*#__PURE__*/React.createElement("span", {
    style: {
      width: '20px',
      height: '20px',
      borderRadius: '50%',
      background: on ? 'var(--brand-yellow)' : 'var(--brand-white)',
      transform: on ? 'translateX(20px)' : 'translateX(0)',
      transition: 'transform var(--dur-base) var(--ease-standard), background-color var(--dur-base) var(--ease-standard)',
      boxShadow: 'var(--shadow-xs)'
    }
  })), label ? /*#__PURE__*/React.createElement("span", {
    style: {
      font: 'var(--type-body)'
    }
  }, label) : null);
}
Object.assign(__ds_scope, { Switch });
})(); } catch (e) { __ds_ns.__errors.push({ path: "components/forms/Switch.jsx", error: String((e && e.message) || e) }); }

// components/navigation/Tabs.jsx
try { (() => {
function _extends() { return _extends = Object.assign ? Object.assign.bind() : function (n) { for (var e = 1; e < arguments.length; e++) { var t = arguments[e]; for (var r in t) ({}).hasOwnProperty.call(t, r) && (n[r] = t[r]); } return n; }, _extends.apply(null, arguments); }
function Tabs({
  items = [],
  value,
  defaultValue,
  onChange,
  variant = 'underline',
  style,
  ...rest
}) {
  const [inner, setInner] = React.useState(defaultValue ?? (items[0] && items[0].value));
  const active = value !== undefined ? value : inner;
  const pick = v => {
    if (value === undefined) setInner(v);
    onChange && onChange(v);
  };
  const underline = variant === 'underline';
  return /*#__PURE__*/React.createElement("div", _extends({
    role: "tablist",
    style: {
      display: 'flex',
      gap: underline ? 'var(--space-6)' : 'var(--space-2)',
      borderBottom: underline ? '1px solid var(--border-default)' : 'none',
      ...style
    }
  }, rest), items.map(it => {
    const on = it.value === active;
    return /*#__PURE__*/React.createElement("button", {
      key: it.value,
      role: "tab",
      "aria-selected": on,
      onClick: () => pick(it.value),
      style: {
        display: 'inline-flex',
        alignItems: 'center',
        gap: 'var(--space-2)',
        border: underline ? 0 : '1px solid ' + (on ? 'var(--brand-black)' : 'var(--border-default)'),
        borderBottom: underline ? '3px solid ' + (on ? 'var(--brand-yellow)' : 'transparent') : undefined,
        background: underline ? 'transparent' : on ? 'var(--brand-black)' : 'var(--brand-white)',
        color: underline ? on ? 'var(--text-primary)' : 'var(--text-muted)' : on ? 'var(--brand-white)' : 'var(--text-primary)',
        borderRadius: underline ? 0 : 'var(--radius-sm)',
        padding: underline ? '0 0 12px' : '8px 16px',
        marginBottom: underline ? '-1px' : 0,
        font: 'var(--type-button)',
        fontSize: 'var(--fs-sm)',
        letterSpacing: 'var(--ls-wide)',
        textTransform: 'uppercase',
        cursor: 'pointer',
        transition: 'var(--transition-control)'
      }
    }, it.icon, it.label, it.count !== undefined ? /*#__PURE__*/React.createElement("span", {
      style: {
        font: 'var(--type-body-sm)',
        opacity: .6
      }
    }, "(", it.count, ")") : null);
  }));
}
Object.assign(__ds_scope, { Tabs });
})(); } catch (e) { __ds_ns.__errors.push({ path: "components/navigation/Tabs.jsx", error: String((e && e.message) || e) }); }

// ui_kits/parts-shop/CartDrawer.jsx
try { (() => {
const {
  Button,
  Badge,
  Icon,
  IconButton,
  Eyebrow
} = window.SDEuropeanDesignSystem_614674 || {};
function CartDrawer({
  open,
  items,
  onClose,
  onRemove
}) {
  const subtotal = items.reduce((s, i) => s + i.price * i.qty, 0);
  const freeShip = subtotal >= 175;
  if (!open) return null;
  return /*#__PURE__*/React.createElement("div", {
    onClick: onClose,
    style: {
      position: 'fixed',
      inset: 0,
      background: 'var(--surface-overlay)',
      zIndex: 200,
      display: 'flex',
      justifyContent: 'flex-end'
    }
  }, /*#__PURE__*/React.createElement("aside", {
    onClick: e => e.stopPropagation(),
    style: {
      width: '420px',
      maxWidth: '100%',
      background: '#fff',
      height: '100%',
      display: 'flex',
      flexDirection: 'column',
      boxShadow: 'var(--shadow-overlay)'
    }
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      padding: 'var(--space-5) var(--space-6)',
      borderBottom: '1px solid var(--border-default)',
      display: 'flex',
      alignItems: 'center',
      gap: 'var(--space-3)'
    }
  }, /*#__PURE__*/React.createElement("h2", {
    style: {
      font: 'var(--type-h3)',
      fontSize: 'var(--fs-lg)',
      flex: 1
    }
  }, "Your Cart (", items.length, ")"), /*#__PURE__*/React.createElement(IconButton, {
    icon: /*#__PURE__*/React.createElement(Icon, {
      name: "x",
      size: 20
    }),
    label: "Close cart",
    onClick: onClose
  })), /*#__PURE__*/React.createElement("div", {
    style: {
      padding: 'var(--space-4) var(--space-6)',
      background: freeShip ? 'var(--status-success-surface)' : 'var(--surface-accent-subtle)',
      font: 'var(--type-body-sm)',
      color: freeShip ? 'var(--status-success)' : 'var(--text-primary)',
      display: 'flex',
      gap: '8px',
      alignItems: 'center'
    }
  }, /*#__PURE__*/React.createElement(Icon, {
    name: freeShip ? 'check' : 'truck',
    size: 16
  }), freeShip ? "You've got free shipping." : `Add $${(175 - subtotal).toFixed(2)} more for free shipping.`), /*#__PURE__*/React.createElement("div", {
    style: {
      flex: 1,
      overflow: 'auto',
      padding: 'var(--space-6)',
      display: 'grid',
      gap: 'var(--space-5)',
      alignContent: 'start'
    }
  }, items.length === 0 ? /*#__PURE__*/React.createElement("p", {
    style: {
      font: 'var(--type-body)',
      color: 'var(--text-muted)'
    }
  }, "Nothing in here yet. Browse the store or send us your rego and we'll find the part.") : null, items.map(i => /*#__PURE__*/React.createElement("div", {
    key: i.id,
    style: {
      display: 'grid',
      gridTemplateColumns: '72px 1fr auto',
      gap: 'var(--space-4)',
      alignItems: 'start'
    }
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      border: '1px solid var(--border-default)',
      borderRadius: 'var(--radius-sm)',
      overflow: 'hidden'
    }
  }, /*#__PURE__*/React.createElement(ShopPhoto, {
    label: ""
  })), /*#__PURE__*/React.createElement("div", {
    style: {
      display: 'grid',
      gap: '4px'
    }
  }, /*#__PURE__*/React.createElement("strong", {
    style: {
      font: 'var(--type-h4)',
      fontSize: 'var(--fs-sm)'
    }
  }, i.name), /*#__PURE__*/React.createElement("span", {
    style: {
      font: 'var(--type-body-sm)',
      color: 'var(--text-muted)'
    }
  }, i.fits), /*#__PURE__*/React.createElement("span", {
    style: {
      font: 'var(--type-mono)',
      fontSize: 'var(--fs-xs)',
      color: 'var(--text-muted)'
    }
  }, i.sku, " \xB7 Qty ", i.qty), /*#__PURE__*/React.createElement("button", {
    onClick: () => onRemove(i.id),
    style: {
      justifySelf: 'start',
      border: 0,
      background: 'none',
      padding: 0,
      cursor: 'pointer',
      font: 'var(--type-body-sm)',
      color: 'var(--text-muted)',
      textDecoration: 'underline'
    }
  }, "Remove")), /*#__PURE__*/React.createElement("strong", {
    style: {
      font: 'var(--type-body)'
    }
  }, "$", (i.price * i.qty).toFixed(2))))), /*#__PURE__*/React.createElement("div", {
    style: {
      borderTop: '1px solid var(--border-default)',
      padding: 'var(--space-6)',
      display: 'grid',
      gap: 'var(--space-4)'
    }
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      display: 'flex',
      justifyContent: 'space-between',
      font: 'var(--type-h3)',
      fontSize: 'var(--fs-lg)'
    }
  }, /*#__PURE__*/React.createElement("span", null, "Subtotal"), /*#__PURE__*/React.createElement("span", null, "$", subtotal.toFixed(2))), /*#__PURE__*/React.createElement("span", {
    style: {
      font: 'var(--type-body-sm)',
      color: 'var(--text-muted)'
    }
  }, "Shipping calculated at checkout. NZD, incl. GST."), /*#__PURE__*/React.createElement(Button, {
    size: "lg",
    fullWidth: true,
    disabled: items.length === 0
  }, "Checkout"), /*#__PURE__*/React.createElement(Button, {
    variant: "ghost",
    size: "sm",
    fullWidth: true,
    onClick: onClose
  }, "Continue Shopping"))));
}
Object.assign(window, {
  CartDrawer
});
})(); } catch (e) { __ds_ns.__errors.push({ path: "ui_kits/parts-shop/CartDrawer.jsx", error: String((e && e.message) || e) }); }

// ui_kits/parts-shop/CollectionScreen.jsx
try { (() => {
const {
  Button,
  Select,
  Checkbox,
  Tag,
  Eyebrow,
  Icon,
  Badge,
  Input
} = window.SDEuropeanDesignSystem_614674 || {};
function CollectionScreen({
  go,
  add
}) {
  const [chips, setChips] = React.useState(['BMW']);
  const toggle = c => setChips(s => s.includes(c) ? s.filter(x => x !== c) : [...s, c]);
  const products = window.SHOP_PRODUCTS;
  return /*#__PURE__*/React.createElement("main", {
    style: {
      maxWidth: 'var(--container-wide)',
      margin: '0 auto',
      padding: 'var(--space-8) var(--gutter-lg) var(--space-16)'
    }
  }, /*#__PURE__*/React.createElement("nav", {
    style: {
      font: 'var(--type-body-sm)',
      color: 'var(--text-muted)',
      marginBottom: 'var(--space-5)'
    }
  }, /*#__PURE__*/React.createElement("a", {
    href: "#",
    onClick: e => {
      e.preventDefault();
      go('home');
    },
    style: {
      borderBottom: 0,
      color: 'var(--text-muted)'
    }
  }, "Home"), " / Shop BMW / ", /*#__PURE__*/React.createElement("strong", {
    style: {
      color: 'var(--text-primary)'
    }
  }, "All Parts")), /*#__PURE__*/React.createElement("div", {
    style: {
      display: 'flex',
      alignItems: 'end',
      justifyContent: 'space-between',
      gap: 'var(--space-6)',
      paddingBottom: 'var(--space-5)',
      borderBottom: '1px solid var(--border-default)'
    }
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      display: 'grid',
      gap: 'var(--space-2)'
    }
  }, /*#__PURE__*/React.createElement(Eyebrow, null, "Collection"), /*#__PURE__*/React.createElement("h1", {
    style: {
      font: 'var(--type-h1)',
      fontSize: 'var(--fs-3xl)'
    }
  }, "Shop BMW Parts"), /*#__PURE__*/React.createElement("span", {
    style: {
      font: 'var(--type-body-sm)',
      color: 'var(--text-secondary)'
    }
  }, products.length, " of 4,182 parts \xB7 all in stock in New Zealand")), /*#__PURE__*/React.createElement("div", {
    style: {
      width: '240px'
    }
  }, /*#__PURE__*/React.createElement(Select, {
    label: "Sort by",
    size: "sm",
    options: ['Best selling', 'Price, low to high', 'Price, high to low', 'Newest']
  }))), /*#__PURE__*/React.createElement("div", {
    style: {
      display: 'grid',
      gridTemplateColumns: '260px 1fr',
      gap: 'var(--space-10)',
      marginTop: 'var(--space-8)',
      alignItems: 'start'
    }
  }, /*#__PURE__*/React.createElement("aside", {
    style: {
      display: 'grid',
      gap: 'var(--space-6)',
      position: 'sticky',
      top: '150px'
    }
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      display: 'grid',
      gap: 'var(--space-3)'
    }
  }, /*#__PURE__*/React.createElement(Eyebrow, null, "Check fitment"), /*#__PURE__*/React.createElement(Input, {
    mono: true,
    placeholder: "ABC123",
    prefix: /*#__PURE__*/React.createElement(Icon, {
      name: "car",
      size: 20
    }),
    "aria-label": "Rego"
  }), /*#__PURE__*/React.createElement(Button, {
    size: "sm",
    variant: "secondary",
    fullWidth: true
  }, "Check My Rego"), /*#__PURE__*/React.createElement("p", {
    style: {
      font: 'var(--type-body-sm)',
      color: 'var(--text-muted)'
    }
  }, "We manually check each order with your rego to be 100% sure it will fit.")), [['Make', ['Audi', 'BMW', 'Mercedes-Benz', 'Mini', 'Skoda', 'Volkswagen']], ['Condition', ['New aftermarket', 'OEM', 'Used']], ['Brand', ['Valeo', 'Topran', 'Mann-Filter', 'Hella', 'Arnott', 'Vemo & Vaico']]].map(([h, items]) => /*#__PURE__*/React.createElement("div", {
    key: h,
    style: {
      display: 'grid',
      gap: 'var(--space-3)',
      paddingTop: 'var(--space-5)',
      borderTop: '1px solid var(--border-default)'
    }
  }, /*#__PURE__*/React.createElement(Eyebrow, null, h), items.map(i => /*#__PURE__*/React.createElement(Checkbox, {
    key: i,
    label: i,
    checked: chips.includes(i),
    onChange: () => toggle(i)
  })))), /*#__PURE__*/React.createElement("div", {
    style: {
      display: 'grid',
      gap: 'var(--space-3)',
      paddingTop: 'var(--space-5)',
      borderTop: '1px solid var(--border-default)'
    }
  }, /*#__PURE__*/React.createElement(Eyebrow, null, "Availability"), /*#__PURE__*/React.createElement(Checkbox, {
    label: "In stock only",
    defaultChecked: true
  }))), /*#__PURE__*/React.createElement("div", null, chips.length ? /*#__PURE__*/React.createElement("div", {
    style: {
      display: 'flex',
      gap: 'var(--space-2)',
      flexWrap: 'wrap',
      marginBottom: 'var(--space-6)'
    }
  }, chips.map(c => /*#__PURE__*/React.createElement(Tag, {
    key: c,
    onRemove: () => toggle(c)
  }, c)), /*#__PURE__*/React.createElement("button", {
    onClick: () => setChips([]),
    style: {
      border: 0,
      background: 'none',
      cursor: 'pointer',
      font: 'var(--type-body-sm)',
      color: 'var(--text-muted)',
      textDecoration: 'underline'
    }
  }, "Clear all")) : null, /*#__PURE__*/React.createElement("div", {
    style: {
      display: 'grid',
      gridTemplateColumns: 'repeat(3,1fr)',
      gap: 'var(--space-6)'
    }
  }, products.map(p => /*#__PURE__*/React.createElement(ProductCard, {
    key: p.id,
    p: p,
    onOpen: () => go('product'),
    onAdd: add
  }))), /*#__PURE__*/React.createElement("div", {
    style: {
      display: 'flex',
      justifyContent: 'center',
      gap: 'var(--space-2)',
      marginTop: 'var(--space-10)'
    }
  }, ['1', '2', '3', '…', '47'].map((n, i) => /*#__PURE__*/React.createElement("button", {
    key: i,
    style: {
      minWidth: '40px',
      height: '40px',
      borderRadius: 'var(--radius-sm)',
      cursor: 'pointer',
      border: '1px solid ' + (i === 0 ? 'var(--brand-black)' : 'var(--border-default)'),
      background: i === 0 ? 'var(--brand-black)' : '#fff',
      color: i === 0 ? '#fff' : 'var(--text-primary)',
      font: 'var(--type-body-sm)',
      fontWeight: 'var(--fw-semibold)'
    }
  }, n))))));
}
Object.assign(window, {
  CollectionScreen
});
})(); } catch (e) { __ds_ns.__errors.push({ path: "ui_kits/parts-shop/CollectionScreen.jsx", error: String((e && e.message) || e) }); }

// ui_kits/parts-shop/ProductCard.jsx
try { (() => {
const {
  Badge,
  Button,
  Icon
} = window.SDEuropeanDesignSystem_614674 || {};
const STOCK = {
  in: ['success', 'In Stock'],
  low: ['warning', 'Low Stock'],
  order: ['info', 'Special Order']
};
function ProductCard({
  p,
  onOpen,
  onAdd
}) {
  const [h, setH] = React.useState(false);
  const [tone, label] = STOCK[p.stock];
  return /*#__PURE__*/React.createElement("div", {
    onMouseEnter: () => setH(true),
    onMouseLeave: () => setH(false),
    onClick: () => onOpen && onOpen(p),
    style: {
      background: '#fff',
      border: '1px solid var(--border-default)',
      borderRadius: 'var(--radius-md)',
      overflow: 'hidden',
      cursor: 'pointer',
      display: 'flex',
      flexDirection: 'column',
      transform: h ? 'var(--hover-lift)' : 'none',
      boxShadow: h ? 'var(--shadow-sm)' : 'none',
      transition: 'var(--transition-surface)'
    }
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      position: 'relative'
    }
  }, /*#__PURE__*/React.createElement(ShopPhoto, {
    label: "Part photo"
  }), p.was ? /*#__PURE__*/React.createElement("span", {
    style: {
      position: 'absolute',
      top: '10px',
      left: '10px'
    }
  }, /*#__PURE__*/React.createElement(Badge, {
    tone: "accent"
  }, "Save $", (p.was - p.price).toFixed(0))) : null, p.condition === 'Used' ? /*#__PURE__*/React.createElement("span", {
    style: {
      position: 'absolute',
      top: '10px',
      left: '10px'
    }
  }, /*#__PURE__*/React.createElement(Badge, {
    tone: "dark"
  }, "Used")) : null), /*#__PURE__*/React.createElement("div", {
    style: {
      padding: 'var(--space-4)',
      display: 'grid',
      gap: '6px',
      flex: 1
    }
  }, /*#__PURE__*/React.createElement("span", {
    style: {
      font: 'var(--type-label)',
      letterSpacing: 'var(--ls-wide)',
      textTransform: 'uppercase',
      color: 'var(--text-muted)'
    }
  }, p.brand), /*#__PURE__*/React.createElement("h3", {
    style: {
      font: 'var(--type-h4)',
      fontSize: 'var(--fs-base)',
      lineHeight: 1.3
    }
  }, p.name), /*#__PURE__*/React.createElement("span", {
    style: {
      font: 'var(--type-body-sm)',
      color: 'var(--text-secondary)'
    }
  }, p.fits), /*#__PURE__*/React.createElement("span", {
    style: {
      font: 'var(--type-mono)',
      fontSize: 'var(--fs-xs)',
      color: 'var(--text-muted)'
    }
  }, p.sku), /*#__PURE__*/React.createElement("div", {
    style: {
      marginTop: 'auto',
      paddingTop: 'var(--space-3)',
      display: 'flex',
      alignItems: 'baseline',
      gap: 'var(--space-2)'
    }
  }, /*#__PURE__*/React.createElement("strong", {
    style: {
      font: 'var(--type-h3)',
      fontSize: 'var(--fs-lg)'
    }
  }, "$", p.price.toFixed(2)), p.was ? /*#__PURE__*/React.createElement("s", {
    style: {
      font: 'var(--type-body-sm)',
      color: 'var(--text-muted)'
    }
  }, "$", p.was.toFixed(2)) : null), /*#__PURE__*/React.createElement("div", {
    style: {
      display: 'flex',
      alignItems: 'center',
      gap: 'var(--space-2)',
      marginTop: '2px'
    }
  }, /*#__PURE__*/React.createElement(Badge, {
    tone: tone,
    size: "sm",
    dot: true
  }, label)), /*#__PURE__*/React.createElement("div", {
    style: {
      marginTop: 'var(--space-3)'
    }
  }, /*#__PURE__*/React.createElement(Button, {
    size: "sm",
    fullWidth: true,
    onClick: e => {
      e.stopPropagation();
      onAdd && onAdd(p);
    },
    iconLeft: /*#__PURE__*/React.createElement(Icon, {
      name: "shopping-cart",
      size: 16
    })
  }, "Add to Cart"))));
}
Object.assign(window, {
  ProductCard
});
})(); } catch (e) { __ds_ns.__errors.push({ path: "ui_kits/parts-shop/ProductCard.jsx", error: String((e && e.message) || e) }); }

// ui_kits/parts-shop/ProductScreen.jsx
try { (() => {
const {
  Button,
  Badge,
  Tabs,
  Eyebrow,
  Icon,
  Input,
  Tooltip
} = window.SDEuropeanDesignSystem_614674 || {};
function ProductScreen({
  go,
  add
}) {
  const p = window.SHOP_PRODUCTS[0];
  const [tab, setTab] = React.useState('desc');
  const [qty, setQty] = React.useState(1);
  const [shot, setShot] = React.useState(0);
  return /*#__PURE__*/React.createElement("main", {
    style: {
      maxWidth: 'var(--container-wide)',
      margin: '0 auto',
      padding: 'var(--space-8) var(--gutter-lg) var(--space-16)'
    }
  }, /*#__PURE__*/React.createElement("nav", {
    style: {
      font: 'var(--type-body-sm)',
      color: 'var(--text-muted)',
      marginBottom: 'var(--space-6)'
    }
  }, /*#__PURE__*/React.createElement("a", {
    href: "#",
    onClick: e => {
      e.preventDefault();
      go('home');
    },
    style: {
      borderBottom: 0,
      color: 'var(--text-muted)'
    }
  }, "Home"), " / ", /*#__PURE__*/React.createElement("a", {
    href: "#",
    onClick: e => {
      e.preventDefault();
      go('collection');
    },
    style: {
      borderBottom: 0,
      color: 'var(--text-muted)'
    }
  }, "Shop BMW"), " / ", /*#__PURE__*/React.createElement("strong", {
    style: {
      color: 'var(--text-primary)'
    }
  }, p.name)), /*#__PURE__*/React.createElement("div", {
    style: {
      display: 'grid',
      gridTemplateColumns: '1.1fr 1fr',
      gap: 'var(--space-12)',
      alignItems: 'start'
    }
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      display: 'grid',
      gap: 'var(--space-3)'
    }
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      border: '1px solid var(--border-default)',
      borderRadius: 'var(--radius-lg)',
      overflow: 'hidden'
    }
  }, /*#__PURE__*/React.createElement(ShopPhoto, {
    label: 'Product photo ' + (shot + 1),
    ratio: "4 / 3"
  })), /*#__PURE__*/React.createElement("div", {
    style: {
      display: 'grid',
      gridTemplateColumns: 'repeat(4,1fr)',
      gap: 'var(--space-3)'
    }
  }, [0, 1, 2, 3].map(i => /*#__PURE__*/React.createElement("button", {
    key: i,
    onClick: () => setShot(i),
    style: {
      padding: 0,
      border: '1px solid ' + (shot === i ? 'var(--brand-black)' : 'var(--border-default)'),
      borderRadius: 'var(--radius-md)',
      overflow: 'hidden',
      cursor: 'pointer',
      background: '#fff'
    }
  }, /*#__PURE__*/React.createElement(ShopPhoto, {
    label: '0' + (i + 1)
  }))))), /*#__PURE__*/React.createElement("div", {
    style: {
      display: 'grid',
      gap: 'var(--space-4)'
    }
  }, /*#__PURE__*/React.createElement(Eyebrow, null, p.brand), /*#__PURE__*/React.createElement("h1", {
    style: {
      font: 'var(--type-h1)',
      fontSize: 'var(--fs-3xl)'
    }
  }, p.name), /*#__PURE__*/React.createElement("div", {
    style: {
      display: 'flex',
      gap: 'var(--space-2)',
      flexWrap: 'wrap'
    }
  }, /*#__PURE__*/React.createElement(Badge, {
    tone: "success",
    dot: true
  }, "In Stock"), /*#__PURE__*/React.createElement(Badge, {
    tone: "accent"
  }, "12 Month Warranty"), /*#__PURE__*/React.createElement(Badge, {
    tone: "neutral"
  }, p.condition)), /*#__PURE__*/React.createElement("div", {
    style: {
      display: 'flex',
      alignItems: 'baseline',
      gap: 'var(--space-3)'
    }
  }, /*#__PURE__*/React.createElement("strong", {
    style: {
      font: 'var(--type-h1)',
      fontSize: '38px'
    }
  }, "$", p.price.toFixed(2)), p.was ? /*#__PURE__*/React.createElement("s", {
    style: {
      font: 'var(--type-lead)',
      color: 'var(--text-muted)'
    }
  }, "$", p.was.toFixed(2)) : null, /*#__PURE__*/React.createElement("span", {
    style: {
      font: 'var(--type-body-sm)',
      color: 'var(--text-muted)'
    }
  }, "NZD, incl. GST")), /*#__PURE__*/React.createElement("div", {
    style: {
      display: 'grid',
      gap: '6px',
      font: 'var(--type-body-sm)',
      color: 'var(--text-secondary)'
    }
  }, /*#__PURE__*/React.createElement("span", null, "Fits ", /*#__PURE__*/React.createElement("strong", {
    style: {
      color: 'var(--text-primary)'
    }
  }, p.fits)), /*#__PURE__*/React.createElement("span", null, "Part number ", /*#__PURE__*/React.createElement("span", {
    style: {
      font: 'var(--type-mono)',
      color: 'var(--text-primary)'
    }
  }, p.sku))), /*#__PURE__*/React.createElement("div", {
    style: {
      background: 'var(--surface-subtle)',
      borderRadius: 'var(--radius-md)',
      padding: 'var(--space-4)',
      display: 'grid',
      gap: 'var(--space-3)'
    }
  }, /*#__PURE__*/React.createElement(Eyebrow, null, "Guaranteed fit"), /*#__PURE__*/React.createElement("div", {
    style: {
      display: 'flex',
      gap: 'var(--space-2)'
    }
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      flex: 1
    }
  }, /*#__PURE__*/React.createElement(Input, {
    mono: true,
    placeholder: "Enter your rego",
    "aria-label": "Rego"
  })), /*#__PURE__*/React.createElement(Button, {
    variant: "secondary"
  }, "Check Fitment")), /*#__PURE__*/React.createElement("p", {
    style: {
      font: 'var(--type-body-sm)',
      color: 'var(--text-muted)'
    }
  }, "We manually check each order with your rego number to be 100% sure it will fit.")), /*#__PURE__*/React.createElement("div", {
    style: {
      display: 'flex',
      gap: 'var(--space-3)',
      alignItems: 'stretch'
    }
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      display: 'flex',
      alignItems: 'center',
      border: '1px solid var(--border-default)',
      borderRadius: 'var(--radius-sm)'
    }
  }, /*#__PURE__*/React.createElement("button", {
    onClick: () => setQty(Math.max(1, qty - 1)),
    style: {
      width: '42px',
      height: '44px',
      border: 0,
      background: 'none',
      cursor: 'pointer',
      font: 'var(--type-h4)'
    }
  }, "\u2212"), /*#__PURE__*/React.createElement("span", {
    style: {
      width: '40px',
      textAlign: 'center',
      font: 'var(--type-mono)',
      fontSize: 'var(--fs-base)'
    }
  }, qty), /*#__PURE__*/React.createElement("button", {
    onClick: () => setQty(qty + 1),
    style: {
      width: '42px',
      height: '44px',
      border: 0,
      background: 'none',
      cursor: 'pointer',
      font: 'var(--type-h4)'
    }
  }, "+")), /*#__PURE__*/React.createElement(Button, {
    size: "md",
    style: {
      flex: 1
    },
    onClick: () => add(p, qty),
    iconLeft: /*#__PURE__*/React.createElement(Icon, {
      name: "shopping-cart",
      size: 20
    })
  }, "Add to Cart")), /*#__PURE__*/React.createElement("div", {
    style: {
      display: 'flex',
      gap: 'var(--space-5)',
      font: 'var(--type-body-sm)',
      color: 'var(--text-secondary)'
    }
  }, /*#__PURE__*/React.createElement("span", {
    style: {
      display: 'inline-flex',
      gap: '8px',
      alignItems: 'center'
    }
  }, /*#__PURE__*/React.createElement(Icon, {
    name: "truck",
    size: 16
  }), "Order before 2pm for overnight delivery"), /*#__PURE__*/React.createElement("span", {
    style: {
      display: 'inline-flex',
      gap: '8px',
      alignItems: 'center'
    }
  }, /*#__PURE__*/React.createElement(Icon, {
    name: "phone",
    size: 16
  }), "0800 269 772")))), /*#__PURE__*/React.createElement("div", {
    style: {
      marginTop: 'var(--space-16)'
    }
  }, /*#__PURE__*/React.createElement(Tabs, {
    items: [{
      value: 'desc',
      label: 'Description'
    }, {
      value: 'fit',
      label: 'Fitment',
      count: 14
    }, {
      value: 'del',
      label: 'Delivery & Warranty'
    }],
    value: tab,
    onChange: setTab
  }), /*#__PURE__*/React.createElement("div", {
    style: {
      marginTop: 'var(--space-6)',
      maxWidth: '760px',
      font: 'var(--type-body)',
      color: 'var(--text-secondary)',
      display: 'grid',
      gap: 'var(--space-3)'
    }
  }, tab === 'desc' ? /*#__PURE__*/React.createElement(React.Fragment, null, /*#__PURE__*/React.createElement("p", null, "Complete front right window regulator with motor, supplied by Teilex. Direct replacement for the original unit \u2014 the common failure on this platform is the plastic slider and cable guide, so we supply the whole assembly rather than a repair kit."), /*#__PURE__*/React.createElement("p", null, "Not sure this is the one? Send through your rego and our parts team will match your VIN to the exact part you need.")) : tab === 'fit' ? /*#__PURE__*/React.createElement("div", {
    style: {
      display: 'grid',
      gridTemplateColumns: 'repeat(2,1fr)',
      gap: '6px var(--space-8)'
    }
  }, ['E90 320i 2005–2011', 'E90 325i 2005–2011', 'E90 330i 2005–2011', 'E91 320i Touring 2006–2012', 'E91 325i Touring 2006–2012', 'E90 320d 2005–2011', 'E90 335i 2007–2011', 'E91 330d Touring 2007–2012'].map(x => /*#__PURE__*/React.createElement("span", {
    key: x,
    style: {
      display: 'flex',
      gap: '8px',
      alignItems: 'center',
      font: 'var(--type-body-sm)'
    }
  }, /*#__PURE__*/React.createElement("span", {
    style: {
      color: 'var(--status-success)'
    }
  }, /*#__PURE__*/React.createElement(Icon, {
    name: "check",
    size: 16
  })), x))) : /*#__PURE__*/React.createElement(React.Fragment, null, /*#__PURE__*/React.createElement("p", null, "Order before 2pm for overnight delivery in most cases. Large items expect 2\u20133 days. Free shipping on orders over $175."), /*#__PURE__*/React.createElement("p", null, "All new parts carry a 12 month part-only warranty so you can buy with confidence. ", /*#__PURE__*/React.createElement(Tooltip, {
    label: "Warrant of Fitness"
  }, /*#__PURE__*/React.createElement("span", {
    style: {
      borderBottom: '1px dotted var(--neutral-400)'
    }
  }, "WOF")), " failures fitted at our Te Rapa workshop are covered on labour too.")))), /*#__PURE__*/React.createElement("div", {
    style: {
      marginTop: 'var(--space-16)'
    }
  }, /*#__PURE__*/React.createElement(Eyebrow, null, "Customers also bought"), /*#__PURE__*/React.createElement("div", {
    style: {
      display: 'grid',
      gridTemplateColumns: 'repeat(4,1fr)',
      gap: 'var(--space-6)',
      marginTop: 'var(--space-5)'
    }
  }, window.SHOP_PRODUCTS.slice(1, 5).map(x => /*#__PURE__*/React.createElement(ProductCard, {
    key: x.id,
    p: x,
    onOpen: () => go('product'),
    onAdd: add
  })))));
}
Object.assign(window, {
  ProductScreen
});
})(); } catch (e) { __ds_ns.__errors.push({ path: "ui_kits/parts-shop/ProductScreen.jsx", error: String((e && e.message) || e) }); }

// ui_kits/parts-shop/ShopChrome.jsx
try { (() => {
const {
  Logo,
  Eyebrow,
  Button,
  IconButton,
  Icon,
  Input,
  Badge
} = window.SDEuropeanDesignSystem_614674 || {};
const MARQUE_NAV = ['Shop BMW', 'Shop Mini', 'Shop Audi', 'Shop Skoda', 'Shop VW', 'Shop Mercedes'];
const CATEGORIES = ['Accessories', 'Wiper Blades', 'Window Regulator', 'Headlights', 'Tail Light', 'Water Pump', 'Coolant Hose', 'Timing Belt Kits', 'Alternators', 'Starter Motor', 'Air Filter', 'Oil Filter', 'Cabin Filter', 'Brake Pads', 'PCV Valve', 'Coding & Cloning'];
const BRANDS = ['Valeo', 'Teilex', 'Topran', 'Mann-Filter', 'Hella Lighting', 'Arnott Suspension', 'Vemo & Vaico', 'Magneti Marelli'];
function AnnouncementBar() {
  return /*#__PURE__*/React.createElement("div", {
    style: {
      background: 'var(--brand-yellow)',
      color: 'var(--brand-black)',
      textAlign: 'center',
      padding: '9px 0',
      font: 'var(--type-label)',
      letterSpacing: 'var(--ls-wide)',
      textTransform: 'uppercase'
    }
  }, "Free shipping on orders over $175");
}
function ShopUtilityBar() {
  return /*#__PURE__*/React.createElement("div", {
    style: {
      background: 'var(--brand-black)',
      color: 'rgba(255,255,255,.75)'
    }
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      maxWidth: 'var(--container-wide)',
      margin: '0 auto',
      padding: '0 var(--gutter-lg)',
      height: 'var(--utilitybar-h)',
      display: 'flex',
      alignItems: 'center',
      gap: 'var(--space-6)',
      font: 'var(--type-body-sm)'
    }
  }, /*#__PURE__*/React.createElement("span", {
    style: {
      display: 'inline-flex',
      alignItems: 'center',
      gap: '8px',
      color: '#fff'
    }
  }, /*#__PURE__*/React.createElement(Icon, {
    name: "phone",
    size: 16
  }), "0800 269 772"), /*#__PURE__*/React.createElement("span", null, "All Parts in Stock in NZ"), /*#__PURE__*/React.createElement("strong", {
    style: {
      marginLeft: 'auto',
      color: 'var(--brand-yellow)'
    }
  }, "Supplying New & Used parts in New Zealand for 30+ years"), /*#__PURE__*/React.createElement("a", {
    href: "#",
    style: {
      color: 'rgba(255,255,255,.75)',
      borderBottom: 0
    }
  }, "Our Regular Website")));
}
function ShopHeader({
  go,
  cartCount,
  onCart
}) {
  const [menu, setMenu] = React.useState(null);
  return /*#__PURE__*/React.createElement("header", {
    style: {
      position: 'sticky',
      top: 0,
      zIndex: 60,
      background: '#fff',
      borderBottom: '1px solid var(--border-default)'
    }
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      maxWidth: 'var(--container-wide)',
      margin: '0 auto',
      padding: '0 var(--gutter-lg)',
      height: '84px',
      display: 'flex',
      alignItems: 'center',
      gap: 'var(--space-8)'
    }
  }, /*#__PURE__*/React.createElement("a", {
    href: "#",
    onClick: e => {
      e.preventDefault();
      go('home');
    },
    style: {
      borderBottom: 0
    }
  }, /*#__PURE__*/React.createElement(Logo, {
    height: 40,
    assetBase: "../../assets"
  })), /*#__PURE__*/React.createElement("div", {
    style: {
      flex: 1,
      maxWidth: '560px'
    }
  }, /*#__PURE__*/React.createElement(Input, {
    placeholder: "Search the store \u2014 part name, number or rego",
    prefix: /*#__PURE__*/React.createElement(Icon, {
      name: "search",
      size: 20
    }),
    "aria-label": "Search the store"
  })), /*#__PURE__*/React.createElement("div", {
    style: {
      marginLeft: 'auto',
      display: 'flex',
      alignItems: 'center',
      gap: 'var(--space-2)'
    }
  }, /*#__PURE__*/React.createElement(Button, {
    variant: "ghost",
    size: "sm",
    iconLeft: /*#__PURE__*/React.createElement(Icon, {
      name: "user",
      size: 20
    })
  }, "Sign In"), /*#__PURE__*/React.createElement("button", {
    onClick: onCart,
    style: {
      position: 'relative',
      border: '1px solid var(--border-default)',
      background: '#fff',
      borderRadius: 'var(--radius-sm)',
      height: '40px',
      padding: '0 14px',
      display: 'inline-flex',
      alignItems: 'center',
      gap: '8px',
      cursor: 'pointer',
      font: 'var(--type-button)',
      fontSize: 'var(--fs-sm)',
      textTransform: 'uppercase',
      letterSpacing: 'var(--ls-wide)'
    }
  }, /*#__PURE__*/React.createElement(Icon, {
    name: "shopping-cart",
    size: 20
  }), "Cart", /*#__PURE__*/React.createElement("span", {
    style: {
      background: 'var(--brand-yellow)',
      color: 'var(--brand-black)',
      borderRadius: 'var(--radius-pill)',
      minWidth: '22px',
      height: '20px',
      display: 'grid',
      placeItems: 'center',
      font: 'var(--type-mono)',
      fontSize: 'var(--fs-xs)'
    }
  }, cartCount)))), /*#__PURE__*/React.createElement("nav", {
    style: {
      borderTop: '1px solid var(--border-default)',
      background: 'var(--surface-subtle)'
    }
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      maxWidth: 'var(--container-wide)',
      margin: '0 auto',
      padding: '0 var(--gutter-lg)',
      display: 'flex',
      gap: 'var(--space-6)',
      height: '48px',
      alignItems: 'center'
    },
    onMouseLeave: () => setMenu(null)
  }, MARQUE_NAV.map(m => /*#__PURE__*/React.createElement("button", {
    key: m,
    onClick: () => go('collection'),
    onMouseEnter: () => setMenu(null),
    style: {
      border: 0,
      background: 'none',
      cursor: 'pointer',
      font: 'var(--type-button)',
      fontSize: 'var(--fs-sm)',
      letterSpacing: 'var(--ls-wide)',
      textTransform: 'uppercase'
    }
  }, m)), [['Popular Categories', CATEGORIES], ['Popular Brands', BRANDS]].map(([label, items]) => /*#__PURE__*/React.createElement("div", {
    key: label,
    style: {
      position: 'relative'
    },
    onMouseEnter: () => setMenu(label)
  }, /*#__PURE__*/React.createElement("button", {
    style: {
      border: 0,
      background: 'none',
      cursor: 'pointer',
      display: 'inline-flex',
      alignItems: 'center',
      gap: '6px',
      font: 'var(--type-button)',
      fontSize: 'var(--fs-sm)',
      letterSpacing: 'var(--ls-wide)',
      textTransform: 'uppercase'
    }
  }, label, /*#__PURE__*/React.createElement(Icon, {
    name: "chevron-down",
    size: 16
  })), menu === label ? /*#__PURE__*/React.createElement("div", {
    style: {
      position: 'absolute',
      top: '100%',
      left: '-16px',
      marginTop: '12px',
      background: '#fff',
      border: '1px solid var(--border-default)',
      borderRadius: 'var(--radius-md)',
      boxShadow: 'var(--shadow-md)',
      padding: 'var(--space-4)',
      display: 'grid',
      gridTemplateColumns: 'repeat(2,minmax(180px,1fr))',
      gap: '2px var(--space-6)',
      zIndex: 70
    }
  }, items.map(i => /*#__PURE__*/React.createElement("a", {
    key: i,
    href: "#",
    onClick: e => {
      e.preventDefault();
      go('collection');
    },
    style: {
      borderBottom: 0,
      padding: '7px 10px',
      borderRadius: 'var(--radius-sm)',
      font: 'var(--type-body-sm)',
      color: 'var(--text-primary)'
    }
  }, i))) : null)))));
}
const TRUST = [['truck', 'Nationwide delivery', 'Order before 2pm for overnight delivery in most cases. Large items 2–3 days.'], ['shield-check', 'Guaranteed fit', 'We manually check each order with your rego number to be 100% sure it will fit.'], ['badge-check', '12 months warranty', 'All new parts carry a 12 month part-only warranty so you can buy with confidence.'], ['heart', 'Kiwi owned & operated', "We're Kiwis helping Kiwis and we've been doing it for over 30 years."]];
function TrustRow() {
  return /*#__PURE__*/React.createElement("div", {
    style: {
      background: 'var(--surface-subtle)',
      borderTop: '1px solid var(--border-default)'
    }
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      maxWidth: 'var(--container-wide)',
      margin: '0 auto',
      padding: 'var(--space-10) var(--gutter-lg)',
      display: 'grid',
      gridTemplateColumns: 'repeat(4,1fr)',
      gap: 'var(--space-8)'
    }
  }, TRUST.map(([ic, t, d]) => /*#__PURE__*/React.createElement("div", {
    key: t,
    style: {
      display: 'grid',
      gap: 'var(--space-2)'
    }
  }, /*#__PURE__*/React.createElement(Icon, {
    name: ic,
    size: 32
  }), /*#__PURE__*/React.createElement("div", {
    style: {
      font: 'var(--type-h4)',
      textTransform: 'uppercase',
      letterSpacing: 'var(--ls-wide)',
      fontSize: 'var(--fs-sm)'
    }
  }, t), /*#__PURE__*/React.createElement("p", {
    style: {
      font: 'var(--type-body-sm)',
      color: 'var(--text-secondary)'
    }
  }, d)))));
}
function ShopFooter() {
  const col = (h, items) => /*#__PURE__*/React.createElement("div", {
    style: {
      display: 'grid',
      gap: 'var(--space-3)',
      alignContent: 'start'
    }
  }, /*#__PURE__*/React.createElement(Eyebrow, {
    tone: "accent"
  }, h), items.map(i => /*#__PURE__*/React.createElement("a", {
    key: i,
    href: "#",
    style: {
      color: 'rgba(255,255,255,.7)',
      borderBottom: 0,
      font: 'var(--type-body-sm)'
    }
  }, i)));
  return /*#__PURE__*/React.createElement("footer", {
    style: {
      background: 'var(--brand-black)',
      color: 'rgba(255,255,255,.7)'
    }
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      maxWidth: 'var(--container-wide)',
      margin: '0 auto',
      padding: 'var(--space-16) var(--gutter-lg) var(--space-8)',
      display: 'grid',
      gridTemplateColumns: '1.4fr 1fr 1fr 1fr',
      gap: 'var(--space-10)'
    }
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      display: 'grid',
      gap: 'var(--space-4)',
      alignContent: 'start'
    }
  }, /*#__PURE__*/React.createElement(Eyebrow, {
    tone: "accent"
  }, "Newsletter Sign Up"), /*#__PURE__*/React.createElement("div", {
    style: {
      display: 'flex',
      gap: 'var(--space-2)'
    }
  }, /*#__PURE__*/React.createElement("input", {
    placeholder: "Your email",
    style: {
      flex: 1,
      height: '44px',
      padding: '0 12px',
      border: '1px solid var(--border-inverse)',
      background: 'transparent',
      color: '#fff',
      borderRadius: 'var(--radius-sm)',
      font: 'var(--type-body)'
    }
  }), /*#__PURE__*/React.createElement(Button, {
    size: "md"
  }, "Submit")), /*#__PURE__*/React.createElement("p", {
    style: {
      font: 'var(--type-body-sm)'
    }
  }, "Need help finding the right part for your European car? Call 0800 269 772 or email parts@sdeuropean.co.nz"), /*#__PURE__*/React.createElement("p", {
    style: {
      font: 'var(--type-body-sm)'
    }
  }, "177 Ellis Street, Hamilton, Waikato")), col('Customer Service', ['Contact Us', 'Refund Policy', 'Privacy Policy', 'Terms of Service', 'Delivery', 'Warranty']), col('Used Parts', ['Audi Dismantling', 'BMW Dismantling', 'Mercedes Dismantling', 'Mini Dismantling', 'Skoda Dismantling', 'Volkswagen Dismantling']), col('Follow Us', ['Facebook', 'Instagram', 'LinkedIn', 'Blog'])), /*#__PURE__*/React.createElement("div", {
    style: {
      maxWidth: 'var(--container-wide)',
      margin: '0 auto',
      padding: '0 var(--gutter-lg) var(--space-10)',
      font: 'var(--type-body-sm)',
      color: 'rgba(255,255,255,.45)',
      borderTop: '1px solid var(--border-inverse)',
      paddingTop: 'var(--space-6)'
    }
  }, "\xA9 Copyright 2026 SD European. All Rights Reserved. SD European is an independent parts and service specialist and has no association with BMW New Zealand or BMW AG."));
}
function ShopPhoto({
  label,
  ratio = '1 / 1',
  dark = false,
  style
}) {
  return /*#__PURE__*/React.createElement("div", {
    style: {
      position: 'relative',
      aspectRatio: ratio,
      width: '100%',
      background: dark ? 'linear-gradient(135deg,#1c1e21,#4a4e54)' : 'linear-gradient(135deg,#e8e9ea,#f7f7f8)',
      display: 'grid',
      placeItems: 'center',
      ...style
    }
  }, /*#__PURE__*/React.createElement("span", {
    style: {
      font: 'var(--type-label)',
      letterSpacing: 'var(--ls-wide)',
      textTransform: 'uppercase',
      color: dark ? 'rgba(255,255,255,.3)' : 'var(--neutral-400)'
    }
  }, label));
}
Object.assign(window, {
  AnnouncementBar,
  ShopUtilityBar,
  ShopHeader,
  TrustRow,
  ShopFooter,
  ShopPhoto,
  SHOP_CATEGORIES: CATEGORIES,
  SHOP_BRANDS: BRANDS
});
})(); } catch (e) { __ds_ns.__errors.push({ path: "ui_kits/parts-shop/ShopChrome.jsx", error: String((e && e.message) || e) }); }

// ui_kits/parts-shop/StorefrontScreen.jsx
try { (() => {
const {
  Button,
  Eyebrow,
  SectionHeading,
  Icon,
  Badge
} = window.SDEuropeanDesignSystem_614674 || {};
const TILES = ['Window Regulator', 'Brakes', 'Suspension', 'Electrical', 'Bumpers', 'Oil Filters', 'Cooling', 'Air Filters'];
function StorefrontScreen({
  go,
  add
}) {
  return /*#__PURE__*/React.createElement("main", null, /*#__PURE__*/React.createElement("div", {
    style: {
      position: 'relative',
      height: '440px',
      background: 'linear-gradient(135deg,#17191c,#43474d)'
    }
  }, /*#__PURE__*/React.createElement("span", {
    style: {
      position: 'absolute',
      inset: 0,
      display: 'grid',
      placeItems: 'center',
      font: 'var(--type-label)',
      letterSpacing: 'var(--ls-wide)',
      textTransform: 'uppercase',
      color: 'rgba(255,255,255,.2)'
    }
  }, "Homepage banner"), /*#__PURE__*/React.createElement("div", {
    style: {
      position: 'absolute',
      inset: 0,
      background: 'var(--scrim-left)'
    }
  }), /*#__PURE__*/React.createElement("div", {
    style: {
      position: 'absolute',
      inset: 0,
      display: 'grid',
      alignItems: 'center'
    }
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      maxWidth: 'var(--container-wide)',
      width: '100%',
      margin: '0 auto',
      padding: '0 var(--gutter-lg)'
    }
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      display: 'grid',
      gap: 'var(--space-4)',
      maxWidth: '620px'
    }
  }, /*#__PURE__*/React.createElement(Eyebrow, {
    tone: "inverse"
  }, "Welcome to NZ's home of"), /*#__PURE__*/React.createElement("h1", {
    style: {
      font: 'var(--type-display)',
      fontSize: '54px',
      color: '#fff'
    }
  }, "European Car Parts"), /*#__PURE__*/React.createElement(Eyebrow, {
    tone: "accent"
  }, "AUDI | BMW | MERCEDES | MINI | SKODA | PORSCHE | VW"), /*#__PURE__*/React.createElement("div", {
    style: {
      marginTop: 'var(--space-2)'
    }
  }, /*#__PURE__*/React.createElement(Button, {
    size: "lg",
    onClick: () => go('collection'),
    iconRight: /*#__PURE__*/React.createElement(Icon, {
      name: "arrow-right",
      size: 20
    })
  }, "Shop Now")))))), /*#__PURE__*/React.createElement("section", {
    style: {
      maxWidth: 'var(--container-wide)',
      margin: '0 auto',
      padding: 'var(--space-16) var(--gutter-lg)'
    }
  }, /*#__PURE__*/React.createElement(SectionHeading, {
    eyebrow: "Shop by category",
    title: "New Aftermarket and OEM"
  }), /*#__PURE__*/React.createElement("div", {
    style: {
      display: 'grid',
      gridTemplateColumns: 'repeat(8,1fr)',
      gap: 'var(--space-4)',
      marginTop: 'var(--space-8)'
    }
  }, TILES.map(t => /*#__PURE__*/React.createElement("button", {
    key: t,
    onClick: () => go('collection'),
    style: {
      border: '1px solid var(--border-default)',
      background: '#fff',
      borderRadius: 'var(--radius-md)',
      padding: 0,
      overflow: 'hidden',
      cursor: 'pointer',
      display: 'grid'
    }
  }, /*#__PURE__*/React.createElement(ShopPhoto, {
    label: "Part"
  }), /*#__PURE__*/React.createElement("span", {
    style: {
      padding: 'var(--space-3)',
      font: 'var(--type-label)',
      letterSpacing: 'var(--ls-wide)',
      textTransform: 'uppercase'
    }
  }, t))))), /*#__PURE__*/React.createElement("section", {
    style: {
      maxWidth: 'var(--container-wide)',
      margin: '0 auto',
      padding: '0 var(--gutter-lg) var(--space-16)'
    }
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      display: 'flex',
      alignItems: 'end',
      justifyContent: 'space-between',
      gap: 'var(--space-6)'
    }
  }, /*#__PURE__*/React.createElement(SectionHeading, {
    eyebrow: "Fresh in",
    title: "Latest Parts"
  }), /*#__PURE__*/React.createElement(Button, {
    variant: "outline",
    onClick: () => go('collection')
  }, "View All Parts")), /*#__PURE__*/React.createElement("div", {
    style: {
      display: 'grid',
      gridTemplateColumns: 'repeat(4,1fr)',
      gap: 'var(--space-6)',
      marginTop: 'var(--space-8)'
    }
  }, window.SHOP_PRODUCTS.slice(0, 4).map(p => /*#__PURE__*/React.createElement(ProductCard, {
    key: p.id,
    p: p,
    onOpen: () => go('product'),
    onAdd: add
  })))), /*#__PURE__*/React.createElement("div", {
    style: {
      position: 'relative',
      overflow: 'hidden',
      background: 'var(--brand-black)'
    }
  }, /*#__PURE__*/React.createElement("img", {
    src: "../../assets/symbol-black-yellow.png",
    alt: "",
    style: {
      position: 'absolute',
      left: '-140px',
      bottom: '-180px',
      width: '560px',
      opacity: .08,
      filter: 'grayscale(1) brightness(3)'
    }
  }), /*#__PURE__*/React.createElement("div", {
    style: {
      position: 'relative',
      maxWidth: 'var(--container-wide)',
      margin: '0 auto',
      padding: 'var(--space-16) var(--gutter-lg)',
      display: 'grid',
      gridTemplateColumns: '1fr 1fr',
      gap: 'var(--space-12)',
      alignItems: 'center'
    }
  }, /*#__PURE__*/React.createElement(SectionHeading, {
    inverse: true,
    eyebrow: "Since 1989",
    title: "Who are SD European?",
    lead: "Founded in Hamilton, Waikato from a passion for restoring BMWs. Over 30 years later, our passion for European cars is stronger than ever. With a friendly and skilled team behind us, we provide high-quality aftermarket parts, OEM parts, second-hand parts, and a dedicated service centre."
  }), /*#__PURE__*/React.createElement("div", {
    style: {
      display: 'grid',
      gridTemplateColumns: 'repeat(3,1fr)',
      gap: 'var(--space-6)'
    }
  }, [['30+', 'Years'], ['350+', 'Cars dismantling'], ['NZ', 'Wide delivery']].map(([n, l]) => /*#__PURE__*/React.createElement("div", {
    key: l,
    style: {
      display: 'grid',
      gap: '4px'
    }
  }, /*#__PURE__*/React.createElement("span", {
    style: {
      font: 'var(--type-h1)',
      fontSize: '44px',
      color: 'var(--brand-yellow)'
    }
  }, n), /*#__PURE__*/React.createElement("span", {
    style: {
      font: 'var(--type-label)',
      letterSpacing: 'var(--ls-wide)',
      textTransform: 'uppercase',
      color: 'rgba(255,255,255,.6)'
    }
  }, l)))))), /*#__PURE__*/React.createElement("section", {
    style: {
      maxWidth: 'var(--container-wide)',
      margin: '0 auto',
      padding: 'var(--space-16) var(--gutter-lg)'
    }
  }, /*#__PURE__*/React.createElement(Eyebrow, null, "Top Brands"), /*#__PURE__*/React.createElement("div", {
    style: {
      display: 'grid',
      gridTemplateColumns: 'repeat(6,1fr)',
      gap: 'var(--space-4)',
      marginTop: 'var(--space-5)'
    }
  }, ['Arnott', 'Vaico / Vemo', 'Mann-Filter', 'Topran', 'Hella', 'Valeo'].map(b => /*#__PURE__*/React.createElement("div", {
    key: b,
    style: {
      height: '80px',
      border: '1px solid var(--border-default)',
      borderRadius: 'var(--radius-md)',
      display: 'grid',
      placeItems: 'center',
      font: 'var(--type-label)',
      color: 'var(--text-muted)'
    }
  }, b)))), /*#__PURE__*/React.createElement(TrustRow, null));
}
Object.assign(window, {
  StorefrontScreen
});
})(); } catch (e) { __ds_ns.__errors.push({ path: "ui_kits/parts-shop/StorefrontScreen.jsx", error: String((e && e.message) || e) }); }

// ui_kits/parts-shop/products.js
try { (() => {
window.SHOP_PRODUCTS = [{
  id: 'p1',
  name: 'Window Regulator — Front Right',
  fits: 'BMW E90 / E91 2005–2011',
  sku: '51-33-7-140-590',
  brand: 'Teilex',
  price: 189.00,
  was: 229.00,
  stock: 'in',
  condition: 'New aftermarket'
}, {
  id: 'p2',
  name: 'Water Pump — N20 / N26',
  fits: 'BMW F20 / F30 2011–2016',
  sku: '11-51-7-586-925',
  brand: 'Vemo',
  price: 264.50,
  stock: 'in',
  condition: 'OEM'
}, {
  id: 'p3',
  name: 'Oil Filter Kit',
  fits: 'Audi A4 B8 2.0 TFSI',
  sku: 'HU 719/7 X',
  brand: 'Mann-Filter',
  price: 38.90,
  stock: 'in',
  condition: 'New aftermarket'
}, {
  id: 'p4',
  name: 'Headlight Assembly — Left',
  fits: 'VW Golf Mk6 2009–2013',
  sku: '5K1-941-005',
  brand: 'Hella',
  price: 612.00,
  stock: 'low',
  condition: 'New aftermarket'
}, {
  id: 'p5',
  name: 'Air Suspension Strut — Rear',
  fits: 'Mercedes W211 E-Class',
  sku: 'A-2603',
  brand: 'Arnott',
  price: 1049.00,
  stock: 'order',
  condition: 'New aftermarket'
}, {
  id: 'p6',
  name: 'Brake Pad Set — Front',
  fits: 'Mini R56 Cooper S',
  sku: '34-11-6-778-320',
  brand: 'Valeo',
  price: 142.00,
  stock: 'in',
  condition: 'New aftermarket'
}, {
  id: 'p7',
  name: 'Rocker Cover Gasket',
  fits: 'Skoda Octavia vRS 1Z',
  sku: '06F-103-483D',
  brand: 'Topran',
  price: 54.20,
  stock: 'in',
  condition: 'New aftermarket'
}, {
  id: 'p8',
  name: 'Tail Light — Right, Outer',
  fits: 'BMW E91 Touring 2008–2012',
  sku: '63-21-7-160-062',
  brand: 'SD Used',
  price: 175.00,
  stock: 'in',
  condition: 'Used'
}];
})(); } catch (e) { __ds_ns.__errors.push({ path: "ui_kits/parts-shop/products.js", error: String((e && e.message) || e) }); }

// ui_kits/website/BookingScreen.jsx
try { (() => {
const {
  Button,
  Input,
  Select,
  Checkbox,
  Radio,
  Eyebrow,
  SectionHeading,
  Icon,
  Badge,
  Toast
} = window.SDEuropeanDesignSystem_614674 || {};
const SLOTS = ['8:00am', '9:30am', '11:00am', '1:00pm', '2:30pm', '4:00pm'];
function BookingScreen() {
  const [step, setStep] = React.useState(1);
  const [service, setService] = React.useState('service');
  const [slot, setSlot] = React.useState('9:30am');
  const [done, setDone] = React.useState(false);
  return /*#__PURE__*/React.createElement("main", null, /*#__PURE__*/React.createElement(Section, {
    tone: "grey",
    pad: "var(--space-12)"
  }, /*#__PURE__*/React.createElement(SectionHeading, {
    eyebrow: "Service Centre \xB7 Te Rapa",
    title: "Online Service Booking",
    lead: "Tell us about your car and pick a time. We'll confirm by phone the same day."
  })), /*#__PURE__*/React.createElement(Section, {
    pad: "var(--space-12)"
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      display: 'grid',
      gridTemplateColumns: '1.5fr 1fr',
      gap: 'var(--space-12)',
      alignItems: 'start'
    }
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      display: 'grid',
      gap: 'var(--space-8)'
    }
  }, /*#__PURE__*/React.createElement("ol", {
    style: {
      display: 'flex',
      gap: 'var(--space-6)',
      listStyle: 'none',
      padding: 0,
      margin: 0
    }
  }, ['Your car', 'Service', 'Your details'].map((s, i) => /*#__PURE__*/React.createElement("li", {
    key: s,
    style: {
      display: 'flex',
      alignItems: 'center',
      gap: 'var(--space-2)',
      font: 'var(--type-label)',
      letterSpacing: 'var(--ls-wide)',
      textTransform: 'uppercase',
      color: step >= i + 1 ? 'var(--text-primary)' : 'var(--text-muted)'
    }
  }, /*#__PURE__*/React.createElement("span", {
    style: {
      width: '26px',
      height: '26px',
      borderRadius: '50%',
      display: 'grid',
      placeItems: 'center',
      background: step >= i + 1 ? 'var(--brand-yellow)' : 'var(--neutral-200)',
      color: 'var(--brand-black)'
    }
  }, i + 1), s))), step === 1 ? /*#__PURE__*/React.createElement("div", {
    style: {
      display: 'grid',
      gap: 'var(--space-5)'
    }
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      display: 'grid',
      gridTemplateColumns: '1fr 1fr',
      gap: 'var(--space-4)'
    }
  }, /*#__PURE__*/React.createElement(Input, {
    label: "Rego",
    mono: true,
    placeholder: "ABC123",
    hint: "We'll pull your make, model and VIN."
  }), /*#__PURE__*/React.createElement(Select, {
    label: "Make",
    options: ['Audi', 'BMW', 'Mercedes-Benz', 'Mini', 'Skoda', 'Volkswagen']
  })), /*#__PURE__*/React.createElement("div", {
    style: {
      display: 'grid',
      gridTemplateColumns: '1fr 1fr',
      gap: 'var(--space-4)'
    }
  }, /*#__PURE__*/React.createElement(Input, {
    label: "Model",
    placeholder: "e.g. 320i Touring"
  }), /*#__PURE__*/React.createElement(Input, {
    label: "Odometer",
    placeholder: "e.g. 148,000",
    suffix: /*#__PURE__*/React.createElement("span", {
      style: {
        font: 'var(--type-body-sm)'
      }
    }, "km")
  })), /*#__PURE__*/React.createElement("div", null, /*#__PURE__*/React.createElement(Button, {
    onClick: () => setStep(2),
    iconRight: /*#__PURE__*/React.createElement(Icon, {
      name: "arrow-right",
      size: 20
    })
  }, "Continue"))) : step === 2 ? /*#__PURE__*/React.createElement("div", {
    style: {
      display: 'grid',
      gap: 'var(--space-5)'
    }
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      display: 'grid',
      gap: 'var(--space-3)'
    }
  }, /*#__PURE__*/React.createElement(Eyebrow, null, "What's it in for?"), [['service', 'Scheduled service', 'Oil, filters, full inspection'], ['wof', 'WOF inspection', 'In and out the same day'], ['diag', 'Diagnostics / fault', "Tell us what it's doing"], ['dsg', 'DSG service', 'Audi, VW and Skoda']].map(([v, l, d]) => /*#__PURE__*/React.createElement(Radio, {
    key: v,
    name: "svc",
    label: l,
    description: d,
    checked: service === v,
    onChange: () => setService(v)
  }))), /*#__PURE__*/React.createElement("div", {
    style: {
      display: 'grid',
      gap: 'var(--space-3)'
    }
  }, /*#__PURE__*/React.createElement(Eyebrow, null, "Pick a time \u2014 Thu 13 Aug"), /*#__PURE__*/React.createElement("div", {
    style: {
      display: 'flex',
      gap: 'var(--space-2)',
      flexWrap: 'wrap'
    }
  }, SLOTS.map(s => /*#__PURE__*/React.createElement("button", {
    key: s,
    onClick: () => setSlot(s),
    style: {
      padding: '10px 16px',
      borderRadius: 'var(--radius-sm)',
      cursor: 'pointer',
      border: '1px solid ' + (slot === s ? 'var(--brand-black)' : 'var(--border-default)'),
      background: slot === s ? 'var(--brand-black)' : '#fff',
      color: slot === s ? '#fff' : 'var(--text-primary)',
      font: 'var(--type-body-sm)',
      fontWeight: 'var(--fw-semibold)',
      transition: 'var(--transition-control)'
    }
  }, s)))), /*#__PURE__*/React.createElement(Checkbox, {
    label: "I'd like a courtesy car",
    description: "Subject to availability \u2014 we'll confirm when we call."
  }), /*#__PURE__*/React.createElement("div", {
    style: {
      display: 'flex',
      gap: 'var(--space-3)'
    }
  }, /*#__PURE__*/React.createElement(Button, {
    variant: "ghost",
    onClick: () => setStep(1)
  }, "Back"), /*#__PURE__*/React.createElement(Button, {
    onClick: () => setStep(3),
    iconRight: /*#__PURE__*/React.createElement(Icon, {
      name: "arrow-right",
      size: 20
    })
  }, "Continue"))) : /*#__PURE__*/React.createElement("div", {
    style: {
      display: 'grid',
      gap: 'var(--space-5)'
    }
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      display: 'grid',
      gridTemplateColumns: '1fr 1fr',
      gap: 'var(--space-4)'
    }
  }, /*#__PURE__*/React.createElement(Input, {
    label: "Name",
    placeholder: "Full name"
  }), /*#__PURE__*/React.createElement(Input, {
    label: "Phone",
    placeholder: "021 000 0000"
  })), /*#__PURE__*/React.createElement(Input, {
    label: "Email",
    type: "email",
    placeholder: "you@example.co.nz"
  }), /*#__PURE__*/React.createElement(Input, {
    label: "Anything we should know?",
    placeholder: "Noise from the front left under braking\u2026"
  }), /*#__PURE__*/React.createElement(Checkbox, {
    label: "Email me service reminders"
  }), /*#__PURE__*/React.createElement("div", {
    style: {
      display: 'flex',
      gap: 'var(--space-3)'
    }
  }, /*#__PURE__*/React.createElement(Button, {
    variant: "ghost",
    onClick: () => setStep(2)
  }, "Back"), /*#__PURE__*/React.createElement(Button, {
    onClick: () => setDone(true)
  }, "Confirm Booking")))), /*#__PURE__*/React.createElement("aside", {
    style: {
      border: '1px solid var(--border-default)',
      borderRadius: 'var(--radius-md)',
      padding: 'var(--space-6)',
      display: 'grid',
      gap: 'var(--space-4)',
      position: 'sticky',
      top: '100px'
    }
  }, /*#__PURE__*/React.createElement(Eyebrow, null, "Your booking"), [['Vehicle', 'BMW 320i Touring'], ['Rego', 'ABC123'], ['Service', service === 'service' ? 'Scheduled service' : service === 'wof' ? 'WOF inspection' : service === 'diag' ? 'Diagnostics' : 'DSG service'], ['When', 'Thu 13 Aug · ' + slot], ['Where', '23 Norris Ave, Te Rapa']].map(([k, v]) => /*#__PURE__*/React.createElement("div", {
    key: k,
    style: {
      display: 'flex',
      justifyContent: 'space-between',
      gap: 'var(--space-4)',
      font: 'var(--type-body-sm)'
    }
  }, /*#__PURE__*/React.createElement("span", {
    style: {
      color: 'var(--text-muted)'
    }
  }, k), /*#__PURE__*/React.createElement("strong", null, v))), /*#__PURE__*/React.createElement("hr", {
    style: {
      border: 0,
      borderTop: '1px solid var(--border-default)',
      margin: 0
    }
  }), /*#__PURE__*/React.createElement("div", {
    style: {
      display: 'flex',
      gap: 'var(--space-2)',
      flexWrap: 'wrap'
    }
  }, /*#__PURE__*/React.createElement(Badge, {
    tone: "success",
    dot: true
  }, "Courtesy cars available"), /*#__PURE__*/React.createElement(Badge, {
    tone: "neutral"
  }, "MTA member")), /*#__PURE__*/React.createElement("p", {
    style: {
      font: 'var(--type-body-sm)',
      color: 'var(--text-muted)'
    }
  }, "We'll never do extra work without your clear authorisation first.")))), done ? /*#__PURE__*/React.createElement("div", {
    style: {
      position: 'fixed',
      right: 'var(--space-6)',
      bottom: 'var(--space-6)',
      zIndex: 200
    }
  }, /*#__PURE__*/React.createElement(Toast, {
    tone: "success",
    title: "Booking request sent",
    message: "We'll call you on 021 000 0000 to confirm today.",
    onDismiss: () => setDone(false),
    icon: /*#__PURE__*/React.createElement(Icon, {
      name: "check",
      size: 20
    })
  })) : null);
}
Object.assign(window, {
  BookingScreen
});
})(); } catch (e) { __ds_ns.__errors.push({ path: "ui_kits/website/BookingScreen.jsx", error: String((e && e.message) || e) }); }

// ui_kits/website/Chrome.jsx
try { (() => {
const {
  Logo,
  Eyebrow,
  Button,
  IconButton,
  Icon
} = window.SDEuropeanDesignSystem_614674 || {};
const NAV = [{
  label: 'Parts Shop',
  screen: null,
  href: '#'
}, {
  label: 'Dismantling',
  screen: 'dismantling'
}, {
  label: 'Parts',
  screen: 'dismantling'
}, {
  label: 'Servicing',
  screen: 'servicing',
  menu: ['Services', 'WOF Inspection', 'Performance Tuning', 'Audi & VW DSG Servicing', 'Key Cutting', 'Commercial Vehicles']
}, {
  label: 'About',
  screen: null,
  menu: ['Staff', 'Latest News', 'Advice Centre', 'Careers']
}, {
  label: 'Contact',
  screen: 'booking',
  menu: ['Online Booking', 'Sell Your Vehicle']
}];
function UtilityBar() {
  return /*#__PURE__*/React.createElement("div", {
    style: {
      height: 'var(--utilitybar-h)',
      background: 'var(--brand-black)',
      color: 'rgba(255,255,255,.8)',
      display: 'flex',
      alignItems: 'center'
    }
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      maxWidth: 'var(--container-max)',
      width: '100%',
      margin: '0 auto',
      padding: '0 var(--gutter)',
      display: 'flex',
      alignItems: 'center',
      gap: 'var(--space-6)',
      font: 'var(--type-body-sm)'
    }
  }, /*#__PURE__*/React.createElement("a", {
    href: "#",
    style: {
      color: 'var(--brand-yellow)',
      borderBottom: 0,
      font: 'var(--type-label)',
      letterSpacing: 'var(--ls-wide)',
      textTransform: 'uppercase'
    }
  }, "Online Booking"), /*#__PURE__*/React.createElement("span", {
    style: {
      marginLeft: 'auto',
      display: 'inline-flex',
      alignItems: 'center',
      gap: '8px'
    }
  }, /*#__PURE__*/React.createElement(Icon, {
    name: "phone",
    size: 16
  }), "Any questions? Call us on ", /*#__PURE__*/React.createElement("strong", {
    style: {
      color: '#fff'
    }
  }, "0800 269 772")), /*#__PURE__*/React.createElement("span", {
    style: {
      display: 'inline-flex',
      gap: '10px',
      opacity: .7
    }
  }, /*#__PURE__*/React.createElement(Icon, {
    name: "facebook",
    size: 16
  }), /*#__PURE__*/React.createElement(Icon, {
    name: "instagram",
    size: 16
  }), /*#__PURE__*/React.createElement(Icon, {
    name: "linkedin",
    size: 16
  }))));
}
function Header({
  go,
  current
}) {
  const [open, setOpen] = React.useState(null);
  return /*#__PURE__*/React.createElement("header", {
    style: {
      position: 'sticky',
      top: 0,
      zIndex: 50,
      background: 'rgba(255,255,255,.94)',
      backdropFilter: 'blur(10px)',
      borderBottom: '1px solid var(--border-default)'
    }
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      height: 'var(--header-h)',
      maxWidth: 'var(--container-max)',
      margin: '0 auto',
      padding: '0 var(--gutter)',
      display: 'flex',
      alignItems: 'center',
      gap: 'var(--space-8)'
    }
  }, /*#__PURE__*/React.createElement("a", {
    href: "#",
    onClick: e => {
      e.preventDefault();
      go('home');
    },
    style: {
      borderBottom: 0
    }
  }, /*#__PURE__*/React.createElement(Logo, {
    height: 38,
    assetBase: "../../assets"
  })), /*#__PURE__*/React.createElement("nav", {
    style: {
      display: 'flex',
      gap: 'var(--space-6)',
      marginLeft: 'auto'
    }
  }, NAV.map(n => /*#__PURE__*/React.createElement("div", {
    key: n.label,
    style: {
      position: 'relative'
    },
    onMouseEnter: () => setOpen(n.label),
    onMouseLeave: () => setOpen(null)
  }, /*#__PURE__*/React.createElement("button", {
    onClick: () => n.screen && go(n.screen),
    style: {
      background: 'none',
      border: 0,
      padding: '0 0 4px',
      cursor: 'pointer',
      font: 'var(--type-button)',
      fontSize: 'var(--fs-sm)',
      letterSpacing: 'var(--ls-wide)',
      textTransform: 'uppercase',
      color: current === n.screen ? 'var(--text-primary)' : 'var(--text-secondary)',
      borderBottom: '3px solid ' + (current === n.screen ? 'var(--brand-yellow)' : 'transparent'),
      transition: 'var(--transition-control)'
    }
  }, n.label), n.menu && open === n.label ? /*#__PURE__*/React.createElement("div", {
    style: {
      position: 'absolute',
      top: '100%',
      left: '-14px',
      marginTop: '14px',
      background: '#fff',
      border: '1px solid var(--border-default)',
      borderRadius: 'var(--radius-md)',
      boxShadow: 'var(--shadow-md)',
      padding: 'var(--space-2)',
      minWidth: '220px',
      display: 'grid'
    }
  }, n.menu.map(m => /*#__PURE__*/React.createElement("a", {
    key: m,
    href: "#",
    onClick: e => {
      e.preventDefault();
      n.screen && go(n.screen);
    },
    style: {
      borderBottom: 0,
      padding: '9px 12px',
      borderRadius: 'var(--radius-sm)',
      font: 'var(--type-body-sm)',
      color: 'var(--text-primary)'
    }
  }, m))) : null))), /*#__PURE__*/React.createElement(Button, {
    size: "sm",
    onClick: () => go('booking')
  }, "Book a Service")));
}
function Photo({
  label,
  ratio = '16 / 9',
  dark = false,
  children,
  style
}) {
  return /*#__PURE__*/React.createElement("div", {
    style: {
      position: 'relative',
      aspectRatio: ratio,
      width: '100%',
      overflow: 'hidden',
      background: dark ? 'linear-gradient(135deg,#1c1e21,#4a4e54)' : 'linear-gradient(135deg,#d9dbdd,#f1f2f3)',
      display: 'grid',
      placeItems: 'center',
      ...style
    }
  }, /*#__PURE__*/React.createElement("span", {
    style: {
      position: 'absolute',
      font: 'var(--type-label)',
      letterSpacing: 'var(--ls-wide)',
      textTransform: 'uppercase',
      color: dark ? 'rgba(255,255,255,.34)' : 'var(--neutral-400)'
    }
  }, label), children);
}
function Footer() {
  const col = (h, items) => /*#__PURE__*/React.createElement("div", {
    style: {
      display: 'grid',
      gap: 'var(--space-3)',
      alignContent: 'start'
    }
  }, /*#__PURE__*/React.createElement(Eyebrow, {
    tone: "accent"
  }, h), items.map(i => /*#__PURE__*/React.createElement("a", {
    key: i,
    href: "#",
    style: {
      color: 'rgba(255,255,255,.72)',
      borderBottom: 0,
      font: 'var(--type-body-sm)'
    }
  }, i)));
  return /*#__PURE__*/React.createElement("footer", {
    style: {
      background: 'var(--brand-black)',
      color: 'rgba(255,255,255,.72)',
      paddingTop: 'var(--space-16)'
    }
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      maxWidth: 'var(--container-max)',
      margin: '0 auto',
      padding: '0 var(--gutter)'
    }
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      display: 'grid',
      gridTemplateColumns: '1.4fr 1fr 1fr 1fr',
      gap: 'var(--space-10)'
    }
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      display: 'grid',
      gap: 'var(--space-4)',
      alignContent: 'start'
    }
  }, /*#__PURE__*/React.createElement(Logo, {
    variant: "lockup-white",
    height: 34,
    assetBase: "../../assets"
  }), /*#__PURE__*/React.createElement("p", {
    style: {
      font: 'var(--type-body-sm)',
      maxWidth: '280px'
    }
  }, "Your European vehicle experts. Parts and servicing for Audi, BMW, Mercedes, Mini, Skoda and VW \u2014 Hamilton, and NZ wide.")), col('Parts', ['Parts Shop', 'Cars for Dismantling', 'New & Used Parts', 'Sell Your Vehicle']), col('Servicing', ['Services', 'WOF Inspection', 'Performance Tuning', 'DSG Servicing', 'Key Cutting']), col('Company', ['About', 'Staff', 'Careers', 'Latest News', 'Contact'])), /*#__PURE__*/React.createElement("div", {
    style: {
      marginTop: 'var(--space-12)',
      paddingTop: 'var(--space-6)',
      borderTop: '1px solid var(--border-inverse)',
      display: 'flex',
      gap: 'var(--space-8)',
      flexWrap: 'wrap',
      font: 'var(--type-body-sm)'
    }
  }, /*#__PURE__*/React.createElement("span", null, /*#__PURE__*/React.createElement("strong", {
    style: {
      color: '#fff'
    }
  }, "Parts"), " \xB7 177 Ellis Street, Frankton, Hamilton"), /*#__PURE__*/React.createElement("span", null, /*#__PURE__*/React.createElement("strong", {
    style: {
      color: '#fff'
    }
  }, "Service"), " \xB7 23 Norris Ave, Te Rapa, Hamilton"), /*#__PURE__*/React.createElement("span", null, /*#__PURE__*/React.createElement("strong", {
    style: {
      color: '#fff'
    }
  }, "Freephone"), " \xB7 0800 269 772")), /*#__PURE__*/React.createElement("div", {
    style: {
      padding: 'var(--space-6) 0 var(--space-10)',
      font: 'var(--type-body-sm)',
      color: 'rgba(255,255,255,.45)'
    }
  }, "\xA9 Copyright 2026 SD European. All Rights Reserved.", /*#__PURE__*/React.createElement("br", null), "SD European is an independent parts and service specialist and has no association with BMW New Zealand or BMW AG.")));
}
function Section({
  children,
  tone = 'white',
  pad = 'var(--section-y)',
  style
}) {
  const bg = tone === 'grey' ? 'var(--surface-subtle)' : tone === 'black' ? 'var(--brand-black)' : 'var(--surface-page)';
  return /*#__PURE__*/React.createElement("section", {
    style: {
      background: bg,
      padding: pad + ' 0',
      ...style
    }
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      maxWidth: 'var(--container-max)',
      margin: '0 auto',
      padding: '0 var(--gutter)'
    }
  }, children));
}
Object.assign(window, {
  UtilityBar,
  Header,
  Footer,
  Section,
  Photo,
  WEBSITE_NAV: NAV
});
})(); } catch (e) { __ds_ns.__errors.push({ path: "ui_kits/website/Chrome.jsx", error: String((e && e.message) || e) }); }

// ui_kits/website/DismantlingScreen.jsx
try { (() => {
const {
  Button,
  Badge,
  Tag,
  Select,
  Input,
  Eyebrow,
  SectionHeading,
  Icon,
  Card
} = window.SDEuropeanDesignSystem_614674 || {};
const VEHICLES = [{
  make: 'BMW',
  model: '320i E91 Touring',
  year: '2008',
  eng: 'N46B20',
  stock: 'SD-10442',
  state: 'Now parting out'
}, {
  make: 'Audi',
  model: 'A4 B8 2.0 TFSI',
  year: '2011',
  eng: 'CDNC',
  stock: 'SD-10438',
  state: 'Now parting out'
}, {
  make: 'Volkswagen',
  model: 'Golf Mk6 GTI',
  year: '2010',
  eng: 'CCZB',
  stock: 'SD-10431',
  state: 'Arriving'
}, {
  make: 'Mercedes-Benz',
  model: 'C200 W204',
  year: '2009',
  eng: 'M271',
  stock: 'SD-10427',
  state: 'Now parting out'
}, {
  make: 'Mini',
  model: 'Cooper S R56',
  year: '2012',
  eng: 'N18',
  stock: 'SD-10419',
  state: 'Now parting out'
}, {
  make: 'Skoda',
  model: 'Octavia vRS 1Z',
  year: '2010',
  eng: 'CDAA',
  stock: 'SD-10411',
  state: 'Last few parts'
}];
const MARQUES = ['All', 'Audi', 'BMW', 'Mercedes-Benz', 'Mini', 'Skoda', 'Volkswagen', 'Land Rover'];
function DismantlingScreen({
  go
}) {
  const [marque, setMarque] = React.useState('All');
  const list = marque === 'All' ? VEHICLES : VEHICLES.filter(v => v.make === marque);
  return /*#__PURE__*/React.createElement("main", null, /*#__PURE__*/React.createElement(Section, {
    tone: "grey",
    pad: "var(--space-12)"
  }, /*#__PURE__*/React.createElement(SectionHeading, {
    eyebrow: "Used Parts",
    title: "Cars for Dismantling",
    lead: "Over 350 vehicles in the yard. Send through your rego and our parts team will match your VIN to the exact part you need."
  })), /*#__PURE__*/React.createElement(Section, {
    pad: "var(--space-10)"
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      display: 'grid',
      gridTemplateColumns: '1.6fr 1fr 1fr auto',
      gap: 'var(--space-4)',
      alignItems: 'end',
      paddingBottom: 'var(--space-6)',
      borderBottom: '1px solid var(--border-default)'
    }
  }, /*#__PURE__*/React.createElement(Input, {
    label: "Search the yard",
    placeholder: "e.g. E91 tailgate",
    prefix: /*#__PURE__*/React.createElement(Icon, {
      name: "search",
      size: 20
    })
  }), /*#__PURE__*/React.createElement(Select, {
    label: "Model year",
    options: ['Any year', '2015+', '2010–2014', '2005–2009', 'Pre-2005']
  }), /*#__PURE__*/React.createElement(Select, {
    label: "Sort by",
    options: ['Newest arrivals', 'Make A–Z', 'Year, newest']
  }), /*#__PURE__*/React.createElement(Button, {
    variant: "secondary"
  }, "Apply")), /*#__PURE__*/React.createElement("div", {
    style: {
      display: 'flex',
      gap: 'var(--space-2)',
      flexWrap: 'wrap',
      margin: 'var(--space-6) 0 var(--space-8)'
    }
  }, MARQUES.map(m => /*#__PURE__*/React.createElement(Tag, {
    key: m,
    selected: marque === m,
    onClick: () => setMarque(m)
  }, m))), /*#__PURE__*/React.createElement("div", {
    style: {
      display: 'grid',
      gridTemplateColumns: 'repeat(3,1fr)',
      gap: 'var(--space-6)'
    }
  }, list.map(v => /*#__PURE__*/React.createElement("div", {
    key: v.stock,
    style: {
      border: '1px solid var(--border-default)',
      borderRadius: 'var(--radius-lg)',
      overflow: 'hidden',
      background: '#fff'
    }
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      position: 'relative'
    }
  }, /*#__PURE__*/React.createElement(Photo, {
    label: "Vehicle in the yard",
    ratio: "4 / 3"
  }), /*#__PURE__*/React.createElement("span", {
    style: {
      position: 'absolute',
      top: '12px',
      left: '12px'
    }
  }, /*#__PURE__*/React.createElement(Badge, {
    tone: v.state === 'Arriving' ? 'info' : v.state === 'Last few parts' ? 'warning' : 'success',
    dot: true
  }, v.state))), /*#__PURE__*/React.createElement("div", {
    style: {
      padding: 'var(--space-5)',
      display: 'grid',
      gap: 'var(--space-2)'
    }
  }, /*#__PURE__*/React.createElement(Eyebrow, null, v.make), /*#__PURE__*/React.createElement("h3", {
    style: {
      font: 'var(--type-h4)',
      fontSize: 'var(--fs-lg)'
    }
  }, v.model), /*#__PURE__*/React.createElement("div", {
    style: {
      display: 'flex',
      gap: 'var(--space-4)',
      font: 'var(--type-body-sm)',
      color: 'var(--text-secondary)'
    }
  }, /*#__PURE__*/React.createElement("span", null, v.year), /*#__PURE__*/React.createElement("span", null, "\xB7"), /*#__PURE__*/React.createElement("span", {
    style: {
      font: 'var(--type-mono)'
    }
  }, v.eng)), /*#__PURE__*/React.createElement("div", {
    style: {
      display: 'flex',
      alignItems: 'center',
      gap: 'var(--space-3)',
      marginTop: 'var(--space-3)'
    }
  }, /*#__PURE__*/React.createElement(Button, {
    size: "sm",
    fullWidth: true
  }, "Enquire About Parts")), /*#__PURE__*/React.createElement("span", {
    style: {
      font: 'var(--type-mono)',
      color: 'var(--text-muted)',
      fontSize: 'var(--fs-xs)'
    }
  }, "Stock ", v.stock)))))), /*#__PURE__*/React.createElement(Section, {
    tone: "black"
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      display: 'grid',
      gridTemplateColumns: '1fr auto',
      gap: 'var(--space-8)',
      alignItems: 'center'
    }
  }, /*#__PURE__*/React.createElement(SectionHeading, {
    inverse: true,
    eyebrow: "Not sure what you need?",
    title: "Send us your rego and we'll match it",
    lead: "We've a team of experts on the end of a phone or email line, equipped with parts catalogues and a heap of knowledge."
  }), /*#__PURE__*/React.createElement("div", {
    style: {
      display: 'flex',
      gap: 'var(--space-3)'
    }
  }, /*#__PURE__*/React.createElement(Button, {
    variant: "inverse",
    size: "lg",
    iconLeft: /*#__PURE__*/React.createElement(Icon, {
      name: "phone",
      size: 20
    })
  }, "0800 269 772"), /*#__PURE__*/React.createElement(Button, {
    size: "lg",
    onClick: () => go('booking')
  }, "Send Your Rego")))));
}
Object.assign(window, {
  DismantlingScreen
});
})(); } catch (e) { __ds_ns.__errors.push({ path: "ui_kits/website/DismantlingScreen.jsx", error: String((e && e.message) || e) }); }

// ui_kits/website/HomeScreen.jsx
try { (() => {
const {
  Button,
  Card,
  Badge,
  Eyebrow,
  SectionHeading,
  Icon
} = window.SDEuropeanDesignSystem_614674 || {};
const SLIDES = [{
  eyebrow: 'Audi | BMW | Mercedes | Mini | Skoda | VW',
  title: 'Your Parts & Servicing Specialists',
  cta: 'Shop Parts'
}, {
  eyebrow: 'Audi | BMW | Mercedes | Mini | Skoda | VW',
  title: "Used & Aftermarket Bumpers in Stock",
  cta: 'Browse Dismantling'
}, {
  eyebrow: 'Audi | BMW | Mercedes | Mini | Skoda | VW',
  title: "NZ's Home Of European Cars",
  cta: 'Book a Service'
}];
const MARQUES = ['Audi', 'BMW', 'Mercedes-Benz', 'Mini', 'Skoda', 'Volkswagen', 'Land Rover'];
const HELP = [['wrench', 'Audi, BMW, Mercedes, Mini, Skoda & VW servicing'], ['package', 'New and used parts, in stock in Hamilton'], ['truck', 'Shipping NZ wide — overnight in most cases'], ['shield-check', 'Authorised mechanical warranty repairers'], ['badge-check', 'MTA member & licensed motor vehicle trader'], ['car', 'Courtesy cars available'], ['clock', 'In business for over 30 years'], ['credit-card', 'Visa / Mastercard accepted']];
function HomeScreen({
  go
}) {
  const [slide, setSlide] = React.useState(0);
  React.useEffect(() => {
    const t = setInterval(() => setSlide(s => (s + 1) % SLIDES.length), 5200);
    return () => clearInterval(t);
  }, []);
  const s = SLIDES[slide];
  return /*#__PURE__*/React.createElement("main", null, /*#__PURE__*/React.createElement("div", {
    style: {
      position: 'relative',
      height: '520px',
      background: 'linear-gradient(135deg,#17191c,#43474d)',
      overflow: 'hidden'
    }
  }, /*#__PURE__*/React.createElement("span", {
    style: {
      position: 'absolute',
      inset: 0,
      display: 'grid',
      placeItems: 'center',
      font: 'var(--type-label)',
      letterSpacing: 'var(--ls-wide)',
      textTransform: 'uppercase',
      color: 'rgba(255,255,255,.22)'
    }
  }, "Full-bleed workshop photography"), /*#__PURE__*/React.createElement("div", {
    style: {
      position: 'absolute',
      inset: 0,
      background: 'var(--scrim-bottom)'
    }
  }), /*#__PURE__*/React.createElement("div", {
    style: {
      position: 'absolute',
      inset: 0,
      display: 'grid',
      alignItems: 'end'
    }
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      maxWidth: 'var(--container-max)',
      width: '100%',
      margin: '0 auto',
      padding: '0 var(--gutter) var(--space-16)'
    }
  }, /*#__PURE__*/React.createElement("div", {
    key: slide,
    style: {
      display: 'grid',
      gap: 'var(--space-4)',
      maxWidth: '740px',
      animation: 'sdRise var(--dur-slow) var(--ease-out) both'
    }
  }, /*#__PURE__*/React.createElement(Eyebrow, {
    tone: "accent"
  }, s.eyebrow.toUpperCase().replace(/ \| /g, ' | ')), /*#__PURE__*/React.createElement("h1", {
    style: {
      font: 'var(--type-display)',
      fontSize: '58px',
      color: '#fff',
      letterSpacing: 'var(--ls-tight)'
    }
  }, s.title), /*#__PURE__*/React.createElement("div", null, /*#__PURE__*/React.createElement(Button, {
    size: "lg",
    iconRight: /*#__PURE__*/React.createElement(Icon, {
      name: "arrow-right",
      size: 20
    })
  }, s.cta))))), /*#__PURE__*/React.createElement("div", {
    style: {
      position: 'absolute',
      bottom: 'var(--space-6)',
      right: 'calc(50% - var(--container-max) / 2 + var(--gutter))',
      display: 'flex',
      gap: '8px'
    }
  }, SLIDES.map((_, i) => /*#__PURE__*/React.createElement("button", {
    key: i,
    onClick: () => setSlide(i),
    "aria-label": 'Slide ' + (i + 1),
    style: {
      width: i === slide ? '28px' : '10px',
      height: '4px',
      border: 0,
      padding: 0,
      background: i === slide ? 'var(--brand-yellow)' : 'rgba(255,255,255,.4)',
      cursor: 'pointer',
      transition: 'width var(--dur-base) var(--ease-standard)'
    }
  })))), /*#__PURE__*/React.createElement(Section, {
    pad: "var(--space-12)"
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      display: 'grid',
      gridTemplateColumns: 'repeat(3,1fr)',
      gap: 'var(--space-6)',
      marginTop: '-84px',
      position: 'relative'
    }
  }, [['Dismantling', 'Over 350 vehicles being parted out right now.', 'dismantling'], ['Parts', 'New aftermarket, OEM and quality used parts.', 'dismantling'], ['Servicing', 'Independent specialist workshop in Te Rapa.', 'servicing']].map(([t, d, sc]) => /*#__PURE__*/React.createElement("div", {
    key: t,
    onClick: () => go(sc),
    style: {
      background: 'var(--brand-black)',
      color: '#fff',
      padding: 'var(--space-8)',
      borderRadius: 'var(--radius-md)',
      display: 'grid',
      gap: 'var(--space-2)',
      cursor: 'pointer',
      boxShadow: 'var(--shadow-lg)'
    }
  }, /*#__PURE__*/React.createElement("h3", {
    style: {
      font: 'var(--type-h3)',
      color: '#fff'
    }
  }, t), /*#__PURE__*/React.createElement("p", {
    style: {
      font: 'var(--type-body-sm)',
      color: 'rgba(255,255,255,.7)'
    }
  }, d), /*#__PURE__*/React.createElement("span", {
    style: {
      marginTop: 'var(--space-3)',
      color: 'var(--brand-yellow)',
      font: 'var(--type-button)',
      letterSpacing: 'var(--ls-wide)',
      textTransform: 'uppercase',
      display: 'inline-flex',
      alignItems: 'center',
      gap: '8px'
    }
  }, "Explore ", /*#__PURE__*/React.createElement(Icon, {
    name: "arrow-right",
    size: 16
  })))))), /*#__PURE__*/React.createElement(Section, {
    tone: "grey"
  }, /*#__PURE__*/React.createElement(SectionHeading, {
    eyebrow: "Latest Arrivals",
    title: "Now Dismantling Car Parts For:"
  }), /*#__PURE__*/React.createElement("div", {
    style: {
      display: 'grid',
      gridTemplateColumns: 'repeat(7,1fr)',
      gap: 'var(--space-4)',
      marginTop: 'var(--space-8)'
    }
  }, MARQUES.map(m => /*#__PURE__*/React.createElement("div", {
    key: m,
    onClick: () => go('dismantling'),
    style: {
      cursor: 'pointer',
      borderRadius: 'var(--radius-lg)',
      overflow: 'hidden',
      border: '1px solid var(--border-default)',
      background: '#fff'
    }
  }, /*#__PURE__*/React.createElement(Photo, {
    label: "Vehicle",
    ratio: "4 / 3"
  }), /*#__PURE__*/React.createElement("div", {
    style: {
      padding: 'var(--space-3)',
      font: 'var(--type-h4)',
      textAlign: 'center'
    }
  }, m))))), /*#__PURE__*/React.createElement(Section, null, /*#__PURE__*/React.createElement(SectionHeading, {
    eyebrow: "Parts & Service",
    title: "European Car Parts Supplier and Vehicle Repair Workshop"
  }), /*#__PURE__*/React.createElement("div", {
    style: {
      display: 'grid',
      gridTemplateColumns: '1fr 1.1fr',
      gap: 'var(--space-12)',
      marginTop: 'var(--space-8)',
      alignItems: 'start'
    }
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      borderRadius: 'var(--radius-lg)',
      overflow: 'hidden'
    }
  }, /*#__PURE__*/React.createElement(Photo, {
    label: "Aftermarket parts on the shelf",
    ratio: "6 / 5"
  })), /*#__PURE__*/React.createElement("div", {
    style: {
      display: 'grid',
      gap: 'var(--space-5)'
    }
  }, /*#__PURE__*/React.createElement("div", null, /*#__PURE__*/React.createElement("h3", {
    style: {
      font: 'var(--type-h3)'
    }
  }, "Parts \u2014 New & Used"), /*#__PURE__*/React.createElement("p", {
    style: {
      font: 'var(--type-body)',
      color: 'var(--text-secondary)',
      marginTop: 'var(--space-2)'
    }
  }, "Looking for parts for your car or workshop? Then you've made it to the right place, as we're one of NZ's largest suppliers of New and Used European parts. With stock of over 350 Audi, BMW, Mercedes, Mini, Skoda & VW and new arrivals every day, we probably have the part you need."), /*#__PURE__*/React.createElement("p", {
    style: {
      font: 'var(--type-body)',
      color: 'var(--text-secondary)',
      marginTop: 'var(--space-3)'
    }
  }, "We're based in Hamilton, but you're probably in Auckland, Tauranga, Wellington, Christchurch or Invercargill? That's no problem, because all our parts are in stock and ready to fire out to you overnight NZ wide.")), /*#__PURE__*/React.createElement("div", null, /*#__PURE__*/React.createElement("h3", {
    style: {
      font: 'var(--type-h3)'
    }
  }, "Mechanical Workshop in Te Rapa"), /*#__PURE__*/React.createElement("p", {
    style: {
      font: 'var(--type-body)',
      color: 'var(--text-secondary)',
      marginTop: 'var(--space-2)'
    }
  }, "We're an independent specialist workshop, which means we have special equipment, knowledge and parts in stock for your Euro vehicle \u2014 without expensive dealership licensing fees.")), /*#__PURE__*/React.createElement("div", {
    style: {
      display: 'flex',
      gap: 'var(--space-3)'
    }
  }, /*#__PURE__*/React.createElement(Button, {
    onClick: () => go('dismantling')
  }, "Browse Parts"), /*#__PURE__*/React.createElement(Button, {
    variant: "outline",
    onClick: () => go('servicing')
  }, "Learn More About Servicing"))))), /*#__PURE__*/React.createElement("div", {
    style: {
      position: 'relative',
      overflow: 'hidden',
      background: 'var(--brand-black)'
    }
  }, /*#__PURE__*/React.createElement("img", {
    src: "../../assets/symbol-black-yellow.png",
    alt: "",
    style: {
      position: 'absolute',
      right: '-160px',
      top: '-120px',
      width: '620px',
      opacity: .08,
      filter: 'grayscale(1) brightness(3)'
    }
  }), /*#__PURE__*/React.createElement(Section, {
    tone: "black",
    style: {
      position: 'relative',
      background: 'transparent'
    }
  }, /*#__PURE__*/React.createElement(SectionHeading, {
    inverse: true,
    eyebrow: "Why SD European",
    title: "How we can help you"
  }), /*#__PURE__*/React.createElement("div", {
    style: {
      display: 'grid',
      gridTemplateColumns: 'repeat(4,1fr)',
      gap: 'var(--space-6) var(--space-8)',
      marginTop: 'var(--space-10)'
    }
  }, HELP.map(([ic, t]) => /*#__PURE__*/React.createElement("div", {
    key: t,
    style: {
      display: 'flex',
      gap: 'var(--space-3)',
      color: '#fff'
    }
  }, /*#__PURE__*/React.createElement("span", {
    style: {
      color: 'var(--brand-yellow)'
    }
  }, /*#__PURE__*/React.createElement(Icon, {
    name: ic,
    size: 24
  })), /*#__PURE__*/React.createElement("span", {
    style: {
      font: 'var(--type-body-sm)',
      color: 'rgba(255,255,255,.82)'
    }
  }, t)))))), /*#__PURE__*/React.createElement(Section, {
    tone: "grey",
    pad: "var(--space-12)"
  }, /*#__PURE__*/React.createElement(Eyebrow, null, "Authorised mechanical repairers for these companies and more"), /*#__PURE__*/React.createElement("div", {
    style: {
      display: 'grid',
      gridTemplateColumns: 'repeat(5,1fr)',
      gap: 'var(--space-6)',
      marginTop: 'var(--space-6)'
    }
  }, ['Provident Insurance', 'Autosure NZ', 'Protecta Insurance', 'AA Insurance', 'Janssen Insurance'].map(n => /*#__PURE__*/React.createElement("div", {
    key: n,
    style: {
      height: '72px',
      background: '#fff',
      border: '1px solid var(--border-default)',
      borderRadius: 'var(--radius-md)',
      display: 'grid',
      placeItems: 'center',
      font: 'var(--type-label)',
      color: 'var(--text-muted)',
      textAlign: 'center',
      padding: '0 8px'
    }
  }, n))), /*#__PURE__*/React.createElement("p", {
    style: {
      font: 'var(--type-body-sm)',
      color: 'var(--text-muted)',
      marginTop: 'var(--space-4)'
    }
  }, "Partner marks are placeholders \u2014 supply the real logo files and we'll drop them in.")));
}
Object.assign(window, {
  HomeScreen
});
})(); } catch (e) { __ds_ns.__errors.push({ path: "ui_kits/website/HomeScreen.jsx", error: String((e && e.message) || e) }); }

// ui_kits/website/ServicingScreen.jsx
try { (() => {
const {
  Button,
  Card,
  Badge,
  Eyebrow,
  SectionHeading,
  Icon,
  Tabs
} = window.SDEuropeanDesignSystem_614674 || {};
const SERVICES = [['wrench', 'Servicing', 'Scheduled and interim servicing for Audi, BMW, Mercedes, Mini, Skoda and VW.'], ['clipboard-check', 'WOF Inspection', 'Warrant of Fitness, booked in and out the same day at Te Rapa.'], ['gauge', 'Performance Tuning', 'ECU remapping and performance work by specialists who know the platform.'], ['cog', 'Audi & VW DSG Servicing', 'DSG fluid and filter service using the correct fill procedure and specification.'], ['key-round', 'Key Cutting', 'Replacement and spare keys for BMW, Audi and VW — cut and coded in house.'], ['truck', 'Commercial Vehicles', 'Fleet and commercial servicing with minimum downtime.']];
const REASONS = [['Expertise', "We're a fully independent specialist garage who only work on European cars, with the tools and the parts on the shelf."], ['Trustworthy', "After you hand over your keys, you'll drive off in the loan car with your mind at ease."], ['Excellent Communication', "Later in the day you'll get a friendly update. We never do extra work without your clear authorisation first — so no nasty surprises at the eftpos machine."], ['Value', 'A family business with 30 years experience, known for excellent value for money.']];
const REVIEWS = [['These people make you feel welcome and valued every time you call or have your vehicle serviced. Highly recommended! I drive 1.5 hours to get my Audi serviced here.', 'Steve Dyer'], ['SD European are always fast, professional, and efficient with their work. I highly recommend them.', 'Daniel Compton, Hamilton'], ['Moved to SD European and am incredibly impressed with their workmanship, cleanliness and customer service!', 'Sue Watson']];
function ServicingScreen({
  go
}) {
  const [tab, setTab] = React.useState('why');
  return /*#__PURE__*/React.createElement("main", null, /*#__PURE__*/React.createElement("div", {
    style: {
      position: 'relative',
      height: '360px',
      background: 'linear-gradient(135deg,#1c1e21,#4a4e54)'
    }
  }, /*#__PURE__*/React.createElement("span", {
    style: {
      position: 'absolute',
      inset: 0,
      display: 'grid',
      placeItems: 'center',
      font: 'var(--type-label)',
      letterSpacing: 'var(--ls-wide)',
      textTransform: 'uppercase',
      color: 'rgba(255,255,255,.22)'
    }
  }, "Technician at work \u2014 close-up"), /*#__PURE__*/React.createElement("div", {
    style: {
      position: 'absolute',
      inset: 0,
      background: 'var(--scrim-left)'
    }
  }), /*#__PURE__*/React.createElement("div", {
    style: {
      position: 'absolute',
      inset: 0,
      display: 'grid',
      alignItems: 'center'
    }
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      maxWidth: 'var(--container-max)',
      width: '100%',
      margin: '0 auto',
      padding: '0 var(--gutter)'
    }
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      maxWidth: '620px',
      display: 'grid',
      gap: 'var(--space-4)'
    }
  }, /*#__PURE__*/React.createElement(Eyebrow, {
    tone: "accent"
  }, "Service Centre \xB7 Te Rapa"), /*#__PURE__*/React.createElement("h1", {
    style: {
      font: 'var(--type-h1)',
      color: '#fff'
    }
  }, "European Car Service & Repairs"), /*#__PURE__*/React.createElement("p", {
    style: {
      font: 'var(--type-lead)',
      color: 'rgba(255,255,255,.8)'
    }
  }, "Does your BMW, Volkswagen, Mercedes, Audi, Mini or Skoda need servicing? We are a fully independent specialist garage."), /*#__PURE__*/React.createElement("div", null, /*#__PURE__*/React.createElement(Button, {
    size: "lg",
    onClick: () => go('booking')
  }, "Book a Service")))))), /*#__PURE__*/React.createElement(Section, null, /*#__PURE__*/React.createElement(SectionHeading, {
    eyebrow: "What we do",
    title: "Our Services"
  }), /*#__PURE__*/React.createElement("div", {
    style: {
      display: 'grid',
      gridTemplateColumns: 'repeat(3,1fr)',
      gap: 'var(--space-6)',
      marginTop: 'var(--space-8)'
    }
  }, SERVICES.map(([ic, t, d]) => /*#__PURE__*/React.createElement(Card, {
    key: t,
    interactive: true,
    onClick: () => go('booking'),
    title: t,
    footer: /*#__PURE__*/React.createElement("span", {
      style: {
        font: 'var(--type-button)',
        letterSpacing: 'var(--ls-wide)',
        textTransform: 'uppercase',
        display: 'inline-flex',
        alignItems: 'center',
        gap: '8px'
      }
    }, "Book ", /*#__PURE__*/React.createElement(Icon, {
      name: "arrow-right",
      size: 16
    }))
  }, /*#__PURE__*/React.createElement("span", {
    style: {
      display: 'block',
      marginBottom: 'var(--space-2)'
    }
  }, /*#__PURE__*/React.createElement(Icon, {
    name: ic,
    size: 32
  })), d)))), /*#__PURE__*/React.createElement(Section, {
    tone: "grey"
  }, /*#__PURE__*/React.createElement(Tabs, {
    items: [{
      value: 'why',
      label: 'Why us'
    }, {
      value: 'reviews',
      label: 'Reviews',
      count: 3
    }, {
      value: 'visit',
      label: 'Visiting'
    }],
    value: tab,
    onChange: setTab
  }), /*#__PURE__*/React.createElement("div", {
    style: {
      marginTop: 'var(--space-8)'
    }
  }, tab === 'why' ? /*#__PURE__*/React.createElement("div", {
    style: {
      display: 'grid',
      gridTemplateColumns: 'repeat(4,1fr)',
      gap: 'var(--space-6)'
    }
  }, REASONS.map(([t, d]) => /*#__PURE__*/React.createElement("div", {
    key: t,
    style: {
      display: 'grid',
      gap: 'var(--space-2)',
      alignContent: 'start'
    }
  }, /*#__PURE__*/React.createElement("hr", {
    className: "sd-rule",
    style: {
      margin: 0
    }
  }), /*#__PURE__*/React.createElement("h3", {
    style: {
      font: 'var(--type-h4)',
      marginTop: 'var(--space-2)'
    }
  }, t), /*#__PURE__*/React.createElement("p", {
    style: {
      font: 'var(--type-body-sm)',
      color: 'var(--text-secondary)'
    }
  }, d)))) : tab === 'reviews' ? /*#__PURE__*/React.createElement("div", {
    style: {
      display: 'grid',
      gridTemplateColumns: 'repeat(3,1fr)',
      gap: 'var(--space-6)'
    }
  }, REVIEWS.map(([q, who]) => /*#__PURE__*/React.createElement("blockquote", {
    key: who,
    style: {
      margin: 0,
      background: '#fff',
      border: '1px solid var(--border-default)',
      borderRadius: 'var(--radius-md)',
      padding: 'var(--space-6)',
      display: 'grid',
      gap: 'var(--space-3)'
    }
  }, /*#__PURE__*/React.createElement("span", {
    style: {
      color: 'var(--brand-yellow)',
      display: 'flex',
      gap: '2px'
    }
  }, [0, 1, 2, 3, 4].map(i => /*#__PURE__*/React.createElement(Icon, {
    key: i,
    name: "star",
    size: 16
  }))), /*#__PURE__*/React.createElement("p", {
    style: {
      font: 'var(--type-body)'
    }
  }, "\u201C", q, "\u201D"), /*#__PURE__*/React.createElement("cite", {
    style: {
      font: 'var(--type-label)',
      letterSpacing: 'var(--ls-wide)',
      textTransform: 'uppercase',
      color: 'var(--text-muted)',
      fontStyle: 'normal'
    }
  }, who)))) : /*#__PURE__*/React.createElement("div", {
    style: {
      display: 'grid',
      gridTemplateColumns: '1fr 1fr',
      gap: 'var(--space-6)'
    }
  }, [['Service Centre', '23 Norris Avenue, Te Rapa, Hamilton 3200', 'By the crossroads of Norris Ave and Bryant Road.'], ['Parts Centre', '177 Ellis Street, Frankton, Hamilton 3204', 'Trade and retail counter, open Saturday mornings.']].map(([t, a, d]) => /*#__PURE__*/React.createElement("div", {
    key: t,
    style: {
      background: '#fff',
      border: '1px solid var(--border-default)',
      borderRadius: 'var(--radius-md)',
      overflow: 'hidden'
    }
  }, /*#__PURE__*/React.createElement(Photo, {
    label: "Map",
    ratio: "21 / 9"
  }), /*#__PURE__*/React.createElement("div", {
    style: {
      padding: 'var(--space-6)',
      display: 'grid',
      gap: 'var(--space-2)'
    }
  }, /*#__PURE__*/React.createElement("h3", {
    style: {
      font: 'var(--type-h4)'
    }
  }, t), /*#__PURE__*/React.createElement("p", {
    style: {
      font: 'var(--type-body)'
    }
  }, a), /*#__PURE__*/React.createElement("p", {
    style: {
      font: 'var(--type-body-sm)',
      color: 'var(--text-muted)'
    }
  }, d))))))));
}
Object.assign(window, {
  ServicingScreen
});
})(); } catch (e) { __ds_ns.__errors.push({ path: "ui_kits/website/ServicingScreen.jsx", error: String((e && e.message) || e) }); }

__ds_ns.Eyebrow = __ds_scope.Eyebrow;

__ds_ns.Logo = __ds_scope.Logo;

__ds_ns.SectionHeading = __ds_scope.SectionHeading;

__ds_ns.Badge = __ds_scope.Badge;

__ds_ns.Button = __ds_scope.Button;

__ds_ns.Card = __ds_scope.Card;

__ds_ns.Icon = __ds_scope.Icon;

__ds_ns.IconButton = __ds_scope.IconButton;

__ds_ns.Tag = __ds_scope.Tag;

__ds_ns.Dialog = __ds_scope.Dialog;

__ds_ns.Toast = __ds_scope.Toast;

__ds_ns.Tooltip = __ds_scope.Tooltip;

__ds_ns.Checkbox = __ds_scope.Checkbox;

__ds_ns.Input = __ds_scope.Input;

__ds_ns.Radio = __ds_scope.Radio;

__ds_ns.Select = __ds_scope.Select;

__ds_ns.Switch = __ds_scope.Switch;

__ds_ns.Tabs = __ds_scope.Tabs;

})();
