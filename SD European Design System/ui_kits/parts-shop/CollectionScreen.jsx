const { Button, Select, Checkbox, Tag, Eyebrow, Icon, Badge, Input } = window.SDEuropeanDesignSystem_614674 || {};

function CollectionScreen({ go, add }) {
  const [chips, setChips] = React.useState(['BMW']);
  const toggle = (c) => setChips((s) => s.includes(c) ? s.filter((x) => x !== c) : [...s, c]);
  const products = window.SHOP_PRODUCTS;
  return (
    <main style={{ maxWidth: 'var(--container-wide)', margin: '0 auto', padding: 'var(--space-8) var(--gutter-lg) var(--space-16)' }}>
      <nav style={{ font: 'var(--type-body-sm)', color: 'var(--text-muted)', marginBottom: 'var(--space-5)' }}>
        <a href="#" onClick={(e) => { e.preventDefault(); go('home'); }} style={{ borderBottom: 0, color: 'var(--text-muted)' }}>Home</a> / Shop BMW / <strong style={{ color: 'var(--text-primary)' }}>All Parts</strong>
      </nav>
      <div style={{ display: 'flex', alignItems: 'end', justifyContent: 'space-between', gap: 'var(--space-6)', paddingBottom: 'var(--space-5)', borderBottom: '1px solid var(--border-default)' }}>
        <div style={{ display: 'grid', gap: 'var(--space-2)' }}>
          <Eyebrow>Collection</Eyebrow>
          <h1 style={{ font: 'var(--type-h1)', fontSize: 'var(--fs-3xl)' }}>Shop BMW Parts</h1>
          <span style={{ font: 'var(--type-body-sm)', color: 'var(--text-secondary)' }}>{products.length} of 4,182 parts · all in stock in New Zealand</span>
        </div>
        <div style={{ width: '240px' }}><Select label="Sort by" size="sm" options={['Best selling', 'Price, low to high', 'Price, high to low', 'Newest']} /></div>
      </div>

      <div style={{ display: 'grid', gridTemplateColumns: '260px 1fr', gap: 'var(--space-10)', marginTop: 'var(--space-8)', alignItems: 'start' }}>
        <aside style={{ display: 'grid', gap: 'var(--space-6)', position: 'sticky', top: '150px' }}>
          <div style={{ display: 'grid', gap: 'var(--space-3)' }}>
            <Eyebrow>Check fitment</Eyebrow>
            <Input mono placeholder="ABC123" prefix={<Icon name="car" size={20} />} aria-label="Rego" />
            <Button size="sm" variant="secondary" fullWidth>Check My Rego</Button>
            <p style={{ font: 'var(--type-body-sm)', color: 'var(--text-muted)' }}>We manually check each order with your rego to be 100% sure it will fit.</p>
          </div>
          {[['Make', ['Audi', 'BMW', 'Mercedes-Benz', 'Mini', 'Skoda', 'Volkswagen']], ['Condition', ['New aftermarket', 'OEM', 'Used']], ['Brand', ['Valeo', 'Topran', 'Mann-Filter', 'Hella', 'Arnott', 'Vemo & Vaico']]].map(([h, items]) => (
            <div key={h} style={{ display: 'grid', gap: 'var(--space-3)', paddingTop: 'var(--space-5)', borderTop: '1px solid var(--border-default)' }}>
              <Eyebrow>{h}</Eyebrow>
              {items.map((i) => <Checkbox key={i} label={i} checked={chips.includes(i)} onChange={() => toggle(i)} />)}
            </div>
          ))}
          <div style={{ display: 'grid', gap: 'var(--space-3)', paddingTop: 'var(--space-5)', borderTop: '1px solid var(--border-default)' }}>
            <Eyebrow>Availability</Eyebrow>
            <Checkbox label="In stock only" defaultChecked />
          </div>
        </aside>

        <div>
          {chips.length ? (
            <div style={{ display: 'flex', gap: 'var(--space-2)', flexWrap: 'wrap', marginBottom: 'var(--space-6)' }}>
              {chips.map((c) => <Tag key={c} onRemove={() => toggle(c)}>{c}</Tag>)}
              <button onClick={() => setChips([])} style={{ border: 0, background: 'none', cursor: 'pointer', font: 'var(--type-body-sm)', color: 'var(--text-muted)', textDecoration: 'underline' }}>Clear all</button>
            </div>
          ) : null}
          <div style={{ display: 'grid', gridTemplateColumns: 'repeat(3,1fr)', gap: 'var(--space-6)' }}>
            {products.map((p) => <ProductCard key={p.id} p={p} onOpen={() => go('product')} onAdd={add} />)}
          </div>
          <div style={{ display: 'flex', justifyContent: 'center', gap: 'var(--space-2)', marginTop: 'var(--space-10)' }}>
            {['1', '2', '3', '…', '47'].map((n, i) => (
              <button key={i} style={{ minWidth: '40px', height: '40px', borderRadius: 'var(--radius-sm)', cursor: 'pointer', border: '1px solid ' + (i === 0 ? 'var(--brand-black)' : 'var(--border-default)'), background: i === 0 ? 'var(--brand-black)' : '#fff', color: i === 0 ? '#fff' : 'var(--text-primary)', font: 'var(--type-body-sm)', fontWeight: 'var(--fw-semibold)' }}>{n}</button>
            ))}
          </div>
        </div>
      </div>
    </main>
  );
}

Object.assign(window, { CollectionScreen });
