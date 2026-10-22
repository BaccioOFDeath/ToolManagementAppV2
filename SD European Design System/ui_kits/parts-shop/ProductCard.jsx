const { Badge, Button, Icon } = window.SDEuropeanDesignSystem_614674 || {};

const STOCK = { in: ['success', 'In Stock'], low: ['warning', 'Low Stock'], order: ['info', 'Special Order'] };

function ProductCard({ p, onOpen, onAdd }) {
  const [h, setH] = React.useState(false);
  const [tone, label] = STOCK[p.stock];
  return (
    <div onMouseEnter={() => setH(true)} onMouseLeave={() => setH(false)} onClick={() => onOpen && onOpen(p)}
      style={{ background: '#fff', border: '1px solid var(--border-default)', borderRadius: 'var(--radius-md)', overflow: 'hidden', cursor: 'pointer', display: 'flex', flexDirection: 'column', transform: h ? 'var(--hover-lift)' : 'none', boxShadow: h ? 'var(--shadow-sm)' : 'none', transition: 'var(--transition-surface)' }}>
      <div style={{ position: 'relative' }}>
        <ShopPhoto label="Part photo" />
        {p.was ? <span style={{ position: 'absolute', top: '10px', left: '10px' }}><Badge tone="accent">Save ${(p.was - p.price).toFixed(0)}</Badge></span> : null}
        {p.condition === 'Used' ? <span style={{ position: 'absolute', top: '10px', left: '10px' }}><Badge tone="dark">Used</Badge></span> : null}
      </div>
      <div style={{ padding: 'var(--space-4)', display: 'grid', gap: '6px', flex: 1 }}>
        <span style={{ font: 'var(--type-label)', letterSpacing: 'var(--ls-wide)', textTransform: 'uppercase', color: 'var(--text-muted)' }}>{p.brand}</span>
        <h3 style={{ font: 'var(--type-h4)', fontSize: 'var(--fs-base)', lineHeight: 1.3 }}>{p.name}</h3>
        <span style={{ font: 'var(--type-body-sm)', color: 'var(--text-secondary)' }}>{p.fits}</span>
        <span style={{ font: 'var(--type-mono)', fontSize: 'var(--fs-xs)', color: 'var(--text-muted)' }}>{p.sku}</span>
        <div style={{ marginTop: 'auto', paddingTop: 'var(--space-3)', display: 'flex', alignItems: 'baseline', gap: 'var(--space-2)' }}>
          <strong style={{ font: 'var(--type-h3)', fontSize: 'var(--fs-lg)' }}>${p.price.toFixed(2)}</strong>
          {p.was ? <s style={{ font: 'var(--type-body-sm)', color: 'var(--text-muted)' }}>${p.was.toFixed(2)}</s> : null}
        </div>
        <div style={{ display: 'flex', alignItems: 'center', gap: 'var(--space-2)', marginTop: '2px' }}><Badge tone={tone} size="sm" dot>{label}</Badge></div>
        <div style={{ marginTop: 'var(--space-3)' }}>
          <Button size="sm" fullWidth onClick={(e) => { e.stopPropagation(); onAdd && onAdd(p); }} iconLeft={<Icon name="shopping-cart" size={16} />}>Add to Cart</Button>
        </div>
      </div>
    </div>
  );
}

Object.assign(window, { ProductCard });
