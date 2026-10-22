import * as React from 'react';

/**
 * @startingPoint section="Core" subtitle="Primary, secondary, outline, ghost and inverse buttons in three sizes" viewport="700x220"
 * The brand's primary call to action. Yellow is reserved for the single most important action per view.
 */
export interface ButtonProps extends React.ButtonHTMLAttributes<HTMLButtonElement> {
  /** primary = yellow (one per view) · secondary = black · outline/ghost = supporting · inverse = white, on black only. */
  variant?: 'primary' | 'secondary' | 'outline' | 'ghost' | 'inverse';
  size?: 'sm' | 'md' | 'lg';
  iconLeft?: React.ReactNode;
  iconRight?: React.ReactNode;
  fullWidth?: boolean;
  disabled?: boolean;
  /** Renders an <a> instead of a <button>. */
  href?: string;
  as?: keyof JSX.IntrinsicElements;
  children?: React.ReactNode;
}
export declare function Button(props: ButtonProps): JSX.Element;
