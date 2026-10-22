const { Button, Card, Badge, Eyebrow, SectionHeading, Icon } = window.SDEuropeanDesignSystem_614674 || {};

const SLIDES = [
  { eyebrow: 'Audi | BMW | Mercedes | Mini | Skoda | VW', title: 'Your Parts & Servicing Specialists', cta: 'Shop Parts' },
  { eyebrow: 'Audi | BMW | Mercedes | Mini | Skoda | VW', title: "Used & Aftermarket Bumpers in Stock", cta: 'Browse Dismantling' },
  { eyebrow: 'Audi | BMW | Mercedes | Mini | Skoda | VW', title: "NZ's Home Of European Cars", cta: 'Book a Service' }
];

const MARQUES = ['Audi', 'BMW', 'Mercedes-Benz', 'Mini', 'Skoda', 'Volkswagen', 'Land Rover'];

const HELP = [
  ['wrench', 'Audi, BMW, Mercedes, Mini, Skoda & VW servicing'],
  ['package', 'New and used parts, in stock in Hamilton'],
  ['truck', 'Shipping NZ wide — overnight in most cases'],
  ['shield-check', 'Authorised mechanical warranty repairers'],
  ['badge-check', 'MTA member & licensed motor vehicle trader'],
  ['car', 'Courtesy cars available'],
  ['clock', 'In business for over 30 years'],
  ['credit-card', 'Visa / Mastercard accepted']
];

