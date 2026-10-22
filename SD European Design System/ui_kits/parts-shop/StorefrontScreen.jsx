const { Button, Eyebrow, SectionHeading, Icon, Badge } = window.SDEuropeanDesignSystem_614674 || {};

const TILES = ['Window Regulator', 'Brakes', 'Suspension', 'Electrical', 'Bumpers', 'Oil Filters', 'Cooling', 'Air Filters'];

function StorefrontScreen({ go, add }) {
  return (
    <main>
      <div style={{ position: 'relative', height: '440px', background: 'linear-gradient(135deg,#17191c,#43474d)' }}>
        <span style={{ position: 'absolute', inset: 0, display: 'grid', placeItems: 'center', font: 'var(--type-label)', letterSpacing: 'var(--ls-wide)', textTransform: 'uppercase', color: 'rgba(255,255,255,.2)' }}>Homepage banner</span>
        <div style={{ position: 'absolute', inset: 0, background: 'var(--scrim-left)' }} />
        <div style={{ position: 'absolute', inset: 0, display: 'grid', alignItems: 'center' }}>
          <div style={{ maxWidth: 'var(--container-wide)', width: '100%', margin: '0 auto', padding: '0 var(--gutter-lg)' }}>
            <div style={{ display: 'grid', gap: 'var(--space-4)', maxWidth: '620px' }}>
              <Eyebrow tone="inverse">Welcome to NZ's home of</Eyebrow>
              <h1 style={{ font: 'var(--type-display)', fontSize: '54px', color: '#fff' }}>European Car Parts</h1>
              <Eyebrow tone="accent">AUDI | BMW | MERCEDES | MINI | SKODA | PORSCHE | VW</Eyebrow>
              <div style={{ marginTop: 'var(--space-2)' }}><Button size="lg" onClick={() => go('collection')} iconRight={<Icon name="arrow-right" size={20} />}>Shop Now</Button></div>
            </div>
          </div>
        </div>
      </div>

      <section style={{ maxWidth: 'var(--container-wide)', margin: '0 auto', padding: 'var(--space-16) var(--gutter-lg)' }}>
        <SectionHeading eyebrow="Shop by category" title="New Aftermarket and OEM" />
        <div style={{ display: 'grid', gridTemplateColumns: 'repeat(8,1fr)', gap: 'var(--space-4)', marginTop: 'var(--space-8)' }}>
          {TILES.map((t) => (
            <button key={t} onClick={() => go('collection')} style={{ border: '1px solid var(--border-default)', background: '#fff', borderRadius: 'var(--radius-md)', padding: 0, overflow: 'hidden', cursor: 'pointer', display: 'grid' }}>
              <ShopPhoto label="Part" />
              <span style={{ padding: 'var(--space-3)', font: 'var(--type-label)', letterSpacing: 'var(--ls-wide)', textTransform: 'uppercase' }}>{t}</span>
            </button>
          ))}
        </div>
      </section>

      <section style={{ maxWidth: 'var(--container-wide)', margin: '0 auto', padding: '0 var(--gutter-lg) var(--space-16)' }}>
        <div style={{ display: 'flex', alignItems: 'end', justifyContent: 'space-between', gap: 'var(--space-6)' }}>
          <SectionHeading eyebrow="Fresh in" title="Latest Parts" />
          <Button variant="outline" onClick={() => go('collection')}>View All Parts</Button>
        </div>
        <div style={{ display: 'grid', gridTemplateColumns: 'repeat(4,1fr)', gap: 'var(--space-6)', marginTop: 'var(--space-8)' }}>
          {window.SHOP_PRODUCTS.slice(0, 4).map((p) => <ProductCard key={p.id} p={p} onOpen={() => go('product')} onAdd={add} />)}
        </div>
      </section>

      <div style={{ position: 'relative', overflow: 'hidden', background: 'var(--brand-black)' }}>
        <img src="../../assets/symbol-black-yellow.png" alt="" style={{ position: 'absolute', left: '-140px', bottom: '-180px', width: '560px', opacity: .08, filter: 'grayscale(1) brightness(3)' }} />
        <div style={{ position: 'relative', maxWidth: 'var(--container-wide)', margin: '0 auto', padding: 'var(--space-16) var(--gutter-lg)', display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 'var(--space-12)', alignItems: 'center' }}>
          <SectionHeading inverse eyebrow="Since 1989" title="Who are SD European?" lead="Founded in Hamilton, Waikato from a passion for restoring BMWs. Over 30 years later, our passion for European cars is stronger than ever. With a friendly and skilled team behind us, we provide high-quality aftermarket parts, OEM parts, second-hand parts, and a dedicated service centre." />
          <div style={{ display: 'grid', gridTemplateColumns: 'repeat(3,1fr)', gap: 'var(--space-6)' }}>
            {[['30+', 'Years'], ['350+', 'Cars dismantling'], ['NZ', 'Wide delivery']].map(([n, l]) => (
              <div key={l} style={{ display: 'grid', gap: '4px' }}>
                <span style={{ font: 'var(--type-h1)', fontSize: '44px', color: 'var(--brand-yellow)' }}>{n}</span>
                <span style={{ font: 'var(--type-label)', letterSpacing: 'var(--ls-wide)', textTransform: 'uppercase', color: 'rgba(255,255,255,.6)' }}>{l}</span>
              </div>
            ))}
          </div>
        </div>
      </div>

      <section style={{ maxWidth: 'var(--container-wide)', margin: '0 auto', padding: 'var(--space-16) var(--gutter-lg)' }}>
        <Eyebrow>Top Brands</Eyebrow>
        <div style={{ display: 'grid', gridTemplateColumns: 'repeat(6,1fr)', gap: 'var(--space-4)', marginTop: 'var(--space-5)' }}>
          {['Arnott', 'Vaico / Vemo', 'Mann-Filter', 'Topran', 'Hella', 'Valeo'].map((b) => (
            <div key={b} style={{ height: '80px', border: '1px solid var(--border-default)', borderRadius: 'var(--radius-md)', display: 'grid', placeItems: 'center', font: 'var(--type-label)', color: 'var(--text-muted)' }}>{b}</div>
          ))}
        </div>
      </section>
      <TrustRow />
    </main>
  );
}

Object.assign(window, { StorefrontScreen });
