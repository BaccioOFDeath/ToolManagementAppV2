import * as React from 'react';

/** A square-or-circular control holding a single icon. Always needs an accessible label. */
export interface IconButtonProps extends React.ButtonHTMLAttributes<HTMLButtonElement> {
  icon: React.ReactNode;
  /** Required — becomes aria-label. */
  label: string;
  variant?: 'ghost' | 'solid' | 'accent' | 'outline';
  size?: 'sm' | 'md' | 'lg';
  shape?: 'circle' | 'square';
  disabled?: boolean;
}
export declare function IconButton(props: IconButtonProps): JSX.Element;
