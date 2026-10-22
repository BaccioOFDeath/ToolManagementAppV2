const { Button, Badge, Tabs, Eyebrow, Icon, Input, Tooltip } = window.SDEuropeanDesignSystem_614674 || {};

function ProductScreen({ go, add }) {
  const p = window.SHOP_PRODUCTS[0];
  const [tab, setTab] = React.useState('desc');
  const [qty, setQty] = React.useState(1);
  const [shot, setShot] = React.useState(0);
  return (
    <main style={{ maxWidth: 'var(--container-wide)', margin: '0 auto', padding: 'var(--space-8) var(--gutter-lg) var(--space-16)' }}>
      <nav style={{ font: 'var(--type-body-sm)', color: 'var(--text-muted)', marginBottom: 'var(--space-6)' }}>
        <a href="#" onClick={(e) => { e.preventDefault(); go('home'); }} style={{ borderBottom: 0, color: 'var(--text-muted)' }}>Home</a> / <a href="#" onClick={(e) => { e.preventDefault(); go('collection'); }} style={{ borderBottom: 0, color: 'var(--text-muted)' }}>Shop BMW</a> / <strong style={{ color: 'var(--text-primary)' }}>{p.name}</strong>
      </nav>

      <div style={{ display: 'grid', gridTemplateColumns: '1.1fr 1fr', gap: 'var(--space-12)', alignItems: 'start' }}>
        <div style={{ display: 'grid', gap: 'var(--space-3)' }}>
          <div style={{ border: '1px solid var(--border-default)', borderRadius: 'var(--radius-lg)', overflow: 'hidden' }}><ShopPhoto label={'Product photo ' + (shot + 1)} ratio="4 / 3" /></div>
          <div style={{ display: 'grid', gridTemplateColumns: 'repeat(4,1fr)', gap: 'var(--space-3)' }}>
            {[0, 1, 2, 3].map((i) => (
              <button key={i} onClick={() => setShot(i)} style={{ padding: 0, border: '1px solid ' + (shot === i ? 'var(--brand-black)' : 'var(--border-default)'), borderRadius: 'var(--radius-md)', overflow: 'hidden', cursor: 'pointer', background: '#fff' }}>
                <ShopPhoto label={'0' + (i + 1)} />
              </button>
            ))}
          </div>
        </div>

        <div style={{ display: 'grid', gap: 'var(--space-4)' }}>
          <Eyebrow>{p.brand}</Eyebrow>
          <h1 style={{ font: 'var(--type-h1)', fontSize: 'var(--fs-3xl)' }}>{p.name}</h1>
          <div style={{ display: 'flex', gap: 'var(--space-2)', flexWrap: 'wrap' }}>
            <Badge tone="success" dot>In Stock</Badge><Badge tone="accent">12 Month Warranty</Badge><Badge tone="neutral">{p.condition}</Badge>
          </div>
          <div style={{ display: 'flex', alignItems: 'baseline', gap: 'var(--space-3)' }}>
            <strong style={{ font: 'var(--type-h1)', fontSize: '38px' }}>${p.price.toFixed(2)}</strong>
            {p.was ? <s style={{ font: 'var(--type-lead)', color: 'var(--text-muted)' }}>${p.was.toFixed(2)}</s> : null}
            <span style={{ font: 'var(--type-body-sm)', color: 'var(--text-muted)' }}>NZD, incl. GST</span>
          </div>
          <div style={{ display: 'grid', gap: '6px', font: 'var(--type-body-sm)', color: 'var(--text-secondary)' }}>
            <span>Fits <strong style={{ color: 'var(--text-primary)' }}>{p.fits}</strong></span>
            <span>Part number <span style={{ font: 'var(--type-mono)', color: 'var(--text-primary)' }}>{p.sku}</span></span>
          </div>

          <div style={{ background: 'var(--surface-subtle)', borderRadius: 'var(--radius-md)', padding: 'var(--space-4)', display: 'grid', gap: 'var(--space-3)' }}>
            <Eyebrow>Guaranteed fit</Eyebrow>
            <div style={{ display: 'flex', gap: 'var(--space-2)' }}>
              <div style={{ flex: 1 }}><Input mono placeholder="Enter your rego" aria-label="Rego" /></div>
              <Button variant="secondary">Check Fitment</Button>
            </div>
            <p style={{ font: 'var(--type-body-sm)', color: 'var(--text-muted)' }}>We manually check each order with your rego number to be 100% sure it will fit.</p>
          </div>

          <div style={{ display: 'flex', gap: 'var(--space-3)', alignItems: 'stretch' }}>
            <div style={{ display: 'flex', alignItems: 'center', border: '1px solid var(--border-default)', borderRadius: 'var(--radius-sm)' }}>
              <button onClick={() => setQty(Math.max(1, qty - 1))} style={{ width: '42px', height: '44px', border: 0, background: 'none', cursor: 'pointer', font: 'var(--type-h4)' }}>−</button>
              <span style={{ width: '40px', textAlign: 'center', font: 'var(--type-mono)', fontSize: 'var(--fs-base)' }}>{qty}</span>
              <button onClick={() => setQty(qty + 1)} style={{ width: '42px', height: '44px', border: 0, background: 'none', cursor: 'pointer', font: 'var(--type-h4)' }}>+</button>
            </div>
            <Button size="md" style={{ flex: 1 }} onClick={() => add(p, qty)} iconLeft={<Icon name="shopping-cart" size={20} />}>Add to Cart</Button>
          </div>
          <div style={{ display: 'flex', gap: 'var(--space-5)', font: 'var(--type-body-sm)', color: 'var(--text-secondary)' }}>
            <span style={{ display: 'inline-flex', gap: '8px', alignItems: 'center' }}><Icon name="truck" size={16} />Order before 2pm for overnight delivery</span>
            <span style={{ display: 'inline-flex', gap: '8px', alignItems: 'center' }}><Icon name="phone" size={16} />0800 269 772</span>
          </div>
        </div>
      </div>

      <div style={{ marginTop: 'var(--space-16)' }}>
        <Tabs items={[{ value: 'desc', label: 'Description' }, { value: 'fit', label: 'Fitment', count: 14 }, { value: 'del', label: 'Delivery & Warranty' }]} value={tab} onChange={setTab} />
        <div style={{ marginTop: 'var(--space-6)', maxWidth: '760px', font: 'var(--type-body)', color: 'var(--text-secondary)', display: 'grid', gap: 'var(--space-3)' }}>
          {tab === 'desc' ? (
            <React.Fragment>
              <p>Complete front right window regulator with motor, supplied by Teilex. Direct replacement for the original unit — the common failure on this platform is the plastic slider and cable guide, so we supply the whole assembly rather than a repair kit.</p>
              <p>Not sure this is the one? Send through your rego and our parts team will match your VIN to the exact part you need.</p>
            </React.Fragment>
          ) : tab === 'fit' ? (
            <div style={{ display: 'grid', gridTemplateColumns: 'repeat(2,1fr)', gap: '6px var(--space-8)' }}>
              {['E90 320i 2005–2011', 'E90 325i 2005–2011', 'E90 330i 2005–2011', 'E91 320i Touring 2006–2012', 'E91 325i Touring 2006–2012', 'E90 320d 2005–2011', 'E90 335i 2007–2011', 'E91 330d Touring 2007–2012'].map((x) => (
                <span key={x} style={{ display: 'flex', gap: '8px', alignItems: 'center', font: 'var(--type-body-sm)' }}><span style={{ color: 'var(--status-success)' }}><Icon name="check" size={16} /></span>{x}</span>
              ))}
            </div>
          ) : (
            <React.Fragment>
              <p>Order before 2pm for overnight delivery in most cases. Large items expect 2–3 days. Free shipping on orders over $175.</p>
              <p>All new parts carry a 12 month part-only warranty so you can buy with confidence. <Tooltip label="Warrant of Fitness"><span style={{ borderBottom: '1px dotted var(--neutral-400)' }}>WOF</span></Tooltip> failures fitted at our Te Rapa workshop are covered on labour too.</p>
            </React.Fragment>
          )}
        </div>
      </div>

      <div style={{ marginTop: 'var(--space-16)' }}>
        <Eyebrow>Customers also bought</Eyebrow>
        <div style={{ display: 'grid', gridTemplateColumns: 'repeat(4,1fr)', gap: 'var(--space-6)', marginTop: 'var(--space-5)' }}>
          {window.SHOP_PRODUCTS.slice(1, 5).map((x) => <ProductCard key={x.id} p={x} onOpen={() => go('product')} onAdd={add} />)}
        </div>
      </div>
    </main>
  );
}

Object.assign(window, { ProductScreen });
