# SD European — Design System

**Your European vehicle experts.**

SD European is an independent European-vehicle parts supplier and mechanical workshop based in Hamilton, Waikato, New Zealand. Founded in 1989 by Steve Daly in a home garage restoring and rebuilding BMWs, it now runs two sites and 30+ staff, supplying new aftermarket, OEM and dismantled parts NZ-wide and servicing Audi, BMW, Mercedes-Benz, Mini, Skoda, Porsche and VW.

- **Parts Centre** — 177 Ellis Street, Frankton, Hamilton
- **Service Centre** — 23 Norris Avenue, Te Rapa, Hamilton
- **Freephone** 0800 269 772 · **Ph** +64 7 829 9649

**Mission** — world-class customer service and the highest quality in everything we do, through four value pillars: **Family, Driven, Innovation, Quality**.
**Vision** — to be the leading car parts supplier in New Zealand, expanding to more service centres, working as one team.

## Sources this system was built from

| Source | Notes |
| --- | --- |
| `uploads/Brand Guidelines - Claude 07.08.2026.pdf` | 6 pages: Brand Introduction, Typography, Imagery & Photography, Voice, Brand Messaging, Colour Palette. **Ground truth** for colour, type and voice. |
| `uploads/sd_european_logo_black.png` / `_white.png` | Horizontal lockup (symbol + wordmark), 3000×496, transparent PNG. |
| `uploads/sd_european_symbol_black_yellow V2.png` | Standalone circular "S" symbol, square, transparent PNG. |
| https://www.sdeuropean.co.nz | Marketing site — nav structure, service names, body copy, section patterns. Read publicly; no codebase or Figma file was provided. |
| https://shop.sdeuropean.co.nz | Shopify parts store — commerce IA, category names, trust-badge copy. Read publicly. |
| https://eurolink.sdeuropean.nz | Dealer portal linked from the site footer. **Not accessible** — no UI kit built for it. |

> **No codebase and no Figma file were supplied.** Component inventory, spacing, radii and elevation are therefore *authored* from the brand guidelines plus observation of the two public sites — they are a proposal, not a transcription. Numeric values are open to correction.

## Products represented

1. **Marketing website** (`ui_kits/website/`) — sdeuropean.co.nz: home, servicing, dismantling/parts listing, booking.
2. **Parts shop** (`ui_kits/parts-shop/`) — shop.sdeuropean.co.nz: storefront, category/collection grid, product detail, cart.

---

## CONTENT FUNDAMENTALS

**Voice: approachable + knowledgeable.** Expert enough to be trusted with a €-priced German engine, plain enough to talk to someone who just wants their car back by Friday.

| Attribute | How it sounds | Real example |
| --- | --- | --- |
| Professional | Knowledgeable and technical, but approachable | "Our technicians deliver precision with every service" |
| Passionate | Enthusiastic about European vehicles | "We live and breathe European cars" |
| Trustworthy | Clear, honest, reliable | "We ensure quality done right the first time" |
| Customer-focused | Caring and personable, treats customers like family | "Your car deserves the best and so do you" |

### Person and address
- **"We" for SD European, "you/your" for the customer.** Never third-person ("SD European offers…") in customer-facing copy. *"We're based in Hamilton, but you're probably in Auckland, Tauranga, Wellington…"*
- Copy anticipates the reader's objection and answers it in the same breath: *"That's no problem, because all our parts are in stock and ready to fire out to you overnight NZ wide."*
- Second person carries into micro-copy too: "Sell Your Vehicle", "Your email", "How we can help you".

### Register
- **NZ English spelling**: specialise, colour, tyre, authorised, licensed. Never Americanise.
- **Kiwi-conversational, never slangy-for-the-sake-of-it.** Real site phrases: *"soon as"*, *"a heap of knowledge"*, *"we've got you covered"*, *"Kiwi owned & operated"*, *"no nasty surprises at the eftpos machine"*. This warmth is a deliberate counterweight to the technical subject matter — keep about one such phrase per paragraph, not one per sentence.
- **Domain vocabulary is used plainly, not explained down**: rego, VIN, WOF, DSG, ECU tuning, dismantling, OEM, aftermarket, courtesy car.
- Sentences are short-to-medium. Fragments are fine as emphasis: *"Quality parts. Exceptional service. Every time."*

