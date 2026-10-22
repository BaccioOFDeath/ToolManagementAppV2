const { Logo, Eyebrow, Button, IconButton, Icon, Input, Badge } = window.SDEuropeanDesignSystem_614674 || {};

const MARQUE_NAV = ['Shop BMW', 'Shop Mini', 'Shop Audi', 'Shop Skoda', 'Shop VW', 'Shop Mercedes'];
const CATEGORIES = ['Accessories', 'Wiper Blades', 'Window Regulator', 'Headlights', 'Tail Light', 'Water Pump', 'Coolant Hose', 'Timing Belt Kits', 'Alternators', 'Starter Motor', 'Air Filter', 'Oil Filter', 'Cabin Filter', 'Brake Pads', 'PCV Valve', 'Coding & Cloning'];
const BRANDS = ['Valeo', 'Teilex', 'Topran', 'Mann-Filter', 'Hella Lighting', 'Arnott Suspension', 'Vemo & Vaico', 'Magneti Marelli'];

function AnnouncementBar() {
  return (
    <div style={{ background: 'var(--brand-yellow)', color: 'var(--brand-black)', textAlign: 'center', padding: '9px 0', font: 'var(--type-label)', letterSpacing: 'var(--ls-wide)', textTransform: 'uppercase' }}>
      Free shipping on orders over $175
    </div>
  );
}

function ShopUtilityBar() {
  return (
    <div style={{ background: 'var(--brand-black)', color: 'rgba(255,255,255,.75)' }}>
      <div style={{ maxWidth: 'var(--container-wide)', margin: '0 auto', padding: '0 var(--gutter-lg)', height: 'var(--utilitybar-h)', display: 'flex', alignItems: 'center', gap: 'var(--space-6)', font: 'var(--type-body-sm)' }}>
        <span style={{ display: 'inline-flex', alignItems: 'center', gap: '8px', color: '#fff' }}><Icon name="phone" size={16} />0800 269 772</span>
        <span>All Parts in Stock in NZ</span>
        <strong style={{ marginLeft: 'auto', color: 'var(--brand-yellow)' }}>Supplying New &amp; Used parts in New Zealand for 30+ years</strong>
        <a href="#" style={{ color: 'rgba(255,255,255,.75)', borderBottom: 0 }}>Our Regular Website</a>
      </div>
    </div>
  );
}

