import * as React from 'react';

/** Single-choice control. Always render two or more in a named group. */
export interface RadioProps extends Omit<React.InputHTMLAttributes<HTMLInputElement>, 'size' | 'type'> {
  label: React.ReactNode;
  description?: React.ReactNode;
}
export declare function Radio(props: RadioProps): JSX.Element;
