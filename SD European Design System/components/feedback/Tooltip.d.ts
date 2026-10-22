import * as React from 'react';

/** Black label revealed on hover/focus. For clarification only — never for essential information. */
export interface TooltipProps extends React.HTMLAttributes<HTMLSpanElement> {
  label: React.ReactNode;
  placement?: 'top' | 'bottom' | 'left' | 'right';
  children: React.ReactNode;
}
export declare function Tooltip(props: TooltipProps): JSX.Element;
