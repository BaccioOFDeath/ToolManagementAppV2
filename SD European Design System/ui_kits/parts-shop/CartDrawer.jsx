const { Button, Badge, Icon, IconButton, Eyebrow } = window.SDEuropeanDesignSystem_614674 || {};

function CartDrawer({ open, items, onClose, onRemove }) {
  const subtotal = items.reduce((s, i) => s + i.price * i.qty, 0);
  const freeShip = subtotal >= 175;
  if (!open) return null;
  return (
    <div onClick={onClose} style={{ position: 'fixed', inset: 0, background: 'var(--surface-overlay)', zIndex: 200, display: 'flex', justifyContent: 'flex-end' }}>
      <aside onClick={(e) => e.stopPropagation()} style={{ width: '420px', maxWidth: '100%', background: '#fff', height: '100%', display: 'flex', flexDirection: 'column', boxShadow: 'var(--shadow-overlay)' }}>
        <div style={{ padding: 'var(--space-5) var(--space-6)', borderBottom: '1px solid var(--border-default)', display: 'flex', alignItems: 'center', gap: 'var(--space-3)' }}>
          <h2 style={{ font: 'var(--type-h3)', fontSize: 'var(--fs-lg)', flex: 1 }}>Your Cart ({items.length})</h2>
          <IconButton icon={<Icon name="x" size={20} />} label="Close cart" onClick={onClose} />
        </div>
        <div style={{ padding: 'var(--space-4) var(--space-6)', background: freeShip ? 'var(--status-success-surface)' : 'var(--surface-accent-subtle)', font: 'var(--type-body-sm)', color: freeShip ? 'var(--status-success)' : 'var(--text-primary)', display: 'flex', gap: '8px', alignItems: 'center' }}>
          <Icon name={freeShip ? 'check' : 'truck'} size={16} />
          {freeShip ? "You've got free shipping." : `Add $${(175 - subtotal).toFixed(2)} more for free shipping.`}
        </div>
        <div style={{ flex: 1, overflow: 'auto', padding: 'var(--space-6)', display: 'grid', gap: 'var(--space-5)', alignContent: 'start' }}>
          {items.length === 0 ? <p style={{ font: 'var(--type-body)', color: 'var(--text-muted)' }}>Nothing in here yet. Browse the store or send us your rego and we'll find the part.</p> : null}
          {items.map((i) => (
            <div key={i.id} style={{ display: 'grid', gridTemplateColumns: '72px 1fr auto', gap: 'var(--space-4)', alignItems: 'start' }}>
              <div style={{ border: '1px solid var(--border-default)', borderRadius: 'var(--radius-sm)', overflow: 'hidden' }}><ShopPhoto label="" /></div>
              <div style={{ display: 'grid', gap: '4px' }}>
                <strong style={{ font: 'var(--type-h4)', fontSize: 'var(--fs-sm)' }}>{i.name}</strong>
                <span style={{ font: 'var(--type-body-sm)', color: 'var(--text-muted)' }}>{i.fits}</span>
                <span style={{ font: 'var(--type-mono)', fontSize: 'var(--fs-xs)', color: 'var(--text-muted)' }}>{i.sku} · Qty {i.qty}</span>
                <button onClick={() => onRemove(i.id)} style={{ justifySelf: 'start', border: 0, background: 'none', padding: 0, cursor: 'pointer', font: 'var(--type-body-sm)', color: 'var(--text-muted)', textDecoration: 'underline' }}>Remove</button>
              </div>
              <strong style={{ font: 'var(--type-body)' }}>${(i.price * i.qty).toFixed(2)}</strong>
            </div>
          ))}
        </div>
        <div style={{ borderTop: '1px solid var(--border-default)', padding: 'var(--space-6)', display: 'grid', gap: 'var(--space-4)' }}>
          <div style={{ display: 'flex', justifyContent: 'space-between', font: 'var(--type-h3)', fontSize: 'var(--fs-lg)' }}><span>Subtotal</span><span>${subtotal.toFixed(2)}</span></div>
          <span style={{ font: 'var(--type-body-sm)', color: 'var(--text-muted)' }}>Shipping calculated at checkout. NZD, incl. GST.</span>
          <Button size="lg" fullWidth disabled={items.length === 0}>Checkout</Button>
          <Button variant="ghost" size="sm" fullWidth onClick={onClose}>Continue Shopping</Button>
        </div>
      </aside>
    </div>
  );
}

Object.assign(window, { CartDrawer });
