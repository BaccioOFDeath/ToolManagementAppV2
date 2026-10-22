const { Button, Card, Badge, Eyebrow, SectionHeading, Icon, Tabs } = window.SDEuropeanDesignSystem_614674 || {};

const SERVICES = [
  ['wrench', 'Servicing', 'Scheduled and interim servicing for Audi, BMW, Mercedes, Mini, Skoda and VW.'],
  ['clipboard-check', 'WOF Inspection', 'Warrant of Fitness, booked in and out the same day at Te Rapa.'],
  ['gauge', 'Performance Tuning', 'ECU remapping and performance work by specialists who know the platform.'],
  ['cog', 'Audi & VW DSG Servicing', 'DSG fluid and filter service using the correct fill procedure and specification.'],
  ['key-round', 'Key Cutting', 'Replacement and spare keys for BMW, Audi and VW — cut and coded in house.'],
  ['truck', 'Commercial Vehicles', 'Fleet and commercial servicing with minimum downtime.']
];

const REASONS = [
  ['Expertise', "We're a fully independent specialist garage who only work on European cars, with the tools and the parts on the shelf."],
  ['Trustworthy', "After you hand over your keys, you'll drive off in the loan car with your mind at ease."],
  ['Excellent Communication', "Later in the day you'll get a friendly update. We never do extra work without your clear authorisation first — so no nasty surprises at the eftpos machine."],
  ['Value', 'A family business with 30 years experience, known for excellent value for money.']
];

const REVIEWS = [
  ['These people make you feel welcome and valued every time you call or have your vehicle serviced. Highly recommended! I drive 1.5 hours to get my Audi serviced here.', 'Steve Dyer'],
  ['SD European are always fast, professional, and efficient with their work. I highly recommend them.', 'Daniel Compton, Hamilton'],
  ['Moved to SD European and am incredibly impressed with their workmanship, cleanliness and customer service!', 'Sue Watson']
];

function ServicingScreen({ go }) {
  const [tab, setTab] = React.useState('why');
  return (
    <main>
      <div style={{ position: 'relative', height: '360px', background: 'linear-gradient(135deg,#1c1e21,#4a4e54)' }}>
        <span style={{ position: 'absolute', inset: 0, display: 'grid', placeItems: 'center', font: 'var(--type-label)', letterSpacing: 'var(--ls-wide)', textTransform: 'uppercase', color: 'rgba(255,255,255,.22)' }}>Technician at work — close-up</span>
        <div style={{ position: 'absolute', inset: 0, background: 'var(--scrim-left)' }} />
        <div style={{ position: 'absolute', inset: 0, display: 'grid', alignItems: 'center' }}>
          <div style={{ maxWidth: 'var(--container-max)', width: '100%', margin: '0 auto', padding: '0 var(--gutter)' }}>
            <div style={{ maxWidth: '620px', display: 'grid', gap: 'var(--space-4)' }}>
              <Eyebrow tone="accent">Service Centre · Te Rapa</Eyebrow>
              <h1 style={{ font: 'var(--type-h1)', color: '#fff' }}>European Car Service &amp; Repairs</h1>
              <p style={{ font: 'var(--type-lead)', color: 'rgba(255,255,255,.8)' }}>Does your BMW, Volkswagen, Mercedes, Audi, Mini or Skoda need servicing? We are a fully independent specialist garage.</p>
              <div><Button size="lg" onClick={() => go('booking')}>Book a Service</Button></div>
            </div>
          </div>
        </div>
      </div>

      <Section>
        <SectionHeading eyebrow="What we do" title="Our Services" />
        <div style={{ display: 'grid', gridTemplateColumns: 'repeat(3,1fr)', gap: 'var(--space-6)', marginTop: 'var(--space-8)' }}>
          {SERVICES.map(([ic, t, d]) => (
            <Card key={t} interactive onClick={() => go('booking')} title={t} footer={<span style={{ font: 'var(--type-button)', letterSpacing: 'var(--ls-wide)', textTransform: 'uppercase', display: 'inline-flex', alignItems: 'center', gap: '8px' }}>Book <Icon name="arrow-right" size={16} /></span>}>
              <span style={{ display: 'block', marginBottom: 'var(--space-2)' }}><Icon name={ic} size={32} /></span>
              {d}
            </Card>
          ))}
        </div>
      </Section>

      <Section tone="grey">
        <Tabs items={[{ value: 'why', label: 'Why us' }, { value: 'reviews', label: 'Reviews', count: 3 }, { value: 'visit', label: 'Visiting' }]} value={tab} onChange={setTab} />
        <div style={{ marginTop: 'var(--space-8)' }}>
          {tab === 'why' ? (
            <div style={{ display: 'grid', gridTemplateColumns: 'repeat(4,1fr)', gap: 'var(--space-6)' }}>
              {REASONS.map(([t, d]) => (
                <div key={t} style={{ display: 'grid', gap: 'var(--space-2)', alignContent: 'start' }}>
                  <hr className="sd-rule" style={{ margin: 0 }} />
                  <h3 style={{ font: 'var(--type-h4)', marginTop: 'var(--space-2)' }}>{t}</h3>
                  <p style={{ font: 'var(--type-body-sm)', color: 'var(--text-secondary)' }}>{d}</p>
                </div>
              ))}
            </div>
          ) : tab === 'reviews' ? (
            <div style={{ display: 'grid', gridTemplateColumns: 'repeat(3,1fr)', gap: 'var(--space-6)' }}>
              {REVIEWS.map(([q, who]) => (
                <blockquote key={who} style={{ margin: 0, background: '#fff', border: '1px solid var(--border-default)', borderRadius: 'var(--radius-md)', padding: 'var(--space-6)', display: 'grid', gap: 'var(--space-3)' }}>
                  <span style={{ color: 'var(--brand-yellow)', display: 'flex', gap: '2px' }}>{[0, 1, 2, 3, 4].map((i) => <Icon key={i} name="star" size={16} />)}</span>
                  <p style={{ font: 'var(--type-body)' }}>“{q}”</p>
                  <cite style={{ font: 'var(--type-label)', letterSpacing: 'var(--ls-wide)', textTransform: 'uppercase', color: 'var(--text-muted)', fontStyle: 'normal' }}>{who}</cite>
                </blockquote>
              ))}
            </div>
          ) : (
            <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 'var(--space-6)' }}>
              {[['Service Centre', '23 Norris Avenue, Te Rapa, Hamilton 3200', 'By the crossroads of Norris Ave and Bryant Road.'], ['Parts Centre', '177 Ellis Street, Frankton, Hamilton 3204', 'Trade and retail counter, open Saturday mornings.']].map(([t, a, d]) => (
                <div key={t} style={{ background: '#fff', border: '1px solid var(--border-default)', borderRadius: 'var(--radius-md)', overflow: 'hidden' }}>
                  <Photo label="Map" ratio="21 / 9" />
                  <div style={{ padding: 'var(--space-6)', display: 'grid', gap: 'var(--space-2)' }}>
                    <h3 style={{ font: 'var(--type-h4)' }}>{t}</h3>
                    <p style={{ font: 'var(--type-body)' }}>{a}</p>
                    <p style={{ font: 'var(--type-body-sm)', color: 'var(--text-muted)' }}>{d}</p>
                  </div>
                </div>
              ))}
            </div>
          )}
        </div>
      </Section>
    </main>
  );
}

Object.assign(window, { ServicingScreen });
