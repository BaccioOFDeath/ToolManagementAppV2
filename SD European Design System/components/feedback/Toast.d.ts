import * as React from 'react';

/** Transient confirmation, bottom-right. Always black with a coloured left bar — never a coloured fill. */
export interface ToastProps extends React.HTMLAttributes<HTMLDivElement> {
  title: React.ReactNode;
  message?: React.ReactNode;
  tone?: 'neutral' | 'success' | 'warning' | 'danger';
  action?: React.ReactNode;
  icon?: React.ReactNode;
  onDismiss?: () => void;
}
export declare function Toast(props: ToastProps): JSX.Element;
