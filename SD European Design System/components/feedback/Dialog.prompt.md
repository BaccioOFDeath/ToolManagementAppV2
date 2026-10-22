One-line: centred modal for a confirmation or a short form; closes on overlay click and Escape.

```jsx
<Dialog open={open} title="Book a Service" onClose={close}
  footer={<><Button variant="ghost" onClick={close}>Cancel</Button><Button>Confirm Booking</Button></>}>
  <Input label="Rego" mono />
</Dialog>
```
