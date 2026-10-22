import * as React from 'react';

/** Eyebrow + heading + 56×4px yellow rule + optional lead — the standard opener for every page section. */
export interface SectionHeadingProps extends React.HTMLAttributes<HTMLElement> {
  eyebrow?: string;
  title: React.ReactNode;
  lead?: React.ReactNode;
  /** 1 for page hero, 2 for sections (default), 3 for sub-sections. */
  level?: 1 | 2 | 3;
  align?: 'left' | 'center';
  /** Set on black bands. */
  inverse?: boolean;
  rule?: boolean;
}
export declare function SectionHeading(props: SectionHeadingProps): JSX.Element;
