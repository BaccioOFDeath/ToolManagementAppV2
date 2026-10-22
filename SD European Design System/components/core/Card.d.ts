import * as React from 'react';

/**
 * @startingPoint section="Core" subtitle="Content, media, inverse and accent card surfaces" viewport="700x300"
 * The default surface: white, 1px hairline, 6px radius, no shadow at rest.
 */
export interface CardProps extends React.HTMLAttributes<HTMLDivElement> {
  /** Image URL. Adds a 4:3 media band and lifts the radius to 10px. */
  media?: string;
  mediaAlt?: string;
  title?: React.ReactNode;
  /** Small uppercase kicker above the title — category, marque, part number. */
  meta?: React.ReactNode;
  footer?: React.ReactNode;
  variant?: 'default' | 'subtle' | 'inverse' | 'accent';
  /** Adds the 2px hover lift + shadow. Set on cards that are links. */
  interactive?: boolean;
  padding?: string;
  children?: React.ReactNode;
}
export declare function Card(props: CardProps): JSX.Element;
