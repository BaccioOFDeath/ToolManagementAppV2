import * as React from 'react';

/**
 * Lucide icon at the brand's stroke weight and size scale.
 * SUBSTITUTION: SD European has no proprietary icon set; Lucide stands in until one is supplied.
 */
export interface IconProps extends React.HTMLAttributes<HTMLSpanElement> {
  /** Lucide icon name, kebab-case, e.g. 'phone', 'arrow-right', 'shopping-cart'. */
  name: string;
  /** 16 inline · 20 in controls · 24 standalone/nav · 32 feature blocks. Never above 40. */
  size?: 16 | 20 | 24 | 32 | 40;
  strokeWidth?: 1.5 | 2;
  /** Accessible name. Omit for decorative icons. */
  label?: string;
}
export declare function Icon(props: IconProps): JSX.Element;