function ShopHeader({ go, cartCount, onCart }) {
  const [menu, setMenu] = React.useState(null);
  return (
    <header style={{ position: 'sticky', top: 0, zIndex: 60, background: '#fff', borderBottom: '1px solid var(--border-default)' }}>
      <div style={{ maxWidth: 'var(--container-wide)', margin: '0 auto', padding: '0 var(--gutter-lg)', height: '84px', display: 'flex', alignItems: 'center', gap: 'var(--space-8)' }}>
        <a href="#" onClick={(e) => { e.preventDefault(); go('home'); }} style={{ borderBottom: 0 }}><Logo height={40} assetBase="../../assets" /></a>
        <div style={{ flex: 1, maxWidth: '560px' }}>
          <Input placeholder="Search the store — part name, number or rego" prefix={<Icon name="search" size={20} />} aria-label="Search the store" />
        </div>
        <div style={{ marginLeft: 'auto', display: 'flex', alignItems: 'center', gap: 'var(--space-2)' }}>
          <Button variant="ghost" size="sm" iconLeft={<Icon name="user" size={20} />}>Sign In</Button>
          <button onClick={onCart} style={{ position: 'relative', border: '1px solid var(--border-default)', background: '#fff', borderRadius: 'var(--radius-sm)', height: '40px', padding: '0 14px', display: 'inline-flex', alignItems: 'center', gap: '8px', cursor: 'pointer', font: 'var(--type-button)', fontSize: 'var(--fs-sm)', textTransform: 'uppercase', letterSpacing: 'var(--ls-wide)' }}>
            <Icon name="shopping-cart" size={20} />Cart
            <span style={{ background: 'var(--brand-yellow)', color: 'var(--brand-black)', borderRadius: 'var(--radius-pill)', minWidth: '22px', height: '20px', display: 'grid', placeItems: 'center', font: 'var(--type-mono)', fontSize: 'var(--fs-xs)' }}>{cartCount}</span>
          </button>
        </div>
      </div>
      <nav style={{ borderTop: '1px solid var(--border-default)', background: 'var(--surface-subtle)' }}>
        <div style={{ maxWidth: 'var(--container-wide)', margin: '0 auto', padding: '0 var(--gutter-lg)', display: 'flex', gap: 'var(--space-6)', height: '48px', alignItems: 'center' }}
          onMouseLeave={() => setMenu(null)}>
          {MARQUE_NAV.map((m) => (
            <button key={m} onClick={() => go('collection')} onMouseEnter={() => setMenu(null)} style={{ border: 0, background: 'none', cursor: 'pointer', font: 'var(--type-button)', fontSize: 'var(--fs-sm)', letterSpacing: 'var(--ls-wide)', textTransform: 'uppercase' }}>{m}</button>
          ))}
          {[['Popular Categories', CATEGORIES], ['Popular Brands', BRANDS]].map(([label, items]) => (
            <div key={label} style={{ position: 'relative' }} onMouseEnter={() => setMenu(label)}>
              <button style={{ border: 0, background: 'none', cursor: 'pointer', display: 'inline-flex', alignItems: 'center', gap: '6px', font: 'var(--type-button)', fontSize: 'var(--fs-sm)', letterSpacing: 'var(--ls-wide)', textTransform: 'uppercase' }}>{label}<Icon name="chevron-down" size={16} /></button>
              {menu === label ? (
                <div style={{ position: 'absolute', top: '100%', left: '-16px', marginTop: '12px', background: '#fff', border: '1px solid var(--border-default)', borderRadius: 'var(--radius-md)', boxShadow: 'var(--shadow-md)', padding: 'var(--space-4)', display: 'grid', gridTemplateColumns: 'repeat(2,minmax(180px,1fr))', gap: '2px var(--space-6)', zIndex: 70 }}>
                  {items.map((i) => <a key={i} href="#" onClick={(e) => { e.preventDefault(); go('collection'); }} style={{ borderBottom: 0, padding: '7px 10px', borderRadius: 'var(--radius-sm)', font: 'var(--type-body-sm)', color: 'var(--text-primary)' }}>{i}</a>)}
                </div>
              ) : null}
            </div>
          ))}
        </div>
      </nav>
    </header>
  );
}

const TRUST = [
  ['truck', 'Nationwide delivery', 'Order before 2pm for overnight delivery in most cases. Large items 2–3 days.'],
  ['shield-check', 'Guaranteed fit', 'We manually check each order with your rego number to be 100% sure it will fit.'],
  ['badge-check', '12 months warranty', 'All new parts carry a 12 month part-only warranty so you can buy with confidence.'],
  ['heart', 'Kiwi owned & operated', "We're Kiwis helping Kiwis and we've been doing it for over 30 years."]
];

function TrustRow() {
  return (
    <div style={{ background: 'var(--surface-subtle)', borderTop: '1px solid var(--border-default)' }}>
      <div style={{ maxWidth: 'var(--container-wide)', margin: '0 auto', padding: 'var(--space-10) var(--gutter-lg)', display: 'grid', gridTemplateColumns: 'repeat(4,1fr)', gap: 'var(--space-8)' }}>
        {TRUST.map(([ic, t, d]) => (
          <div key={t} style={{ display: 'grid', gap: 'var(--space-2)' }}>
            <Icon name={ic} size={32} />
            <div style={{ font: 'var(--type-h4)', textTransform: 'uppercase', letterSpacing: 'var(--ls-wide)', fontSize: 'var(--fs-sm)' }}>{t}</div>
            <p style={{ font: 'var(--type-body-sm)', color: 'var(--text-secondary)' }}>{d}</p>
          </div>
        ))}
      </div>
    </div>
  );
}

