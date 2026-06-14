export const meta = {
  name: 'onboard-fusion-component',
  description: 'Deterministic onboard/audit/upgrade of Fusion components: discover -> audit(member->core DSL primitive) -> observe(DSL wiring + Playwright assert per member) -> author strictly from the plan -> run Playwright -> verify-onboarding-complete (the 100% guarantee). Loops authoring until the gate exits 0 or fails loud. A component is NEVER "done" unless verify-onboarding-complete passes.',
  whenToUse: 'Onboard a new Fusion component, onboard all uncovered, or re-verify after an SF upgrade. Args: {component, fusionType} | {all:true} | (default) all currently-incomplete components.',
  phases: [
    { title: 'Select', detail: 'verify-onboarding-complete --all -> the incomplete target list' },
    { title: 'Onboard', detail: 'per component: observe-plan -> author per plan -> Playwright -> verify; loop until DONE or fail loud' },
  ],
}

const SK = '.claude/skills/onboard-fusion-component/scripts'
const MAX_AUTHOR_ROUNDS = 3

// --- Select the targets ----------------------------------------------------
phase('Select')
const single = args && args.component
  ? [{ component: args.component, fusionType: args.fusionType || ('Fusion' + pascal(args.component)) }]
  : null
const targets = single ?? await selectIncomplete()
log(`targets: ${targets.map(t => t.component).join(', ') || '(none — all already DONE)'}`)

// --- Onboard each, gate-guaranteed -----------------------------------------
phase('Onboard')
const results = []
for (const t of targets) {
  let round = 0, done = false, lastGaps = ''
  while (round < MAX_AUTHOR_ROUNDS && !done) {
    round++
    const r = await agent(AUTHOR(t, round, lastGaps), {
      label: `onboard:${t.component}#${round}`, phase: 'Onboard', schema: RESULT_SCHEMA,
    })
    done = !!(r && r.guaranteePassed)
    lastGaps = r ? (r.remainingGaps || '') : 'agent returned null'
    log(`${t.component} round ${round}: ${done ? 'DONE (verify-onboarding-complete exit 0)' : 'gaps -> ' + lastGaps.slice(0, 160)}`)
  }
  results.push({ component: t.component, done, rounds: round, remainingGaps: done ? '' : lastGaps })
}

const doneCount = results.filter(r => r.done).length
log(`onboarded ${doneCount}/${targets.length} to the 100% guarantee; ${results.length - doneCount} failed loud`)
return { doneCount, total: targets.length, results }

// --- helpers ---------------------------------------------------------------
async function selectIncomplete() {
  // an agent runs the TRX-INDEPENDENT audit per component and returns those with untested/unmapped
  // members (no authoring here). This targets genuinely-incomplete components, NOT ones that merely
  // show a stale-TRX 0b failure (those already have full coverage maps — a final full gate re-confirms).
  const r = await agent(
    `For every directory C under tools/FusionOnboarding/wwwroot/onboarding/fusion (excluding names starting with _), run:\n` +
    `  node ${SK}/audit-primitive-coverage.mjs --component C --fusion-type Fusion<PascalCase of C>\n` +
    `Return the components whose output shows ANY "UNMAPPED" or "UNTESTED" members (i.e. not 100% mapped+covered-in-map), ` +
    `each with its Fusion type. Do NOT edit anything, do NOT run git. Fusion type of "x-y" is "FusionXY".`,
    { label: 'select-incomplete', phase: 'Select', schema: {
      type: 'object', required: ['incomplete'], properties: {
        incomplete: { type: 'array', items: { type: 'object', required: ['component', 'fusionType'],
          properties: { component: { type: 'string' }, fusionType: { type: 'string' } } } } } } })
  return (r && r.incomplete) || []
}

function AUTHOR(t, round, gaps) {
  return `Onboard the Fusion component "${t.component}" (${t.fusionType}) for Alis.Reactive to the 100% guarantee. ` +
`Senior-living UI framework; the domain names the stakes, NOT the scope — no HIPAA/PHI/compliance reasoning. ` +
`Report each step in past tense with the command output.\n\n` +
`DETERMINISTIC SPEC — do not improvise, do not invent a taxonomy, do not exclude a public typed member, never conclude ` +
`"framework defect" (the core DSL is correct; its one limitation is no await-Promise primitive). The core DSL is read-only; ` +
`edit ONLY the sandbox (model/view/controller) for this component, its Playwright test, its test-infra locator, and its ` +
`artifact tree. Do NOT run git.\n\n` +
`STEPS:\n` +
`1. If discovery is missing, generate it: read the Fusion${pascal(t.component)} slice + the EJ2 d.ts it wraps, then ` +
`   node ${SK}/write-fusion-discovery-artifacts.mjs --component ${t.component} --class <EjClass> --namespace <ej.ns> --dts <d.ts> --xml ~/.nuget/packages/syncfusion.ej2.aspnet.core/32.2.8/lib/net10.0/Syncfusion.EJ2.xml --write\n` +
`2. node ${SK}/audit-primitive-coverage.mjs --component ${t.component} --fusion-type ${t.fusionType}  (must be 100% mapped)\n` +
`3. node ${SK}/observe-plan.mjs --component ${t.component} --fusion-type ${t.fusionType}  — this is your EXACT authoring spec. ` +
`   For EVERY member it lists, wire it into a visible outcome in the sandbox view per its primitive (Read scalar -> ` +
`   p.Element(id).SetText(args,x=>x.M); Read bool -> p.When(args,x=>x.M).Truthy().Then(SetText).Else(SetText); Set/Call -> ` +
`   NativeButton Click reaction; Event -> .Reactive(plan, evt=>evt.E, ...)). Use the rating/numeric-text-box committed slices ` +
`   as the pattern. Proper domain model; logical vertical slice (nest only if the component is complex). Real-app view only — ` +
`   no echo/debug spans, no plan-JSON panel.\n` +
`4. Write a Playwright test PER member that performs the real gesture and asserts the observed outcome — fails-when-broken ` +
`   (BDD Rule 3), real interactions only, no page.evaluate (the one exception: a framework-gather request.PostData assert).\n` +
`5. Build + run ONLY this component: scripts/playwright.sh --filter "FullyQualifiedName~...Fusion.${t.fusionType.replace('Fusion','')}." \n` +
`6. node ${SK}/verify-onboarding-complete.mjs --component ${t.component} --fusion-type ${t.fusionType}\n` +
`   Iterate steps 3-6 until it EXITS 0. That gate is the only definition of done; do NOT report done otherwise.\n` +
(gaps ? `\nPREVIOUS ROUND LEFT THESE GAPS — close exactly these (each is a member needing its DSL wiring + a passing fails-when-broken test): ${gaps}\n` : '') +
`\nReturn: guaranteePassed (true ONLY if verify-onboarding-complete printed "DONE — 100% onboarded, guaranteed." / exit 0), ` +
`remainingGaps (the exact members still UNMAPPED/UNTESTED if not), and the verify command's final line.`
}

const RESULT_SCHEMA = { type: 'object', additionalProperties: false,
  required: ['guaranteePassed', 'remainingGaps', 'verifyFinalLine'],
  properties: { guaranteePassed: { type: 'boolean' }, remainingGaps: { type: 'string' }, verifyFinalLine: { type: 'string' } } }

function pascal(s) { return s.split('-').map(p => p[0].toUpperCase() + p.slice(1)).join('') }