function HomeScreen({ go }) {
  const [slide, setSlide] = React.useState(0);
  React.useEffect(() => { const t = setInterval(() => setSlide((s) => (s + 1) % SLIDES.length), 5200); return () => clearInterval(t); }, []);
  const s = SLIDES[slide];
  return (
    <main>
      {/* Hero */}
      <div style={{ position: 'relative', height: '520px', background: 'linear-gradient(135deg,#17191c,#43474d)', overflow: 'hidden' }}>
        <span style={{ position: 'absolute', inset: 0, display: 'grid', placeItems: 'center', font: 'var(--type-label)', letterSpacing: 'var(--ls-wide)', textTransform: 'uppercase', color: 'rgba(255,255,255,.22)' }}>Full-bleed workshop photography</span>
        <div style={{ position: 'absolute', inset: 0, background: 'var(--scrim-bottom)' }} />
        <div style={{ position: 'absolute', inset: 0, display: 'grid', alignItems: 'end' }}>
          <div style={{ maxWidth: 'var(--container-max)', width: '100%', margin: '0 auto', padding: '0 var(--gutter) var(--space-16)' }}>
            <div key={slide} style={{ display: 'grid', gap: 'var(--space-4)', maxWidth: '740px', animation: 'sdRise var(--dur-slow) var(--ease-out) both' }}>
              <Eyebrow tone="accent">{s.eyebrow.toUpperCase().replace(/ \| /g, ' | ')}</Eyebrow>
              <h1 style={{ font: 'var(--type-display)', fontSize: '58px', color: '#fff', letterSpacing: 'var(--ls-tight)' }}>{s.title}</h1>
              <div><Button size="lg" iconRight={<Icon name="arrow-right" size={20} />}>{s.cta}</Button></div>
            </div>
          </div>
        </div>
        <div style={{ position: 'absolute', bottom: 'var(--space-6)', right: 'calc(50% - var(--container-max) / 2 + var(--gutter))', display: 'flex', gap: '8px' }}>
          {SLIDES.map((_, i) => <button key={i} onClick={() => setSlide(i)} aria-label={'Slide ' + (i + 1)} style={{ width: i === slide ? '28px' : '10px', height: '4px', border: 0, padding: 0, background: i === slide ? 'var(--brand-yellow)' : 'rgba(255,255,255,.4)', cursor: 'pointer', transition: 'width var(--dur-base) var(--ease-standard)' }} />)}
        </div>
      </div>

      {/* Three routes */}
      <Section pad="var(--space-12)">
        <div style={{ display: 'grid', gridTemplateColumns: 'repeat(3,1fr)', gap: 'var(--space-6)', marginTop: '-84px', position: 'relative' }}>
          {[['Dismantling', 'Over 350 vehicles being parted out right now.', 'dismantling'], ['Parts', 'New aftermarket, OEM and quality used parts.', 'dismantling'], ['Servicing', 'Independent specialist workshop in Te Rapa.', 'servicing']].map(([t, d, sc]) => (
            <div key={t} onClick={() => go(sc)} style={{ background: 'var(--brand-black)', color: '#fff', padding: 'var(--space-8)', borderRadius: 'var(--radius-md)', display: 'grid', gap: 'var(--space-2)', cursor: 'pointer', boxShadow: 'var(--shadow-lg)' }}>
              <h3 style={{ font: 'var(--type-h3)', color: '#fff' }}>{t}</h3>
              <p style={{ font: 'var(--type-body-sm)', color: 'rgba(255,255,255,.7)' }}>{d}</p>
              <span style={{ marginTop: 'var(--space-3)', color: 'var(--brand-yellow)', font: 'var(--type-button)', letterSpacing: 'var(--ls-wide)', textTransform: 'uppercase', display: 'inline-flex', alignItems: 'center', gap: '8px' }}>Explore <Icon name="arrow-right" size={16} /></span>
            </div>
          ))}
        </div>
      </Section>

      {/* Dismantling marques */}
      <Section tone="grey">
        <SectionHeading eyebrow="Latest Arrivals" title="Now Dismantling Car Parts For:" />
        <div style={{ display: 'grid', gridTemplateColumns: 'repeat(7,1fr)', gap: 'var(--space-4)', marginTop: 'var(--space-8)' }}>
          {MARQUES.map((m) => (
            <div key={m} onClick={() => go('dismantling')} style={{ cursor: 'pointer', borderRadius: 'var(--radius-lg)', overflow: 'hidden', border: '1px solid var(--border-default)', background: '#fff' }}>
              <Photo label="Vehicle" ratio="4 / 3" />
              <div style={{ padding: 'var(--space-3)', font: 'var(--type-h4)', textAlign: 'center' }}>{m}</div>
            </div>
          ))}
        </div>
      </Section>

      {/* Parts + workshop split */}
      <Section>
        <SectionHeading eyebrow="Parts & Service" title="European Car Parts Supplier and Vehicle Repair Workshop" />
        <div style={{ display: 'grid', gridTemplateColumns: '1fr 1.1fr', gap: 'var(--space-12)', marginTop: 'var(--space-8)', alignItems: 'start' }}>
          <div style={{ borderRadius: 'var(--radius-lg)', overflow: 'hidden' }}><Photo label="Aftermarket parts on the shelf" ratio="6 / 5" /></div>
          <div style={{ display: 'grid', gap: 'var(--space-5)' }}>
            <div>
              <h3 style={{ font: 'var(--type-h3)' }}>Parts — New &amp; Used</h3>
              <p style={{ font: 'var(--type-body)', color: 'var(--text-secondary)', marginTop: 'var(--space-2)' }}>Looking for parts for your car or workshop? Then you've made it to the right place, as we're one of NZ's largest suppliers of New and Used European parts. With stock of over 350 Audi, BMW, Mercedes, Mini, Skoda &amp; VW and new arrivals every day, we probably have the part you need.</p>
              <p style={{ font: 'var(--type-body)', color: 'var(--text-secondary)', marginTop: 'var(--space-3)' }}>We're based in Hamilton, but you're probably in Auckland, Tauranga, Wellington, Christchurch or Invercargill? That's no problem, because all our parts are in stock and ready to fire out to you overnight NZ wide.</p>
            </div>
            <div>
              <h3 style={{ font: 'var(--type-h3)' }}>Mechanical Workshop in Te Rapa</h3>
              <p style={{ font: 'var(--type-body)', color: 'var(--text-secondary)', marginTop: 'var(--space-2)' }}>We're an independent specialist workshop, which means we have special equipment, knowledge and parts in stock for your Euro vehicle — without expensive dealership licensing fees.</p>
            </div>
            <div style={{ display: 'flex', gap: 'var(--space-3)' }}>
              <Button onClick={() => go('dismantling')}>Browse Parts</Button>
              <Button variant="outline" onClick={() => go('servicing')}>Learn More About Servicing</Button>
            </div>
          </div>
        </div>
      </Section>

      {/* How we can help — black band with symbol watermark */}
      <div style={{ position: 'relative', overflow: 'hidden', background: 'var(--brand-black)' }}>
        <img src="../../assets/symbol-black-yellow.png" alt="" style={{ position: 'absolute', right: '-160px', top: '-120px', width: '620px', opacity: .08, filter: 'grayscale(1) brightness(3)' }} />
        <Section tone="black" style={{ position: 'relative', background: 'transparent' }}>
          <SectionHeading inverse eyebrow="Why SD European" title="How we can help you" />
          <div style={{ display: 'grid', gridTemplateColumns: 'repeat(4,1fr)', gap: 'var(--space-6) var(--space-8)', marginTop: 'var(--space-10)' }}>
            {HELP.map(([ic, t]) => (
              <div key={t} style={{ display: 'flex', gap: 'var(--space-3)', color: '#fff' }}>
                <span style={{ color: 'var(--brand-yellow)' }}><Icon name={ic} size={24} /></span>
                <span style={{ font: 'var(--type-body-sm)', color: 'rgba(255,255,255,.82)' }}>{t}</span>
              </div>
            ))}
          </div>
        </Section>
      </div>

      {/* Insurers */}
      <Section tone="grey" pad="var(--space-12)">
        <Eyebrow>Authorised mechanical repairers for these companies and more</Eyebrow>
        <div style={{ display: 'grid', gridTemplateColumns: 'repeat(5,1fr)', gap: 'var(--space-6)', marginTop: 'var(--space-6)' }}>
          {['Provident Insurance', 'Autosure NZ', 'Protecta Insurance', 'AA Insurance', 'Janssen Insurance'].map((n) => (
            <div key={n} style={{ height: '72px', background: '#fff', border: '1px solid var(--border-default)', borderRadius: 'var(--radius-md)', display: 'grid', placeItems: 'center', font: 'var(--type-label)', color: 'var(--text-muted)', textAlign: 'center', padding: '0 8px' }}>{n}</div>
          ))}
        </div>
        <p style={{ font: 'var(--type-body-sm)', color: 'var(--text-muted)', marginTop: 'var(--space-4)' }}>Partner marks are placeholders — supply the real logo files and we'll drop them in.</p>
      </Section>
    </main>
  );
}

Object.assign(window, { HomeScreen });