### Casing
- **Headlines**: sentence case or Title Case — both appear. Prefer **Title Case** for page/section headings ("Your Parts & Servicing Specialists", "Now Dismantling Car Parts For:").
- **Eyebrows, marque lists and small labels**: ALL CAPS with wide tracking and pipe separators — `AUDI | BMW | MERCEDES | MINI | SKODA | VW`. This is the single most recognisable typographic device in the brand.
- **Buttons**: Title Case, short verb phrases — "Shop Now", "Book a Service", "Learn More", "Add to Cart".
- Never all-caps a full sentence.

### Taglines (use verbatim, don't paraphrase)
- Primary: **"Your European vehicle experts"**
- "Quality parts. Exceptional service. Every time."
- "Driven by passion. Defined by quality."
- "Where quality parts meet expert service."
- "The trusted name for European vehicle care."
- Store framing: "NZ's Home of European Cars" / "Welcome to NZ's home of European Car Parts"

### Emoji
**Not part of the brand system.** The Shopify announcement bar uses a lone 📦 on the free-shipping strip; treat that as a platform artefact, not a licence. Author new work without emoji.

### Numbers, proof and legal
- Proof points are concrete and repeated: *30+ years*, *over 350 vehicles dismantling*, *12 months warranty*, *order before 2pm for overnight delivery*, *free shipping over $175*.
- Prices are NZD, `$1,249.00` format, GST convention follows the store.
- Part numbers, VINs and regos set in `--font-mono`.
- Independence disclaimer must appear in footers: *"SD European is an independent parts and service specialist and has no association with BMW New Zealand or BMW AG."*
- Marque names are references to fitment, never endorsements. Never render another manufacturer's logo.

---

## VISUAL FOUNDATIONS

The look is **workshop-clean**: high-contrast black, one hot yellow, generous white, and real photography. It should read like a well-lit service bay — nothing decorative that isn't doing a job.

### Colour
Four brand colours, straight from the guidelines: black `#010101`, yellow `#FFC326`, white `#FFFFFF`, light grey `#D8D8D8`. Everything else in `tokens/colors.css` is a derived ramp for states only.

- **Yellow is an accent, never a background field for long text.** It carries primary buttons, the 4px section rule, active states, the symbol, and one hero band per page at most. Rough budget: **≤10% of any screen**.
- **Black is the second brand colour, not a text colour by default** — full-bleed black bands separate sections and host inverse content (footer, stats strip, quote block).
- Yellow on black and black on yellow are both approved. **Never white text on yellow** (1.6:1) and never yellow text on white for body copy — yellow text is only allowed at ≥21px bold over black.
- Greys do the entire remaining job: `--neutral-100` for subtle surfaces, `--neutral-200` (= brand grey) for hairline borders, `--neutral-600` for secondary text.
- Status colours (green/amber/red/blue) exist for stock, warranty and fault states in the shop and workshop UI. They are muted and desaturated so they never compete with the brand yellow.

### Typography
- **Headings — League Spartan.** Geometric, tight, confident. Weight 700–800, `letter-spacing: -0.02em`, line-height 1.05–1.18. Never lighter than 600 in a heading.
- **Body — Frutiger** (licensed) — currently substituted with **Source Sans 3**, see *Font substitution* below. Regular 400, line-height 1.65, max measure ~68 characters.
- **Eyebrow/marque device** — League Spartan 700, 12px, `letter-spacing: 0.22em`, uppercase, grey or yellow.
- **Mono** — system mono for part numbers, VINs and regos only.
- Scale is a 1.200 minor third from 16px. Display 72px is reserved for hero units.

### Layout
- 1200px content container, 1440px wide container for full-bleed media rows; 24px gutters (40px ≥1200px). 12-column grid.
- Vertical rhythm: 80px section padding, 112px for hero and closing sections. 4px spacing base.
- Sticky header (76px) with a 40px black utility bar above it carrying phone number and locations — the utility bar is the brand's signature fixed element. It scrolls away; the header does not.
- Sections alternate white → light grey → black. **Two background colours per page maximum**, plus black.
- Every section heading is preceded by an eyebrow and followed by a 56×4px yellow rule (`.sd-rule`).

