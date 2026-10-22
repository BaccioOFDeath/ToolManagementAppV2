One-line: the standard text field — 4px radius, hairline border, black focus border plus yellow glow.

```jsx
<Input label="Rego" mono placeholder="ABC123" hint="We'll match your VIN to the exact part." />
<Input label="Email" type="email" error="Enter a valid email address." />
```

Use `mono` for rego / VIN / part numbers. `prefix`/`suffix` take an `<Icon>` or a unit string.
