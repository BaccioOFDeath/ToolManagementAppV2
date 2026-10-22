import * as React from 'react';

/** The brand's ALL-CAPS wide-tracked label. Sits above section headings and carries marque lists. */
export interface EyebrowProps extends React.HTMLAttributes<HTMLElement> {
  tone?: 'muted' | 'accent' | 'inverse';
  as?: keyof JSX.IntrinsicElements;
  children?: React.ReactNode;
}
export declare function Eyebrow(props: EyebrowProps): JSX.Element;