function ShopFooter() {
  const col = (h, items) => (
    <div style={{ display: 'grid', gap: 'var(--space-3)', alignContent: 'start' }}>
      <Eyebrow tone="accent">{h}</Eyebrow>
      {items.map((i) => <a key={i} href="#" style={{ color: 'rgba(255,255,255,.7)', borderBottom: 0, font: 'var(--type-body-sm)' }}>{i}</a>)}
    </div>
  );
  return (
    <footer style={{ background: 'var(--brand-black)', color: 'rgba(255,255,255,.7)' }}>
      <div style={{ maxWidth: 'var(--container-wide)', margin: '0 auto', padding: 'var(--space-16) var(--gutter-lg) var(--space-8)', display: 'grid', gridTemplateColumns: '1.4fr 1fr 1fr 1fr', gap: 'var(--space-10)' }}>
        <div style={{ display: 'grid', gap: 'var(--space-4)', alignContent: 'start' }}>
          <Eyebrow tone="accent">Newsletter Sign Up</Eyebrow>
          <div style={{ display: 'flex', gap: 'var(--space-2)' }}>
            <input placeholder="Your email" style={{ flex: 1, height: '44px', padding: '0 12px', border: '1px solid var(--border-inverse)', background: 'transparent', color: '#fff', borderRadius: 'var(--radius-sm)', font: 'var(--type-body)' }} />
            <Button size="md">Submit</Button>
          </div>
          <p style={{ font: 'var(--type-body-sm)' }}>Need help finding the right part for your European car? Call 0800 269 772 or email parts@sdeuropean.co.nz</p>
          <p style={{ font: 'var(--type-body-sm)' }}>177 Ellis Street, Hamilton, Waikato</p>
        </div>
        {col('Customer Service', ['Contact Us', 'Refund Policy', 'Privacy Policy', 'Terms of Service', 'Delivery', 'Warranty'])}
        {col('Used Parts', ['Audi Dismantling', 'BMW Dismantling', 'Mercedes Dismantling', 'Mini Dismantling', 'Skoda Dismantling', 'Volkswagen Dismantling'])}
        {col('Follow Us', ['Facebook', 'Instagram', 'LinkedIn', 'Blog'])}
      </div>
      <div style={{ maxWidth: 'var(--container-wide)', margin: '0 auto', padding: '0 var(--gutter-lg) var(--space-10)', font: 'var(--type-body-sm)', color: 'rgba(255,255,255,.45)', borderTop: '1px solid var(--border-inverse)', paddingTop: 'var(--space-6)' }}>
        © Copyright 2026 SD European. All Rights Reserved. SD European is an independent parts and service specialist and has no association with BMW New Zealand or BMW AG.
      </div>
    </footer>
  );
}

function ShopPhoto({ label, ratio = '1 / 1', dark = false, style }) {
  return (
    <div style={{ position: 'relative', aspectRatio: ratio, width: '100%', background: dark ? 'linear-gradient(135deg,#1c1e21,#4a4e54)' : 'linear-gradient(135deg,#e8e9ea,#f7f7f8)', display: 'grid', placeItems: 'center', ...style }}>
      <span style={{ font: 'var(--type-label)', letterSpacing: 'var(--ls-wide)', textTransform: 'uppercase', color: dark ? 'rgba(255,255,255,.3)' : 'var(--neutral-400)' }}>{label}</span>
    </div>
  );
}

Object.assign(window, { AnnouncementBar, ShopUtilityBar, ShopHeader, TrustRow, ShopFooter, ShopPhoto, SHOP_CATEGORIES: CATEGORIES, SHOP_BRANDS: BRANDS });
