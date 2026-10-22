# UI kit — SD European parts shop

Recreation of **https://shop.sdeuropean.co.nz** (Shopify) using this design system's primitives.
Built from the public storefront's markup, IA and copy. No theme source or Figma file was supplied.

## Screens
| File | Screen | Source |
| --- | --- | --- |
| `StorefrontScreen.jsx` | Storefront — banner, category tiles, latest parts, "Who are SD European?" black band with symbol watermark, top brands, trust row | `/` |
| `CollectionScreen.jsx` | Collection — breadcrumb, sort, rego fitment check, facet sidebar, product grid, pagination | `/collections/shop-bmw` |
| `ProductScreen.jsx` | Product detail — gallery, price, fitment check, quantity, Description / Fitment / Delivery tabs, cross-sell | product template |
| `CartDrawer.jsx` | Slide-over cart with free-shipping progress and checkout CTA | `/cart` |
| `ShopChrome.jsx` | Announcement bar, black utility bar, sticky header with search + mega-menus, trust row, footer | site-wide |
| `ProductCard.jsx` | Product tile used across all three screens | site-wide |
| `products.js` | Representative product data (part numbers and fitment are illustrative) | — |

`index.html` wires them into a click-through: browse → collection → product → add to cart → cart drawer.

## Known gaps
- **Product and banner photography is placeholder.** Every image is a labelled grey block.
- **Supplier brand logos (Valeo, Topran, Mann-Filter, Hella, Arnott, Vemo/Vaico) are text placeholders** — third-party marks were not supplied and are not ours to reproduce.
- Checkout, account and search-results pages are Shopify-owned and are not recreated.
- Prices, SKUs and stock states are illustrative.
