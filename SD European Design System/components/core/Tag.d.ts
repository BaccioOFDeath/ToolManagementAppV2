import * as React from 'react';

/** A filter chip or removable token. Square-ish (4px) — distinct from the pill-shaped Badge. */
export interface TagProps extends React.HTMLAttributes<HTMLSpanElement> {
  selected?: boolean;
  /** Renders a × affordance. */
  onRemove?: (e: React.MouseEvent) => void;
  children?: React.ReactNode;
}
export declare function Tag(props: TagProps): JSX.Element;
