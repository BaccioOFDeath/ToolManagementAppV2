import * as React from 'react';

/**
 * The SD European lockup or standalone symbol, with brand-mandated clear space.
 * Never recolour, stretch or add effects to the mark.
 */
export interface LogoProps extends React.HTMLAttributes<HTMLElement> {
  /** Which mark to render. Use lockup-white only on black or dark photography. */
  variant?: 'lockup-black' | 'lockup-white' | 'symbol';
  /** Rendered height in px. Lockup should not go below 24px (≈140px wide); use 'symbol' instead. */
  height?: number;
  /** Path prefix to the design system's assets folder. */
  assetBase?: string;
  /** Wraps the mark in a link (typically home). */
  href?: string;
}
export declare function Logo(props: LogoProps): JSX.Element;