### Backgrounds and imagery
- **Photography-led, not illustration-led.** No hand-drawn illustration, no gradient meshes, no abstract shapes. If an illustration is needed, use a photograph instead.
- Photo direction (from the guidelines): authentic and real, sharp and clean, close-ups of parts and technicians in action, natural or balanced lighting, clean uncluttered backgrounds, generous negative space, custom over stock, diverse team.
- Colour vibe of imagery: **neutral-to-cool, high clarity, no grain, no filters.** Workshop metal, black paint, clean concrete. Yellow appears in-frame via the brand, not via grading. Occasional full-bleed hero photos are desaturated to ~30% behind large type.
- Repeating patterns: none. The only motif is the **circular symbol used as an oversized watermark** — a single large, low-opacity (6–10%) mark bleeding off one edge of a section, black on white or white on black. Never tiled.
- Full-bleed hero images are always paired with a **protection scrim gradient**, not a capsule: `--scrim-bottom` for bottom-anchored copy, `--scrim-left` for left-anchored. Solid translucent capsules are only used for small floating labels over busy images (e.g. a price chip on a product photo).

### Corners, borders, shadows
- **Corners are nearly square.** 4px on inputs, tags and buttons; 6px on cards and dialogs; 10px on large media cards. Pill radius is reserved for badges and filter chips. Fully round only for the symbol, avatars and icon buttons. Nothing else is rounded — the brand's precision reads through sharp geometry.
- **Borders before shadows.** Default card is white, 1px `--neutral-200` hairline, 6px radius, no shadow. Shadow appears on hover and on floating layers only.
- Shadow ramp is soft and neutral (never coloured, never yellow): `xs` 1px, `sm` cards on hover, `md` dropdowns/popovers, `lg` sticky bars, `overlay` for dialogs. No inner shadows except a subtle top highlight on black surfaces (`--shadow-inset-top`).
- Focus is a 2px solid yellow outline at 2px offset — plus `--ring-focus` (3px yellow glow) on dark surfaces.

### Interaction states
- **Hover — darken, don't lighten.** Yellow primary → `--yellow-500`. Black secondary → `--neutral-700`. Ghost/text → yellow underline grows in. Cards lift 2px and gain `--shadow-sm`. Never opacity-fade on hover; opacity is reserved for disabled.
- **Press — colour shift + 1.5% shrink.** `transform: scale(.985)`, yellow → `--yellow-600`. No ripples.
- **Disabled** — `opacity: .45`, `cursor: not-allowed`, no state changes.
- **Loading** — greyscale skeleton blocks in `--neutral-100`, 1.4s shimmer. No spinners over 24px.

### Motion
Motion is **short, flat and functional** — this is an engineering brand, nothing bounces.
- 140ms for control state changes, 200ms for surfaces, 320ms for entrances, 520ms only for hero/carousel crossfades.
- Easing is `cubic-bezier(.2,0,.2,1)` throughout; `ease-out` for entering, `ease-in` for exiting. **No spring, no overshoot, no bounce, no elastic.**
- Entrances are fade + 8px rise. Exits are fade only. Carousels crossfade; they do not slide.
- `prefers-reduced-motion` collapses all durations to 0 (already wired in `tokens/motion.css`).

### Transparency and blur
Used sparingly and only for two jobs: (1) scrim gradients over photography, (2) the sticky header gains `rgba(255,255,255,.92)` + `backdrop-filter: blur(10px)` once the page scrolls. Modal overlay is `rgba(1,1,1,.72)`, unblurred. Nothing else is translucent — no frosted cards, no glassmorphism.

