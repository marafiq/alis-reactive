# Bring your own component: release plan

Status: release plan on `feature/component-redesign` (PR #177). Verification: Playwright only.
Behavior contract: [`capability-catalog.md`](capability-catalog.md).

## Goal

A developer takes a dependency on Alis.Reactive and onboards any component that already works in
the browser. They install, bundle and construct it; the framework locates its JS object and brings
typed access to its properties, methods and events, through the same fluent API every framework
feature already speaks. The framework becomes simpler and more stable by inverting the dependency:
today the core knows specific components and vendors and our slices reach into core internals;
after this release the core defines the abstractions and every component (Fusion, Native,
app-level, brought) depends only on its public surface.

## Decisions

Owner:
1. The app constructs and destroys its widgets; the framework only locates the object.
2. Syncfusion keeps the `Fusion` prefix; members and events use the component's own JS/MVC names.
3. Playwright is the only verification; one component, one vertical slice at a time.

Taken for this release (reversible):
4. No break to the public authoring API consumers use today. Per-member methods (`.SetValue()`,
   `.Increment()`, `evt => evt.Changed` where it exists) stay as one-line delegates over the new
   surface; removing them is a separate, later release.
5. When an object is not on its element, the runtime logs one error naming the element and the
   library, skips that behavior, and keeps wiring the rest. No silent deferral.
6. `alis:mount` and `alis:unmount` events and the `data-alis-component` marker are the public
   contract between the runtime and app bundles.
7. `vendor` becomes `library` in the plan and in C#.

## The model

The unit is a typed JS object. The plan and runtime already work this way (components and plugins
resolve to `RuntimeObject` through the same contracts); the release makes C# authoring match.

- **One declaration vocabulary.** `JsObject` owns protected factories: `Property<T>("name")`,
  `Method("name")` and `Method<TArg..>`, `Function<TReturn>("name")` and
  `Function<TArg.., TReturn>`, `Event<TPayload>("name")`. Names are the JS API's; paths may nest
  (`event.setProp`). Member types: `JsProperty<T>`, `JsMethod`, `JsFunction<..>`, `JsEvent<T>`.
- **Where the object lives is the only difference.** `Component : JsObject` is found from its
  framework-rendered element through its library (Fusion: `ej2_instances.0`; Native: the element;
  a brought library: its own path). `InputComponent<TValue> : Component` adds a value (property or
  getter/setter methods, default `value`) and a change event (default `change`). A payload object
  is the event's argument. A plugin is found by name in the catalog.
- **One verb set on every handle.** `Set(m, literal | TypedSource<T>)`, `Call(m, args…)` where each
  argument is a literal or a `TypedSource`, and `Get(m)` for properties and functions returning
  `TypedSource<T>`. Every existing source flows in: another component, a payload read, a response
  read (`res.Read(r => r.Items)`), URL, plugin, an array result.
- **Payloads.** Data stays plain C# properties read with today's grammar (`p.When(args, x => x.Text)`,
  `FromEvent(args, x => x.Text)`); behavior is declared members used through
  `p.Payload(args).Set(a => a.Cancel, true)` and `.Call(a => a.UpdateData, res.Read(r => r.Items))`.
  Reactions stay synchronous so writes land before the vendor reads them.
- **Libraries are data.** A library declares how to find its object from the element, how to
  subscribe and unsubscribe, and how each event's payload is read. The plan carries `libraries`;
  the runtime has one generic driver and no vendor code.
- **The framework dictates** IDs (`IdGenerator`, explicit ids, fixed app-level ids), `name` as the
  binding path, the `{id}_error` element, no JavaScript in views, and the trusted-plan rule.

## Phases

Each phase lands as one or more PRs, each slice proven by its Playwright suites (targeted while
working, full suite before merge), and leaves `main` releasable.

### Phase 0: make the proof trustworthy

The release's only verification has holes (catalog §4); refactoring on false greens is the largest
risk, so this comes first and changes no framework code.
1. Fix false greens: HttpMixing else-path tests use real gestures and camelCase payloads; the
   ArrayManager send test gathers the real array; `FromEvent` tests assert the query value;
   ColorPicker toggle asserts the popup; the 422 summary is read from the response.
2. Turn plan-only fixtures into browser journeys where they pin user-visible behavior
   (component-method sources in gather and conditions, URL and plugin sources in conditions,
   event-payload gather, capitalised members read from responses); delete the pure plan-shape pins.
3. Add browser proofs for what the release touches and nothing proves: Fusion live revalidation on
   change (NumericTextBox, DatePicker), InPlaceEditor BeginEdit cancel, rules removed when a slot
   unloads, two partials merging into one plan, a Grid returning method into arrays and a condition.

Proof: every fixed or added test is seen red once with its capability broken, then green.

### Phase 1: one public SDK for JS objects

