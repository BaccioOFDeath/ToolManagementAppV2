import * as React from 'react';

/** Multi-select control. Checked state is black box with a yellow tick. */
export interface CheckboxProps extends Omit<React.InputHTMLAttributes<HTMLInputElement>, 'size'> {
  label: React.ReactNode;
  description?: React.ReactNode;
}
export declare function Checkbox(props: CheckboxProps): JSX.Element;
