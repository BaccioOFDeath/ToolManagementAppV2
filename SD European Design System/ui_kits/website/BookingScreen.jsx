const { Button, Input, Select, Checkbox, Radio, Eyebrow, SectionHeading, Icon, Badge, Toast } = window.SDEuropeanDesignSystem_614674 || {};

const SLOTS = ['8:00am', '9:30am', '11:00am', '1:00pm', '2:30pm', '4:00pm'];

function BookingScreen() {
  const [step, setStep] = React.useState(1);
  const [service, setService] = React.useState('service');
  const [slot, setSlot] = React.useState('9:30am');
  const [done, setDone] = React.useState(false);

  return (
    <main>
      <Section tone="grey" pad="var(--space-12)">
        <SectionHeading eyebrow="Service Centre · Te Rapa" title="Online Service Booking" lead="Tell us about your car and pick a time. We'll confirm by phone the same day." />
      </Section>

      <Section pad="var(--space-12)">
        <div style={{ display: 'grid', gridTemplateColumns: '1.5fr 1fr', gap: 'var(--space-12)', alignItems: 'start' }}>
          <div style={{ display: 'grid', gap: 'var(--space-8)' }}>
            <ol style={{ display: 'flex', gap: 'var(--space-6)', listStyle: 'none', padding: 0, margin: 0 }}>
              {['Your car', 'Service', 'Your details'].map((s, i) => (
                <li key={s} style={{ display: 'flex', alignItems: 'center', gap: 'var(--space-2)', font: 'var(--type-label)', letterSpacing: 'var(--ls-wide)', textTransform: 'uppercase', color: step >= i + 1 ? 'var(--text-primary)' : 'var(--text-muted)' }}>
                  <span style={{ width: '26px', height: '26px', borderRadius: '50%', display: 'grid', placeItems: 'center', background: step >= i + 1 ? 'var(--brand-yellow)' : 'var(--neutral-200)', color: 'var(--brand-black)' }}>{i + 1}</span>{s}
                </li>
              ))}
            </ol>

            {step === 1 ? (
              <div style={{ display: 'grid', gap: 'var(--space-5)' }}>
                <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 'var(--space-4)' }}>
                  <Input label="Rego" mono placeholder="ABC123" hint="We'll pull your make, model and VIN." />
                  <Select label="Make" options={['Audi', 'BMW', 'Mercedes-Benz', 'Mini', 'Skoda', 'Volkswagen']} />
                </div>
                <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 'var(--space-4)' }}>
                  <Input label="Model" placeholder="e.g. 320i Touring" />
                  <Input label="Odometer" placeholder="e.g. 148,000" suffix={<span style={{ font: 'var(--type-body-sm)' }}>km</span>} />
                </div>
                <div><Button onClick={() => setStep(2)} iconRight={<Icon name="arrow-right" size={20} />}>Continue</Button></div>
              </div>
            ) : step === 2 ? (
              <div style={{ display: 'grid', gap: 'var(--space-5)' }}>
                <div style={{ display: 'grid', gap: 'var(--space-3)' }}>
                  <Eyebrow>What's it in for?</Eyebrow>
                  {[['service', 'Scheduled service', 'Oil, filters, full inspection'], ['wof', 'WOF inspection', 'In and out the same day'], ['diag', 'Diagnostics / fault', "Tell us what it's doing"], ['dsg', 'DSG service', 'Audi, VW and Skoda']].map(([v, l, d]) => (
                    <Radio key={v} name="svc" label={l} description={d} checked={service === v} onChange={() => setService(v)} />
                  ))}
                </div>
                <div style={{ display: 'grid', gap: 'var(--space-3)' }}>
                  <Eyebrow>Pick a time — Thu 13 Aug</Eyebrow>
                  <div style={{ display: 'flex', gap: 'var(--space-2)', flexWrap: 'wrap' }}>
                    {SLOTS.map((s) => (
                      <button key={s} onClick={() => setSlot(s)} style={{
                        padding: '10px 16px', borderRadius: 'var(--radius-sm)', cursor: 'pointer',
                        border: '1px solid ' + (slot === s ? 'var(--brand-black)' : 'var(--border-default)'),
                        background: slot === s ? 'var(--brand-black)' : '#fff', color: slot === s ? '#fff' : 'var(--text-primary)',
                        font: 'var(--type-body-sm)', fontWeight: 'var(--fw-semibold)', transition: 'var(--transition-control)'
                      }}>{s}</button>
                    ))}
                  </div>
                </div>
                <Checkbox label="I'd like a courtesy car" description="Subject to availability — we'll confirm when we call." />
                <div style={{ display: 'flex', gap: 'var(--space-3)' }}>
                  <Button variant="ghost" onClick={() => setStep(1)}>Back</Button>
                  <Button onClick={() => setStep(3)} iconRight={<Icon name="arrow-right" size={20} />}>Continue</Button>
                </div>
              </div>
            ) : (
              <div style={{ display: 'grid', gap: 'var(--space-5)' }}>
                <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 'var(--space-4)' }}>
                  <Input label="Name" placeholder="Full name" />
                  <Input label="Phone" placeholder="021 000 0000" />
                </div>
                <Input label="Email" type="email" placeholder="you@example.co.nz" />
                <Input label="Anything we should know?" placeholder="Noise from the front left under braking…" />
                <Checkbox label="Email me service reminders" />
                <div style={{ display: 'flex', gap: 'var(--space-3)' }}>
                  <Button variant="ghost" onClick={() => setStep(2)}>Back</Button>
                  <Button onClick={() => setDone(true)}>Confirm Booking</Button>
                </div>
              </div>
            )}
          </div>

          <aside style={{ border: '1px solid var(--border-default)', borderRadius: 'var(--radius-md)', padding: 'var(--space-6)', display: 'grid', gap: 'var(--space-4)', position: 'sticky', top: '100px' }}>
            <Eyebrow>Your booking</Eyebrow>
            {[['Vehicle', 'BMW 320i Touring'], ['Rego', 'ABC123'], ['Service', service === 'service' ? 'Scheduled service' : service === 'wof' ? 'WOF inspection' : service === 'diag' ? 'Diagnostics' : 'DSG service'], ['When', 'Thu 13 Aug · ' + slot], ['Where', '23 Norris Ave, Te Rapa']].map(([k, v]) => (
              <div key={k} style={{ display: 'flex', justifyContent: 'space-between', gap: 'var(--space-4)', font: 'var(--type-body-sm)' }}>
                <span style={{ color: 'var(--text-muted)' }}>{k}</span><strong>{v}</strong>
              </div>
            ))}
            <hr style={{ border: 0, borderTop: '1px solid var(--border-default)', margin: 0 }} />
            <div style={{ display: 'flex', gap: 'var(--space-2)', flexWrap: 'wrap' }}>
              <Badge tone="success" dot>Courtesy cars available</Badge><Badge tone="neutral">MTA member</Badge>
            </div>
            <p style={{ font: 'var(--type-body-sm)', color: 'var(--text-muted)' }}>We'll never do extra work without your clear authorisation first.</p>
          </aside>
        </div>
      </Section>

      {done ? (
        <div style={{ position: 'fixed', right: 'var(--space-6)', bottom: 'var(--space-6)', zIndex: 200 }}>
          <Toast tone="success" title="Booking request sent" message="We'll call you on 021 000 0000 to confirm today." onDismiss={() => setDone(false)} icon={<Icon name="check" size={20} />} />
        </div>
      ) : null}
    </main>
  );
}

Object.assign(window, { BookingScreen });
