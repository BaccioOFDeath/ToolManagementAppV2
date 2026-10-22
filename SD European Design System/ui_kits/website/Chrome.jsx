const { Logo, Eyebrow, Button, IconButton, Icon } = window.SDEuropeanDesignSystem_614674 || {};

const NAV = [
  { label: 'Parts Shop', screen: null, href: '#' },
  { label: 'Dismantling', screen: 'dismantling' },
  { label: 'Parts', screen: 'dismantling' },
  { label: 'Servicing', screen: 'servicing', menu: ['Services', 'WOF Inspection', 'Performance Tuning', 'Audi & VW DSG Servicing', 'Key Cutting', 'Commercial Vehicles'] },
  { label: 'About', screen: null, menu: ['Staff', 'Latest News', 'Advice Centre', 'Careers'] },
  { label: 'Contact', screen: 'booking', menu: ['Online Booking', 'Sell Your Vehicle'] }
];

function UtilityBar() {
  return (
    <div style={{ height: 'var(--utilitybar-h)', background: 'var(--brand-black)', color: 'rgba(255,255,255,.8)', display: 'flex', alignItems: 'center' }}>
      <div style={{ maxWidth: 'var(--container-max)', width: '100%', margin: '0 auto', padding: '0 var(--gutter)', display: 'flex', alignItems: 'center', gap: 'var(--space-6)', font: 'var(--type-body-sm)' }}>
        <a href="#" style={{ color: 'var(--brand-yellow)', borderBottom: 0, font: 'var(--type-label)', letterSpacing: 'var(--ls-wide)', textTransform: 'uppercase' }}>Online Booking</a>
        <span style={{ marginLeft: 'auto', display: 'inline-flex', alignItems: 'center', gap: '8px' }}><Icon name="phone" size={16} />Any questions? Call us on <strong style={{ color: '#fff' }}>0800 269 772</strong></span>
        <span style={{ display: 'inline-flex', gap: '10px', opacity: .7 }}><Icon name="facebook" size={16} /><Icon name="instagram" size={16} /><Icon name="linkedin" size={16} /></span>
      </div>
    </div>
  );
}

function Header({ go, current }) {
  const [open, setOpen] = React.useState(null);
  return (
    <header style={{ position: 'sticky', top: 0, zIndex: 50, background: 'rgba(255,255,255,.94)', backdropFilter: 'blur(10px)', borderBottom: '1px solid var(--border-default)' }}>
      <div style={{ height: 'var(--header-h)', maxWidth: 'var(--container-max)', margin: '0 auto', padding: '0 var(--gutter)', display: 'flex', alignItems: 'center', gap: 'var(--space-8)' }}>
        <a href="#" onClick={(e) => { e.preventDefault(); go('home'); }} style={{ borderBottom: 0 }}><Logo height={38} assetBase="../../assets" /></a>
        <nav style={{ display: 'flex', gap: 'var(--space-6)', marginLeft: 'auto' }}>
          {NAV.map((n) => (
            <div key={n.label} style={{ position: 'relative' }} onMouseEnter={() => setOpen(n.label)} onMouseLeave={() => setOpen(null)}>
              <button onClick={() => n.screen && go(n.screen)} style={{
                background: 'none', border: 0, padding: '0 0 4px', cursor: 'pointer',
                font: 'var(--type-button)', fontSize: 'var(--fs-sm)', letterSpacing: 'var(--ls-wide)', textTransform: 'uppercase',
                color: current === n.screen ? 'var(--text-primary)' : 'var(--text-secondary)',
                borderBottom: '3px solid ' + (current === n.screen ? 'var(--brand-yellow)' : 'transparent'), transition: 'var(--transition-control)'
              }}>{n.label}</button>
              {n.menu && open === n.label ? (
                <div style={{ position: 'absolute', top: '100%', left: '-14px', marginTop: '14px', background: '#fff', border: '1px solid var(--border-default)', borderRadius: 'var(--radius-md)', boxShadow: 'var(--shadow-md)', padding: 'var(--space-2)', minWidth: '220px', display: 'grid' }}>
                  {n.menu.map((m) => <a key={m} href="#" onClick={(e) => { e.preventDefault(); n.screen && go(n.screen); }} style={{ borderBottom: 0, padding: '9px 12px', borderRadius: 'var(--radius-sm)', font: 'var(--type-body-sm)', color: 'var(--text-primary)' }}>{m}</a>)}
                </div>
              ) : null}
            </div>
          ))}
        </nav>
        <Button size="sm" onClick={() => go('booking')}>Book a Service</Button>
      </div>
    </header>
  );
}

