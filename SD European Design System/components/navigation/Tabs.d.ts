import * as React from 'react';

/**
 * @startingPoint section="Navigation" subtitle="Underline and segmented tab bars" viewport="700x160"
 * Switches between sibling views. Active underline is the brand yellow, 3px.
 */
export interface TabItem { value: string; label: React.ReactNode; icon?: React.ReactNode; count?: number }
export interface TabsProps extends Omit<React.HTMLAttributes<HTMLDivElement>, 'onChange'> {
  items: TabItem[];
  value?: string;
  defaultValue?: string;
  onChange?: (value: string) => void;
  /** underline = page-level sections · segmented = compact in-panel switching. */
  variant?: 'underline' | 'segmented';
}
export declare function Tabs(props: TabsProps): JSX.Element;
