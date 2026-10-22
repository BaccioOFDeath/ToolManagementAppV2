import * as React from 'react';

/** Small pill for state: stock level, warranty, fitment, promo. Pill radius is reserved for badges and filter chips. */
export interface BadgeProps extends React.HTMLAttributes<HTMLSpanElement> {
  tone?: 'neutral' | 'accent' | 'dark' | 'success' | 'warning' | 'danger' | 'info';
  size?: 'sm' | 'md';
  /** Leading status dot. */
  dot?: boolean;
  children?: React.ReactNode;
}
export declare function Badge(props: BadgeProps): JSX.Element;
