import * as React from 'react';

/** Single-line text field with uppercase label, hint and error slot. */
export interface InputProps extends React.InputHTMLAttributes<HTMLInputElement> {
  label?: string;
  hint?: string;
  /** Replaces the hint and turns the border red. */
  error?: string;
  prefix?: React.ReactNode;
  suffix?: React.ReactNode;
  size?: 'sm' | 'md' | 'lg';
  /** Sets the mono face + uppercase — use for rego, VIN and part numbers. */
  mono?: boolean;
}
export declare function Input(props: InputProps): JSX.Element;
