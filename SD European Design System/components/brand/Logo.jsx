import React from 'react';

const SRC = {
  'lockup-black': 'logo-horizontal-black.png',
  'lockup-white': 'logo-horizontal-white.png',
  'symbol': 'symbol-black-yellow.png'
};

export function Logo({ variant = 'lockup-black', height = 40, assetBase = '/assets', href, className, style, ...rest }) {
  const isSymbol = variant === 'symbol';
  const img = (
    <img
      src={assetBase + '/' + SRC[variant]}
      alt="SD European"
      style={{ height: height + 'px', width: 'auto', display: 'block' }}
    />
  );
  const wrap = {
    display: 'inline-block',
    padding: isSymbol ? 0 : `0 ${Math.round(height * 0.18)}px`,
    border: 0,
    ...style
  };
  if (href) return <a href={href} className={className} style={{ ...wrap, borderBottom: 0 }} {...rest}>{img}</a>;
  return <span className={className} style={wrap} {...rest}>{img}</span>;
}
