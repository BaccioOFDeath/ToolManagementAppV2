const { Button, Badge, Tag, Select, Input, Eyebrow, SectionHeading, Icon, Card } = window.SDEuropeanDesignSystem_614674 || {};

const VEHICLES = [
  { make: 'BMW', model: '320i E91 Touring', year: '2008', eng: 'N46B20', stock: 'SD-10442', state: 'Now parting out' },
  { make: 'Audi', model: 'A4 B8 2.0 TFSI', year: '2011', eng: 'CDNC', stock: 'SD-10438', state: 'Now parting out' },
  { make: 'Volkswagen', model: 'Golf Mk6 GTI', year: '2010', eng: 'CCZB', stock: 'SD-10431', state: 'Arriving' },
  { make: 'Mercedes-Benz', model: 'C200 W204', year: '2009', eng: 'M271', stock: 'SD-10427', state: 'Now parting out' },
  { make: 'Mini', model: 'Cooper S R56', year: '2012', eng: 'N18', stock: 'SD-10419', state: 'Now parting out' },
  { make: 'Skoda', model: 'Octavia vRS 1Z', year: '2010', eng: 'CDAA', stock: 'SD-10411', state: 'Last few parts' }
];

const MARQUES = ['All', 'Audi', 'BMW', 'Mercedes-Benz', 'Mini', 'Skoda', 'Volkswagen', 'Land Rover'];

function DismantlingScreen({ go }) {
  const [marque, setMarque] = React.useState('All');
  const list = marque === 'All' ? VEHICLES : VEHICLES.filter((v) => v.make === marque);
  return (
    <main>
      <Section tone="grey" pad="var(--space-12)">
        <SectionHeading eyebrow="Used Parts" title="Cars for Dismantling" lead="Over 350 vehicles in the yard. Send through your rego and our parts team will match your VIN to the exact part you need." />
      </Section>

      <Section pad="var(--space-10)">
        <div style={{ display: 'grid', gridTemplateColumns: '1.6fr 1fr 1fr auto', gap: 'var(--space-4)', alignItems: 'end', paddingBottom: 'var(--space-6)', borderBottom: '1px solid var(--border-default)' }}>
          <Input label="Search the yard" placeholder="e.g. E91 tailgate" prefix={<Icon name="search" size={20} />} />
          <Select label="Model year" options={['Any year', '2015+', '2010–2014', '2005–2009', 'Pre-2005']} />
          <Select label="Sort by" options={['Newest arrivals', 'Make A–Z', 'Year, newest']} />
          <Button variant="secondary">Apply</Button>
        </div>

        <div style={{ display: 'flex', gap: 'var(--space-2)', flexWrap: 'wrap', margin: 'var(--space-6) 0 var(--space-8)' }}>
          {MARQUES.map((m) => <Tag key={m} selected={marque === m} onClick={() => setMarque(m)}>{m}</Tag>)}
        </div>

        <div style={{ display: 'grid', gridTemplateColumns: 'repeat(3,1fr)', gap: 'var(--space-6)' }}>
          {list.map((v) => (
            <div key={v.stock} style={{ border: '1px solid var(--border-default)', borderRadius: 'var(--radius-lg)', overflow: 'hidden', background: '#fff' }}>
              <div style={{ position: 'relative' }}>
                <Photo label="Vehicle in the yard" ratio="4 / 3" />
                <span style={{ position: 'absolute', top: '12px', left: '12px' }}>
                  <Badge tone={v.state === 'Arriving' ? 'info' : v.state === 'Last few parts' ? 'warning' : 'success'} dot>{v.state}</Badge>
                </span>
              </div>
              <div style={{ padding: 'var(--space-5)', display: 'grid', gap: 'var(--space-2)' }}>
                <Eyebrow>{v.make}</Eyebrow>
                <h3 style={{ font: 'var(--type-h4)', fontSize: 'var(--fs-lg)' }}>{v.model}</h3>
                <div style={{ display: 'flex', gap: 'var(--space-4)', font: 'var(--type-body-sm)', color: 'var(--text-secondary)' }}>
                  <span>{v.year}</span><span>·</span><span style={{ font: 'var(--type-mono)' }}>{v.eng}</span>
                </div>
                <div style={{ display: 'flex', alignItems: 'center', gap: 'var(--space-3)', marginTop: 'var(--space-3)' }}>
                  <Button size="sm" fullWidth>Enquire About Parts</Button>
                </div>
                <span style={{ font: 'var(--type-mono)', color: 'var(--text-muted)', fontSize: 'var(--fs-xs)' }}>Stock {v.stock}</span>
              </div>
            </div>
          ))}
        </div>
      </Section>

      <Section tone="black">
        <div style={{ display: 'grid', gridTemplateColumns: '1fr auto', gap: 'var(--space-8)', alignItems: 'center' }}>
          <SectionHeading inverse eyebrow="Not sure what you need?" title="Send us your rego and we'll match it" lead="We've a team of experts on the end of a phone or email line, equipped with parts catalogues and a heap of knowledge." />
          <div style={{ display: 'flex', gap: 'var(--space-3)' }}>
            <Button variant="inverse" size="lg" iconLeft={<Icon name="phone" size={20} />}>0800 269 772</Button>
            <Button size="lg" onClick={() => go('booking')}>Send Your Rego</Button>
          </div>
        </div>
      </Section>
    </main>
  );
}

Object.assign(window, { DismantlingScreen });