### Font substitution — ACTION NEEDED
**Frutiger is a licensed Linotype face and no font files were supplied.** `--font-body` currently resolves to **Source Sans 3** (Google Fonts) — the closest open humanist sans: tall x-height, open apertures, squarish bowls, same neutral-warm tone. It is *not* metrically identical; Frutiger is slightly narrower with more distinctive `a`/`e` terminals.
`--font-body` already lists `"Frutiger","Frutiger LT Std"` ahead of the fallback, so a licensed local/webfont install takes over automatically. **Please send Frutiger WOFF2 files (or confirm the substitution) and we'll self-host them.**
League Spartan (headings) is genuine — it is on Google Fonts and is loaded directly.

---

## ICONOGRAPHY

**No proprietary icon set exists.** Neither supplied source contains an icon font, SVG sprite or icon library — the marketing site uses a theme's built-in glyphs and the store uses Shopify theme icons. Nothing was available to copy.

- **Substitution (flagged):** this system uses **Lucide** from CDN (`https://unpkg.com/lucide@latest`) — 24px grid, 2px stroke, round caps, no fill. It was chosen because the brand's geometry is precise, thin-stroked and unornamented; a filled or duotone set (Material, Phosphor Fill) would fight the flat black-and-yellow palette. **Please confirm or supply the real set.**
- Rules: **stroke-only, 1.5–2px, `currentColor`, never two-tone, never yellow-filled.** Icons inherit text colour; the only coloured icon is a yellow icon on black.
- Sizes: 16px inline with body text, 20px in buttons and list items, 24px standalone/nav, 32px in feature blocks. Never above 40px — big pictograms are replaced by photography.
- Icons never appear inside coloured circles or squares. A feature row is `icon + heading + line of copy`, left-aligned, icon in `--brand-black` at 24px.
- **Unicode as icons:** only the pipe `|` in marque lists and `·` as a metadata separator. No arrows, ticks or bullets drawn from Unicode — use Lucide `arrow-right`, `check`, etc.
- **Emoji are never used as icons.**
- **Marque logos (BMW, Audi, VW…) are never rendered.** Marques appear as set type only, per the independence disclaimer.
- **Brand assets** (in `assets/`): `logo-horizontal-black.png`, `logo-horizontal-white.png`, `symbol-black-yellow.png`. The symbol is the interlocking "S" in a quartered circle. Minimum clear space = the radius of the inner disc. Minimum lockup width 140px; below that use the symbol alone. Never recolour, outline, stretch, rotate or add effects to either mark. On photography, place the white lockup over the darkest quadrant, never over detail.
  - *Missing:* vector (SVG/EPS) versions of both marks, and a stacked/vertical lockup. Please supply.

---

## INDEX

**Root**
- `styles.css` — the single entry point consumers link. `@import` list only.
- `readme.md` — this file.
- `SKILL.md` — Agent-Skills wrapper so this folder works inside Claude Code.
- `thumbnail.html` — homepage tile.

**`tokens/`** — `fonts.css`, `colors.css`, `typography.css`, `spacing.css`, `radius.css`, `elevation.css`, `motion.css`, `base.css`

**`assets/`** — `logo-horizontal-black.png`, `logo-horizontal-white.png`, `symbol-black-yellow.png`

**`guidelines/`** — foundation specimen cards (Colors, Type, Spacing, Brand, Motion groups in the Design System tab)

**`components/`**
- `brand/` — `Logo`, `Eyebrow`, `SectionHeading`
- `core/` — `Button`, `IconButton`, `Card`, `Badge`, `Tag`, `Icon`
- `forms/` — `Input`, `Select`, `Checkbox`, `Radio`, `Switch`
- `navigation/` — `Tabs`
- `feedback/` — `Dialog`, `Toast`, `Tooltip`

**`ui_kits/`**
- `website/` — marketing site recreation (`index.html` + screen JSX)
- `parts-shop/` — Shopify storefront recreation (`index.html` + screen JSX)

### Intentional additions
No source defined a component inventory, so the standard set was authored. Three brand-specific additions beyond it:
- **`Logo`** — enforces the lockup/symbol rules and clear space so consumers can't mis-scale the mark.
- **`Eyebrow` / `SectionHeading`** — the ALL-CAPS tracked label + yellow rule is the brand's signature section device; codifying it stops it being re-hand-rolled inconsistently.
- **`Icon`** — thin wrapper over Lucide so the stroke width, size scale and `currentColor` rule are enforced in one place.