function Photo({ label, ratio = '16 / 9', dark = false, children, style }) {
  return (
    <div style={{
      position: 'relative', aspectRatio: ratio, width: '100%', overflow: 'hidden',
      background: dark ? 'linear-gradient(135deg,#1c1e21,#4a4e54)' : 'linear-gradient(135deg,#d9dbdd,#f1f2f3)',
      display: 'grid', placeItems: 'center', ...style
    }}>
      <span style={{ position: 'absolute', font: 'var(--type-label)', letterSpacing: 'var(--ls-wide)', textTransform: 'uppercase', color: dark ? 'rgba(255,255,255,.34)' : 'var(--neutral-400)' }}>{label}</span>
      {children}
    </div>
  );
}

function Footer() {
  const col = (h, items) => (
    <div style={{ display: 'grid', gap: 'var(--space-3)', alignContent: 'start' }}>
      <Eyebrow tone="accent">{h}</Eyebrow>
      {items.map((i) => <a key={i} href="#" style={{ color: 'rgba(255,255,255,.72)', borderBottom: 0, font: 'var(--type-body-sm)' }}>{i}</a>)}
    </div>
  );
  return (
    <footer style={{ background: 'var(--brand-black)', color: 'rgba(255,255,255,.72)', paddingTop: 'var(--space-16)' }}>
      <div style={{ maxWidth: 'var(--container-max)', margin: '0 auto', padding: '0 var(--gutter)' }}>
        <div style={{ display: 'grid', gridTemplateColumns: '1.4fr 1fr 1fr 1fr', gap: 'var(--space-10)' }}>
          <div style={{ display: 'grid', gap: 'var(--space-4)', alignContent: 'start' }}>
            <Logo variant="lockup-white" height={34} assetBase="../../assets" />
            <p style={{ font: 'var(--type-body-sm)', maxWidth: '280px' }}>Your European vehicle experts. Parts and servicing for Audi, BMW, Mercedes, Mini, Skoda and VW — Hamilton, and NZ wide.</p>
          </div>
          {col('Parts', ['Parts Shop', 'Cars for Dismantling', 'New & Used Parts', 'Sell Your Vehicle'])}
          {col('Servicing', ['Services', 'WOF Inspection', 'Performance Tuning', 'DSG Servicing', 'Key Cutting'])}
          {col('Company', ['About', 'Staff', 'Careers', 'Latest News', 'Contact'])}
        </div>
        <div style={{ marginTop: 'var(--space-12)', paddingTop: 'var(--space-6)', borderTop: '1px solid var(--border-inverse)', display: 'flex', gap: 'var(--space-8)', flexWrap: 'wrap', font: 'var(--type-body-sm)' }}>
          <span><strong style={{ color: '#fff' }}>Parts</strong> · 177 Ellis Street, Frankton, Hamilton</span>
          <span><strong style={{ color: '#fff' }}>Service</strong> · 23 Norris Ave, Te Rapa, Hamilton</span>
          <span><strong style={{ color: '#fff' }}>Freephone</strong> · 0800 269 772</span>
        </div>
        <div style={{ padding: 'var(--space-6) 0 var(--space-10)', font: 'var(--type-body-sm)', color: 'rgba(255,255,255,.45)' }}>
          © Copyright 2026 SD European. All Rights Reserved.<br />
          SD European is an independent parts and service specialist and has no association with BMW New Zealand or BMW AG.
        </div>
      </div>
    </footer>
  );
}

function Section({ children, tone = 'white', pad = 'var(--section-y)', style }) {
  const bg = tone === 'grey' ? 'var(--surface-subtle)' : tone === 'black' ? 'var(--brand-black)' : 'var(--surface-page)';
  return (
    <section style={{ background: bg, padding: pad + ' 0', ...style }}>
      <div style={{ maxWidth: 'var(--container-max)', margin: '0 auto', padding: '0 var(--gutter)' }}>{children}</div>
    </section>
  );
}

Object.assign(window, { UtilityBar, Header, Footer, Section, Photo, WEBSITE_NAV: NAV });
