import * as React from 'react';

/** Immediate on/off toggle (applies instantly — use Checkbox inside forms that submit). */
export interface SwitchProps extends Omit<React.InputHTMLAttributes<HTMLInputElement>, 'size' | 'type'> {
  label?: React.ReactNode;
}
export declare function Switch(props: SwitchProps): JSX.Element;
