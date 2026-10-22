# UI kit — SD European marketing website

Recreation of **https://www.sdeuropean.co.nz** using this design system's primitives.
Built from the public site's markup and copy (no codebase or Figma file was supplied), plus the brand guidelines for colour, type and voice.

## Screens
| File | Screen | Source page |
| --- | --- | --- |
| `HomeScreen.jsx` | Home — hero carousel, three routes, dismantling marques, parts/workshop split, "How we can help you" black band, insurer row | `/` |
| `ServicingScreen.jsx` | Servicing — hero, six service cards, Why us / Reviews / Visiting tabs | `/sdservices`, `/european-car-service`, `/location` |
| `DismantlingScreen.jsx` | Cars for dismantling — filter bar, marque chips, vehicle grid, rego CTA band | `/cars-for-dismantling` |
| `BookingScreen.jsx` | Online service booking — three-step form with live summary | `/online-service-booking` |
| `Chrome.jsx` | Utility bar, sticky header + dropdowns, black footer, Section and Photo helpers | site-wide |

`index.html` wires them into a click-through: nav items and CTAs move between screens.

## Known gaps
- **Photography is placeholder.** Every image is a labelled grey/dark block. The real site is photography-led; drop real workshop and vehicle shots in and the layouts are finished.
- **Insurer/partner logos are text placeholders** — real logo files were not supplied.
- Vehicle listings, part numbers and reviews are representative; reviews are quoted from the public servicing page.
- The EuroLink dealer portal (`eurolink.sdeuropean.nz`) is not reachable, so no kit exists for it.
