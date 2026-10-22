One-line: renders the SD European horizontal lockup or circular symbol at a safe size with correct clear space — use it anywhere the brand mark appears instead of an <img>.

```jsx
<Logo variant="lockup-black" height={40} assetBase="../../assets" href="/" />
```

Variants: `lockup-black` (default, on white/light), `lockup-white` (on black or the dark quadrant of a photo), `symbol` (square mark for favicons, avatars, watermarks and any lockup narrower than 140px).
`height` drives everything; horizontal clear space is derived at 18% of height. Do not override colour.
