One-line: short-lived confirmation or error, stacked bottom-right, auto-dismissing after ~5s.

```jsx
<Toast tone="success" title="Added to cart" message="Water Pump — N20 · $189.00" onDismiss={close} />
```

Enters with fade + 8px rise, exits with fade only. Never use for anything the user must act on — that's a Dialog.