C# only; plan JSON and runtime unchanged.
- Introduce the vocabulary and verbs above; rename slice 1's `ComponentProperty`/`ComponentMethod`
  into it.
- Move slices onto it, payload-heavy first because they prove payload writes and calls:
  AutoComplete and MultiSelect filtering, Grid editing events, Kanban; then inputs; Grid's
  remaining surface; Native.
- Remove `InternalsVisibleTo` for Fusion and Native.

Proof: each slice's suites; full suite at the end. Exit: no slice references `ReactionGraph`,
`PayloadSource`, `EmitSet`, `EmitCall` or `Read`; Fusion and Native compile without
`InternalsVisibleTo`.

### Phase 2: inputs come from their declaration

- The input binding carries the value accessor (property or methods) and the change event; gather
  (`IncludeAll`, `Include<T>`), validation reads and live clear use them; `live-clear.ts` loses the
  hard-coded `"change"`.
- Delete `RegisteredInputValueRead.ForFusionInPlaceEditorValueRead`, the registration profiles and
  kind tokens, and the `"value"` fallback alias; `NativeGatherExtensions` stops assuming NativeTextBox.

Proof: Validation (Contract, RoomRate, FallRiskScreening, Rules), HttpPipeline (OutingRoster,
MedicationOrder, MedicationPass, CarePlanLookup), InPlaceEditor suites, Phase 0 Fusion live
revalidation; `plan.ts` regenerated.

### Phase 3: libraries as data

- Plan `libraries` map; one generic driver: locate by path from the element, subscribe and
  unsubscribe by declared method names, payload selector per event (Fusion: first argument, `{}`
  when absent; Native: `currentTarget`, or `detail` for a CustomEvent).
- Delete both drivers, both event adapters, `resolver.ts` and the silent readiness deferral
  (decision 5).

Proof: full suite on CI's five shards.

### Phase 4: lifecycle and partials

- The runtime raises `alis:mount {root}` before wiring, at boot and after each injection, and
  `alis:unmount {root}` before a slot is replaced or closed.
- Injection re-activates inline scripts itself instead of calling `ej.base.append`; first prove
  what Syncfusion's partial scripts need (the Schedule drawer uses `ScriptManager`).
- The Native RadioGroup and CheckList scripts move into Native's shipped module run on mount, so
  views carry no JavaScript and `SetValue` syncs the boxes.
- Closing the drawer unloads its slot.

Proof: Drawer suites (exact request counts), Contract partial suites, Stepper wizard, Schedule
drawer, AdmissionWizard, RealTime panel, ContentType injection with interactive inputs, Phase 0
slot-unload proof.

### Phase 5: remaining leaks

- FileUpload declares its file value, so `rawFile` leaves the payload writer.
- Confirm becomes an app-level object reached through the plan; conditions stop reading
  `window.alis.confirm`; app-level modules leave `root.ts`.
- Method contracts are keyed per declaration with overloads by arity (fixes the latent
  `addRecord` conflict).

Proof: FileUpload, Guards Confirm, ShiftNote confirm-in-drawer, Grid and AppLevel suites. Exit:
core C# and runtime name no vendor or component.

### Phase 6: brought components

Tom Select and FullCalendar are installed in the sandbox the way an app would install them,
declared in the sandbox assembly (no `InternalsVisibleTo`) and constructed on `alis:mount`.

Proof: one Activities planner journey.
- Clicking an activity reads `event.id` from the payload and calls `event.setProp('title', …)`
  with a value from the response.
- A GET by route param from the payload sets a FusionNumericTextBox (model expression) and Tom
  Select (method-backed value); a chained GET and parallel GETs follow; when all settle a Toast
  (app-level) shows; `getEvents()` feeds a count.
- Tom Select is required: the error shows in `{id}_error` and clears on its change; submitting
  posts both values.
- The form loads as a partial; unloading leaves no orphan dropdown.

## Release exit criteria

- CI green on the release commit: `gate / test`, five Playwright shards, `verify-net48`.
- Fusion and Native build without `InternalsVisibleTo`.
- Core C# and runtime name no vendor or component.
- The brought-component journey is green.
- Consumer authoring API unchanged: `examples/resident-intake` builds and its views are untouched.

## Risks

- Changing every plan in Phase 3: the full suite is the gate; land Phase 3 alone.
- `ej.base.append` may be what runs Syncfusion and Native scripts in partials: prove before removing.
- Payload writes depend on the synchronous lane: the generic driver keeps listeners synchronous.
- Fusion objects that are not ready at wiring time are skipped silently today; decision 5 makes
  them loud, which can surface latent ordering bugs; the partial suites are the gate.

## Done so far

- Lean `CLAUDE.md`, onboarding tooling and stale files removed, hooks fixed.
- Slice 1: FusionNumericTextBox on a public declaration surface (`f0ec4f5`); full suite and net48
  green on CI.
- Capability catalog from all 176 suites (`2c37ab5`).
