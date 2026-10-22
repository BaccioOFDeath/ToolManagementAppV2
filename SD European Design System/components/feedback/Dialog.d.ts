import * as React from 'react';

/** Modal for confirmations and short forms (booking, quote request, back-in-stock). Overlay is rgba(1,1,1,.72), unblurred. */
export interface DialogProps extends React.HTMLAttributes<HTMLDivElement> {
  open: boolean;
  title: React.ReactNode;
  description?: React.ReactNode;
  /** Action row, right-aligned on a grey band. */
  footer?: React.ReactNode;
  onClose?: () => void;
  width?: number;
}
export declare function Dialog(props: DialogProps): JSX.Element | null;
