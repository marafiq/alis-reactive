# Source Of Truth

- Previous transcript:
  - [2026-03-31-session-transcript-continuation-02.md](./2026-03-31-session-transcript-continuation-02.md)
- Proof artifacts produced from this continuation:
  - [2026-03-31-end-state-schema-proof.md](./2026-03-31-end-state-schema-proof.md)
  - [2026-03-31-end-state-schema-proof-matrix.md](./2026-03-31-end-state-schema-proof-matrix.md)
  - [2026-03-31-end-state-reactive-plan.schema.json](./2026-03-31-end-state-reactive-plan.schema.json)

# 2026-03-31 Session Transcript Continuation 03

## Chronological Notes

### Trigger and partial-load pressure

- We re-checked the browser partial path and confirmed:
  - validation is lazily enriched today
  - component-event triggers are not lazily enriched
  - partial-injected component-event entries are wired immediately
- That forced one hard decision:
  - `ComponentEventTrigger` must be self-sufficient in the end-state schema

### Governing proof rule

- New rule locked:
  - running real use cases against the end-state plan schema is the only path forward
- If the end-state schema cannot narrate every real workflow with activity diagrams, the refactor plan is dead

### New architectural distinction

- Self-sufficient at wire time:
  - `ComponentEventTrigger`
  - external trigger attachments
- Lazy-resolvable through current registrations:
  - validation fields
  - `IncludeAll`

This split is the key to avoiding accidental runtime magic.

### Validation insight

- The earlier shape copied `fieldId`, `vendor`, `readExpr`, and `coerceAs` into validation fields
- The new end-state proof instead keeps validation fields pure:
  - `modelPath`
  - `rules`
- Runtime can resolve current components through the `components` map keyed by model path

This removes the need for lazy copied enrichment as a contract concern.

### Payload unification

- trigger payload and response payload are one source family:
  - `source.kind = payload`
  - `scope = trigger | response`
- This is what lets:
  - response success handlers read server payload
  - chained requests gather from previous success payload
  - payload mutation helpers consume response data without inventing a separate source model

### Component-event trigger decision

- Current shape is enough to wire today
- Current shape is not complete enough for the intended architecture
- End-state proof shape:
  - target attachment data
  - explicit payload mode
    - `none`
    - `callback`
    - `object`

This removes vendor-specific payload invention from runtime as the source of truth.

### Final schema direction

- end-state schema should not end on `readExpr` and `coerce`
- final emitted vocabulary now uses:
  - `value.path`
  - `shape`
  - `PlanValue`
  - `ValueAccess`
  - `payload.scope`
- `mutate-event` becomes `mutate-payload` in the proof contract because the mutation target is trigger payload, not a DOM event-specific concept

### Expert review folded in

- runtime review reinforced:
  - component-event triggers must remain self-sufficient for partials
  - request input, response payload, and chained continuity are one family
- descriptor / writer review reinforced:
  - compile-time placeholders are not emitted schema concepts
  - public validation input and public `BindSource` usage must be separated from internal semantics and emitted projection

## Outcome

This continuation produced:

- a final end-state schema candidate
- an exhaustive proof matrix
- typechecked TS proof fixtures for real workflow families

The stacked refactor plan should now be derived from these proof artifacts, not the other way around.
