# Issue #86 Principles And Why This Schema Is Better

## Architectural Principles

1. One-way value flow governs everything: resolve a root, apply access steps,
   get a raw JS value, shape if needed, then consume.
2. Trigger attachment and trigger payload are separate concerns and must be
   explicit in the schema.
3. Component surface identity and binding participation are different concerns;
   one component registry may hold both, but they are not the same noun.
4. Binding is optional and narrow: it exists only when a component can declare a
   self-sufficient canonical semantic value; current proof coverage for this is
   centered on readable bound components.
5. `Request` is a first-class unit and owns its own stages.
6. Pipeline order is first-class; the schema must preserve declaration order
   instead of flattening behavior into a single structural stage.
7. Validation stays as targets, rules, and conditions; runtime lookup details do
   not belong in the emitted validation contract.
8. Scope crossing happens only through explicit `dispatch`.
9. Partial lifecycle is honest: some trigger roots are self-sufficient at wire
   time, while binding-driven consumers resolve lazily against the latest merged
   component registry.
10. Runtime stays tiny: resolve root, execute access, shape if needed, then
    perform `set` or `call`.
11. Extension happens through vertical slices contributing explicit component
    semantics, not through reflective escape hatches.
12. Hidden schema evolution is allowed; public DSL churn is not the default.

## Why This Schema Is Better

### Fewer Concepts

- `entries` becomes `reactions`
- stitched request leftovers are collapsed back into one `Request` unit
- one component registry replaces the old split-brain between value registration
  thinking and generic component surfaces
- one shared value/access language replaces separate ad hoc shapes for gather,
  command values, dispatch payloads, and response reads

### Clearer Ownership

- `Component` owns resolvable surface identity
- `Binding` owns canonical semantic participation
- `Trigger` owns attachment plus carried payload contract
- `Request` owns transport stages
- `Validation` owns rules only
- `Access` owns reading
- `Mutation` owns effects

That removes overlapping meaning and keeps each schema object narrow.

### Better SOLID Separation

- registration no longer carries downstream consumer semantics by accident
- validation no longer carries copied lookup/enrichment data
- request input semantics are not modeled differently from command and guard
  value semantics
- the final schema carries trigger payload shape explicitly instead of leaving
  it to runtime invention

### Better Testability

The architecture now has clean proof seams:

- TS runtime execution proof through shared native/fusion proof surfaces
- C# lowering proof through isolated DSL-surface tests
- request-unit, merge-lifecycle, and trigger-order proofs as independent
  checkpoints

That lets us test:

- DSL -> schema lowering
- schema -> runtime execution
- lifecycle behavior

without forcing every test through one giant end-to-end fixture.

### Better Extension Surface

A new vertical slice should only need to declare:

- how its component root resolves
- whether it participates in binding
- how its canonical semantic value is accessed
- which typed DSL methods lower to `set` or `call`
- which trigger payload it explicitly emits

That is a much cleaner extension story than teaching the runtime new special
cases.

### Better Alignment With Real JS Semantics

The actual browser-side interaction algebra is small:

- read members
- invoke methods
- set properties
- call methods
- subscribe to events

This schema matches that directly instead of pretending the framework exposes
arbitrary reflection.

### Better Path To A Dumber Runtime

The end-state runtime does not need to infer meaning from:

- `componentType`
- duplicated read/coerce metadata in validation DTOs
- invented native trigger payload shapes
- transport-specific value lanes

It only needs to execute explicit schema instructions.
